using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Collections;
using ThisMember.Core.Exceptions;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using ThisMember.Core.Options;
using ThisMember.Core.Misc;
using ThisMember.Extensions;

namespace ThisMember.Core
{
  /// <summary>
  /// Generates an expression from all members that need to be mapped
  /// and then compiles the expression to a dynamic method.
  /// </summary>
  /// <remarks>This class is not thread safe and an instance cannot be shared by multiple threads.</remarks>
  internal class CompiledMapGenerator : IMapGenerator
  {

    private readonly IMemberMapper mapper;
    private ParameterExpression sourceParameter;
    private ParameterExpression destinationParameter;
    private readonly MapProposalProcessor mapProcessor;
    private IList<ParameterExpression> newParameters;
    private readonly ProposedMap proposedMap;
    private readonly MapperOptions options;
    private IList<IndexedParameterExpression> Parameters { get; set; }

    public DebugInformation DebugInformation { get; private set; }

    public CompiledMapGenerator(IMemberMapper mapper, ProposedMap map, MapperOptions options)
    {
      this.mapper = mapper;
      this.proposedMap = map;
      this.mapProcessor = new MapProposalProcessor(mapper);
      this.newParameters = new List<ParameterExpression>();
      this.options = options;
    }

    private int currentID = 0;

    private string GetParameterName(PropertyOrFieldInfo member)
    {
      return member.Name;
    }

    private static readonly MethodInfo anyMethod;

    static CompiledMapGenerator()
    {
      anyMethod = (from m in typeof(Enumerable).GetMember("Any", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public)
                   let method = m as MethodInfo
                   where method != null && method.GetParameters().Length == 1
                   select method).Single();
    }

    private Dictionary<Type, Stack<ParameterExpression>> paramCache = new Dictionary<Type, Stack<ParameterExpression>>();

    /// <summary>
    /// Gets a reusable parameter for a certain type and with a certain name influenced by <param name="purpose">purpose</param>.
    /// Property obtained must later returned to the cache when done with it by calling ReleaseParameter.
    /// </summary>
    /// <returns></returns>
    private ParameterExpression ObtainParameter(Type type, string purpose = null)
    {
      Stack<ParameterExpression> parameters;

      if (!paramCache.TryGetValue(type, out parameters))
      {
        parameters = new Stack<ParameterExpression>();
        paramCache.Add(type, parameters);
      }

      if (parameters.Count == 0)
      {
        var paramExpr = Expression.Parameter(type, GetParamName(type, purpose));
        parameters.Push(paramExpr);
        newParameters.Add(paramExpr);
      }

      return parameters.Pop();
    }

    private void ReleaseParameter(ParameterExpression param)
    {
      paramCache[param.Type].Push(param);
    }

    private int intCounter = 0;
    private const string validIntNames = "ijklmnopqrstuvwxyzabcdefghABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string GetParamName(Type t, string purpose)
    {

      if (!string.IsNullOrEmpty(purpose))
      {
        return purpose + "#" + currentID++;
      }

      if (typeof(int) == t)
      {
        return new String(validIntNames[(intCounter++) % validIntNames.Length], 1);
      }

      if (CollectionTypeHelper.IsEnumerable(t))
      {
        return "collection#" + currentID++;
      }

      if (typeof(IEnumerator).IsAssignableFrom(t))
      {
        return "enumerator#" + currentID++;
      }

      return new string(Char.ToLower(t.Name[0]), 1) + t.Name.Substring(1) + "#" + currentID++;
    }

    /// <summary>
    /// Processes a mapping for a type.
    /// </summary>
    private void BuildTypeMappingExpressions
    (
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping typeMapping,
      List<Expression> expressions,
      CustomMapping customMapping = null
    )
    {
      // If there's custom mapping defined
      if (customMapping != null)
      {
        // Custom mapping has a source parameter that needs to be replaced with actual parameter
        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(customMapping.SourceParameter, source));

        // Custom mapping may contain more parameters that represent arguments in the custom mapping
        foreach (var param in customMapping.ArgumentParameters)
        {
          // Find the corresponding parameter that we want to replace the custom mapping's placeholder with
          var correspondingParam = this.Parameters.Single(p => p.Index == param.Index);
          mapProcessor.ParametersToReplace.Add(new ExpressionTuple(param.Parameter, correspondingParam.Parameter));
        }
      }

      // Simple property assignments
      foreach (var member in typeMapping.ProposedMappings)
      {
        BuildMemberAssignmentExpressions(source, destination, member, expressions, typeMapping, customMapping);
      }

      // Nested type mappings
      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {
        // If it's a collection type
        if (typeMapping.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexTypeMapping))
        {
          BuildEnumerableMappingExpressions(source, destination, complexTypeMapping, expressions);
        }
        else
        {
          // If it's not a collection but just a nested type
          BuildComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions);
        }
      }
    }


    /// <summary>
    /// Assigns an expression that can be pretty much anything to a destination mapping.
    /// </summary>
    /// <returns></returns>
    private BinaryExpression AssignSimpleProperty
    (
      ProposedMemberMapping member,
      MemberExpression destination,
      Expression source,
      ProposedTypeMapping typeMapping,
      bool usesConversionFunction
    )
    {
      // If the destination is nullable but the source expression returns a non-nullable value type.
      if (destination.Type.IsNullableValueType() && !source.Type.IsNullableValueType())
      {
        source = HandleDestinationNullableValueType(destination, source);
      }
      // If the destination is not a nullable value type but the source expression does return one.
      else if (!destination.Type.IsNullableValueType() && source.Type.IsNullableValueType())
      {
        source = HandleSourceNullableValueType(destination, source);
      }
      // dest.Member = source.Member != null ? source.Member : dest.Member 
      else if (source.Type.IsClass && options.Conventions.MakeCloneIfDestinationIsTheSameAsSource
        && source.Type.IsAssignableFrom(destination.Type)
        && !typeMapping.NewInstanceCreated
        && !usesConversionFunction)
      {
        source = Expression.Condition(Expression.NotEqual(source, Expression.Constant(null)), source, destination);
      }
      else if (destination.Type.IsNullableValueType() && source.Type.IsNullableValueType())
      {
        source = HandleNullableValueTypes(destination, source);
      }

      if (!source.Type.IsAssignableFrom(destination.Type))
      {
        // cast
        source = Expression.Convert(source, destination.Type);
      }

      return Expression.Assign(destination, source);
    }

    private Expression HandleNullableValueTypes(MemberExpression destination, Expression source)
    {
      // If the source is null then ignore it if option is turned on, preserving the 
      // original value.
      Expression elseClause = options.Conventions.IgnoreMembersWithNullValueOnSource ?
      (Expression)destination : Expression.Default(source.Type);

      // Depending on the above option this can either be
      // source.Member.HasValue ? source.Member.Value : default(T)
      // OR source.Member.HasValue ? source.Member.Value : dest.Member
      source = Expression.Condition(Expression.IsTrue(Expression.Property(source, "HasValue")), Expression.Convert(Expression.Property(source, "Value"), source.Type), elseClause);
      return source;
    }

    private Expression HandleSourceNullableValueType(MemberExpression destination, Expression source)
    {
      var nullableType = source.Type.GetGenericArguments().Single();

      // If the source is null then ignore it if option is turned on, preserving the 
      // original value.
      Expression elseClause = options.Conventions.IgnoreMembersWithNullValueOnSource ?
      (Expression)destination : Expression.Default(nullableType);

      // Depending on the above option this can either be
      // source.Member.HasValue ? source.Member.Value : default(T)
      // OR source.Member.HasValue ? source.Member.Value : dest.Member
      source = Expression.Condition(Expression.IsTrue(Expression.Property(source, "HasValue")), Expression.Property(source, "Value"), elseClause);
      return source;
    }

    private static Expression HandleDestinationNullableValueType(MemberExpression destination, Expression source)
    {
      var nullableType = destination.Type.GetGenericArguments().Single();

      if (!source.Type.IsAssignableFrom(nullableType))
      {
        source = Expression.Convert(source, nullableType);
      }

      // new Nullable<T>(source.Member)
      source = Expression.New(destination.Type.GetConstructor(new[] { nullableType }), source);
      return source;
    }

    /// <summary>
    /// Assign source member to a destination mapping, applying any custom mappings in the process.
    /// </summary>
    private void BuildMemberAssignmentExpressions
    (
      ParameterExpression source,
      ParameterExpression destination,
      ProposedMemberMapping member,
      List<Expression> expressions,
      ProposedTypeMapping typeMapping,
      CustomMapping customMapping = null
    )
    {
      if (member.Ignored)
      {
        return;
      }

      Expression condition = null;

      if (member.Condition != null)
      {
        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(member.Condition.Parameters.Single(), this.sourceParameter));

        condition = member.Condition.Body;
      }

      var destMember = Expression.MakeMemberAccess(destination, member.DestinationMember);

      Expression assignSourceToDest = null;

      Expression customExpression = null;

      Expression assignConversionToDest = null;

      LambdaExpression conversionFunction = null;

      bool conversionReturnTypeSameAsParameterType = false;

      ParameterExpression conversionParameter = null;

      if (customMapping != null)
      {
        conversionFunction = customMapping.GetConversionFunction(member.SourceMember, member.DestinationMember);

        if (conversionFunction != null)
        {
          conversionParameter = conversionFunction.Parameters.Single();

          conversionReturnTypeSameAsParameterType = conversionParameter.Type == conversionFunction.ReturnType;
        }
      }

      if (customMapping != null && (customExpression = customMapping.GetExpressionForMember(member.DestinationMember)) != null)
      {
        assignSourceToDest = Expression.Assign(destMember, customExpression);
      }
      else if (member.HierarchicalMapping != null)
      {
        var accessHierarchy = BuildHierarchicalExpression(source, member.HierarchicalMapping, null);

        assignSourceToDest = Expression.Assign(destMember, accessHierarchy);
      }
      else
      {
        Expression sourceExpression = Expression.MakeMemberAccess(source, member.SourceMember);

        bool usesConversionFunction = false;

        if (conversionFunction != null && !conversionReturnTypeSameAsParameterType)
        {
          ValidateConversionFunction(conversionFunction, conversionParameter, sourceExpression, destMember);

          this.mapProcessor.ParametersToReplace.Add(new ExpressionTuple(conversionParameter, sourceExpression));

          sourceExpression = conversionFunction.Body;
          usesConversionFunction = true;
        }

        assignSourceToDest = AssignSimpleProperty(member, destMember, sourceExpression, typeMapping, usesConversionFunction);
      }

      if (conversionFunction != null && conversionReturnTypeSameAsParameterType)
      {
        this.mapProcessor.ParametersToReplace.Add(new ExpressionTuple(conversionParameter, destMember));

        assignConversionToDest = Expression.Assign(destMember, conversionFunction.Body);
      }

      // If a condition to the mapping was specified
      if (condition != null)
      {
        if (assignConversionToDest != null)
        {
          assignSourceToDest = Expression.Block(assignSourceToDest, assignConversionToDest);
        }

        var ifCondition = Expression.IfThen(condition, assignSourceToDest);
        expressions.Add(ifCondition);
      }
      else
      {
        expressions.Add(assignSourceToDest);
        if (assignConversionToDest != null)
        {
          expressions.Add(assignConversionToDest);
        }
      }
    }

    private void ValidateConversionFunction(LambdaExpression conversionFunction, ParameterExpression conversionParameter, Expression sourceExpression, Expression destinationExpression)
    {
      if (!conversionParameter.Type.IsAssignableFrom(sourceExpression.Type))
      {
        throw new InvalidOperationException(
          string.Format("Cannot use conversion function accepting parameter {0} and returning {1} with parameter of type {2}",
          conversionParameter.Type, conversionFunction.ReturnType, sourceExpression.Type));
      }
      else if (!destinationExpression.Type.IsAssignableFrom(conversionFunction.ReturnType))
      {
        throw new InvalidOperationException(
          string.Format("Cannot use a conversion function returning {0} and assign it to type {1}", conversionFunction.ReturnType, destinationExpression.Type));
      }
    }

    private static Expression BuildHierarchicalExpression(ParameterExpression sourceParam, ProposedHierarchicalMapping mapping, Expression expression)
    {

      foreach (var member in mapping.Members)
      {
        if (expression == null)
        {
          expression = Expression.MakeMemberAccess(sourceParam, member);
        }
        else
        {
          expression = Expression.MakeMemberAccess(expression, member);
        }

      }

      return expression;
    }

    private bool IsClone
    {
      get
      {
        return this.options.Conventions.MakeCloneIfDestinationIsTheSameAsSource
          && this.proposedMap.SourceType == this.proposedMap.DestinationType;
      }
    }

    private class EnumerableMappingFacts
    {
      public Type DestinationEnumerableType { get; set; }

      public Type SourceEnumerableType { get; set; }

      public Type DestinationElementType { get; set; }

      public Type SourceElementType { get; set; }

      public bool CanAssignSourceElementsToDestination { get; set; }

      public bool SourceIsDictionaryType { get; set; }

      public bool DestinationIsDictionaryType { get; set; }

      public bool SourceIsArray { get; set; }

      public bool SourceIsCollection { get; set; }

      public bool SourceIsIEnumerable { get; set; }

      public bool DestinationIsArray { get; set; }

      public bool PreserveDestinationContents { get; set; }

      public bool SourceIsList { get; set; }

      public bool CanAssignSourceToDestination { get; set; }

      public bool DestinationCouldBeArray { get; set; }
    }

    private EnumerableMappingFacts FindEnumerableMappingFacts(
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping complexTypeMapping)
    {
      var facts = new EnumerableMappingFacts();


      if (complexTypeMapping.SourceMember != null)
      {
        facts.SourceEnumerableType = complexTypeMapping.SourceMember.PropertyOrFieldType;
      }
      else
      {
        facts.SourceEnumerableType = source.Type;
      }

      if (complexTypeMapping.DestinationMember != null)
      {
        facts.DestinationEnumerableType = complexTypeMapping.DestinationMember.PropertyOrFieldType;
      }
      else
      {
        facts.DestinationEnumerableType = destination.Type;
      }

      facts.DestinationElementType = CollectionTypeHelper.GetTypeInsideEnumerable(facts.DestinationEnumerableType);

      facts.SourceElementType = CollectionTypeHelper.GetTypeInsideEnumerable(facts.SourceEnumerableType);

      facts.CanAssignSourceElementsToDestination = CanAssignSourceElementToDestination(complexTypeMapping, facts.DestinationElementType, facts.SourceElementType);

      // If we're working with a Dictionary as the source
      if (facts.SourceElementType.IsGenericType
        && typeof(KeyValuePair<,>).IsAssignableFrom(facts.SourceElementType.GetGenericTypeDefinition()))
      {
        facts.SourceIsDictionaryType = true;
      }

      // If we're working with a Dictionary as the destination
      if (facts.DestinationElementType.IsGenericType
        && typeof(KeyValuePair<,>).IsAssignableFrom(facts.DestinationElementType.GetGenericTypeDefinition()))
      {
        facts.DestinationIsDictionaryType = true;
      }

      facts.SourceIsArray = facts.SourceEnumerableType.IsArray;

      var genericCollection = typeof(ICollection<>).MakeGenericType(facts.SourceElementType);

      if (genericCollection.IsAssignableFrom(facts.SourceEnumerableType))
      {
        facts.SourceIsCollection = true;
      }
      else
      {
        facts.SourceIsIEnumerable = true;
      }

      facts.DestinationIsArray = facts.DestinationEnumerableType.IsArray;

      if (options.Conventions.PreserveDestinationListContents
          && IsCollectionType(facts.DestinationEnumerableType)
          && !facts.DestinationIsArray)
      {
        facts.PreserveDestinationContents = true;
      }

      if (IsListType(facts.SourceEnumerableType))
      {
        facts.SourceIsList = true;
      }

      if (facts.DestinationEnumerableType.IsAssignableFrom(facts.SourceEnumerableType))
      {
        facts.CanAssignSourceToDestination = true;
      }

      if (facts.DestinationEnumerableType.IsInterface)
      {
        facts.DestinationCouldBeArray = !facts.DestinationIsDictionaryType;
      }
      else if (facts.DestinationIsArray)
      {
        facts.DestinationCouldBeArray = true;
      }

      return facts;
    }

    private static readonly MethodInfo referenceEqualsMethod = typeof(object).GetMethod("ReferenceEquals");

    private static readonly MethodInfo countMethod = (from m in typeof(Enumerable).GetMethods()
                                                      where m.Name == "Count" && m.IsGenericMethod
                                                      && m.GetParameters().Length == 1
                                                      select m).FirstOrDefault();

    /// <summary>
    /// Generates the loop that maps any IEnumerable type
    /// </summary>
    private void BuildEnumerableMappingExpressions
    (
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping complexTypeMapping,
      List<Expression> expressions
    )
    {
      // Find out the properties of this mapping which we will use to generate code
      var facts = FindEnumerableMappingFacts(source, destination, complexTypeMapping);

      // Early return because we're ignoring this mapping
      if (complexTypeMapping.Ignored)
      {
        return;
      }

      var ifNotNullBlock = new List<Expression>();

      Type destinationCollectionType;
      ParameterExpression destinationCollection;

      // If SourceMember is null, it means that the root type that is being mapped is enumerable itself.
      var accessSourceCollection = complexTypeMapping.SourceMember != null ?
        (Expression)Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember) : source;

      var accessSourceEnumerableSize = GetEnumerableSizeAccessor(facts, accessSourceCollection);

      // destination.Collection = newCollection OR return destination, if destination is enumerable itself
      Expression accessDestinationCollection = complexTypeMapping.DestinationMember != null
        ? (Expression)Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember)
        : destination;

      if (facts.CanAssignSourceToDestination && !facts.PreserveDestinationContents && !this.IsClone)
      {
        var assignSourceToDest = Expression.Assign(accessDestinationCollection, accessSourceCollection);
        expressions.Add(assignSourceToDest);
        return; // Early exit, nothing else to do but this simple assignment
      }

      // If it's an array, create a new array destination type
      if (facts.DestinationIsArray)
      {
        destinationCollectionType = facts.DestinationEnumerableType;

        //destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        destinationCollection = ObtainParameter(destinationCollectionType);

        //newParameters.Add(destinationCollection);

        var createDestinationCollection = Expression.New(destinationCollectionType.GetConstructors().Single(), accessSourceEnumerableSize);

        // destination = new DestinationType[source.Length/Count/Count()]
        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }
      else
      {
        // if it's not a Dictionary type, use List as the container to create
        if (!facts.DestinationIsDictionaryType)
        {
          destinationCollectionType = typeof(List<>).MakeGenericType(facts.DestinationElementType);
        }
        else
        {
          destinationCollectionType = facts.DestinationEnumerableType;
        }

        Expression assignListTypeToParameter;

        var createDestinationCollection = Expression.New(destinationCollectionType);

        // If it's an IList but not an array we want to check if the destination property isn't null
        // and if it isn't, we want to reuse it.
        if (facts.PreserveDestinationContents)
        {
          Expression reuseCondition;

          if (facts.DestinationCouldBeArray && options.Safety.EnsureCollectionIsNotArrayType)
          {
            reuseCondition = Expression.And(Expression.NotEqual(accessDestinationCollection, Expression.Constant(null)),
              Expression.IsFalse(Expression.TypeIs(accessDestinationCollection, facts.DestinationElementType.MakeArrayType())));
          }
          else
          {
            reuseCondition = Expression.NotEqual(accessDestinationCollection, Expression.Constant(null));
          }

          assignListTypeToParameter = Expression.Condition(reuseCondition,
            accessDestinationCollection,
            Expression.Convert(createDestinationCollection, accessDestinationCollection.Type));

          destinationCollection = ObtainParameter(accessDestinationCollection.Type);
        }
        else
        {
          assignListTypeToParameter = createDestinationCollection;
          destinationCollection = ObtainParameter(destinationCollectionType);
        }

        // destination = new List<DestinationType>();
        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, assignListTypeToParameter);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }

      ParameterExpression sourceCollectionItem, destinationCollectionItem;

      if (facts.CanAssignSourceElementsToDestination
        && facts.SourceIsDictionaryType
        && facts.DestinationIsDictionaryType)
      {
        // If both types are a dictionary type and they are assignable to each other,
        // then we want to use the KeyValuePair itself, not just the value.
        sourceCollectionItem = ObtainParameter(facts.SourceElementType, "item");
        destinationCollectionItem = ObtainParameter(facts.DestinationElementType, "item");
      }
      else
      {

        if (facts.CanAssignSourceElementsToDestination && facts.SourceIsDictionaryType)
        {
          // Get the value type of the KeyValuePair
          sourceCollectionItem = ObtainParameter(facts.SourceElementType.GetGenericArguments().Last(), "item");
        }
        else
        {
          sourceCollectionItem = ObtainParameter(facts.SourceElementType, "item");
        }

        if (facts.CanAssignSourceElementsToDestination && facts.DestinationIsDictionaryType)
        {
          // Get the value type of the KeyValuePair
          destinationCollectionItem = ObtainParameter(facts.DestinationElementType.GetGenericArguments().Last(), "item");
        }
        else
        {
          destinationCollectionItem = ObtainParameter(facts.DestinationElementType, "item");
        }
      }

      var expressionsInsideLoop = new List<Expression>();


      BinaryExpression assignNewItemToDestinationItem;

      // The elements in the collection are not of types that are assignable to eachother
      // so we have to create a new item and do additional mapping (most likely).
      if (!facts.CanAssignSourceElementsToDestination)
      {
        var createNewDestinationCollectionItem = GetConstructorForType(facts.DestinationElementType, this.sourceParameter, this.destinationParameter);
        // var destinationItem = new DestinationItem();
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);

        // Used as a flag by the member assignments of this type mapping if we do need to care
        // about preserving existing values on the destination, which we dont if the destination is a new instance
        complexTypeMapping.NewInstanceCreated = true;
      }
      else
      {
        // var destinationItem = sourceItem;
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, sourceCollectionItem);

        // Used as a flag by the member assignments of this type mapping if we do need to care
        // about preserving existing values on the destination, which we do if the destination is not a new instance
        complexTypeMapping.NewInstanceCreated = false;
      }

      var @break = Expression.Label();

      var iteratorVar = ObtainParameter(typeof(int)); //Expression.Parameter(typeof(int), GetIteratorVarName());

      // i++
      var increment = Expression.PostIncrementAssign(iteratorVar);

      Expression assignItemToDestination;

      if (facts.DestinationIsArray)
      {
        // destination[i]
        var accessDestinationCollectionByIndex = Expression.MakeIndex(destinationCollection, null, new[] { iteratorVar });
        // destination[i] = destinationItem;
        var assignDestinationItemToArray = Expression.Assign(accessDestinationCollectionByIndex, destinationCollectionItem);

        assignItemToDestination = assignDestinationItemToArray;
      }
      else
      {
        var addMethod = typeof(ICollection<>).MakeGenericType(facts.DestinationElementType).GetMethod("Add", new[] { facts.DestinationElementType });

        var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

        // destination.Add(destinationItem);
        assignItemToDestination = callAddOnDestinationCollection;

      }

      var loopBlock = new List<Expression>();

      // If it's an IList, we want to iterate through it using a good old for-loop for maximum efficiency.
      if (facts.SourceIsList)
      {
        // var i = 0
        var assignZeroToIteratorVar = Expression.Assign(iteratorVar, Expression.Constant(0));

        ifNotNullBlock.Add(assignZeroToIteratorVar);

        // i < source.Length/Count
        var terminationCondition = Expression.LessThan(iteratorVar, accessSourceEnumerableSize);

        // Find the first indexer property on the list type
        var indexer = facts.SourceEnumerableType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);

        // source[i]
        Expression accessSourceCollectionByIndex = Expression.MakeIndex(accessSourceCollection, indexer, new[] { iteratorVar });

        if (facts.SourceIsDictionaryType)
        {
          accessSourceCollectionByIndex = Expression.Property(accessSourceCollectionByIndex, "Value");
        }

        // var item = source[i]
        var assignCurrent = Expression.Assign(sourceCollectionItem, accessSourceCollectionByIndex);

        expressionsInsideLoop.Add(assignCurrent);

        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        ProcessTypeModifierData(sourceCollectionItem, expressionsInsideLoop, MappingSides.Source);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, complexTypeMapping.CustomMapping);

        expressionsInsideLoop.Add(assignItemToDestination);

        expressionsInsideLoop.Add(increment);

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        // i = 0; while(i < source.Length/Count) { .. i++; }
        var @for = Expression.Loop(
                        Expression.IfThenElse(
                        terminationCondition,
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        //ifNotNullBlock.Add(@for);
        loopBlock.Add(@for);
      }
      else // If it's any normal IEnumerable, use this faux foreach loop
      {
        var getEnumeratorOnSourceMethod = typeof(IEnumerable<>).MakeGenericType(facts.SourceElementType).GetMethod("GetEnumerator", Type.EmptyTypes);

        var sourceEnumeratorType = getEnumeratorOnSourceMethod.ReturnType;

        var sourceEnumerator = ObtainParameter(sourceEnumeratorType);

        var doMoveNextCall = Expression.Call(sourceEnumerator, typeof(IEnumerator).GetMethod("MoveNext"));

        var assignToEnum = Expression.Assign(sourceEnumerator, Expression.Call(accessSourceCollection, getEnumeratorOnSourceMethod));

        loopBlock.Add(assignToEnum);

        var accessCurrentOnEnumerator = Expression.Property(sourceEnumerator, "Current");

        if (facts.SourceIsDictionaryType && !facts.DestinationIsDictionaryType)
        {
          accessCurrentOnEnumerator = Expression.Property(accessCurrentOnEnumerator, "Value");
        }

        var assignCurrent = Expression.Assign(sourceCollectionItem, accessCurrentOnEnumerator);

        expressionsInsideLoop.Add(assignCurrent);
        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        ProcessTypeModifierData(sourceCollectionItem, expressionsInsideLoop, MappingSides.Source);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, complexTypeMapping.CustomMapping);

        expressionsInsideLoop.Add(assignItemToDestination);

        if (facts.DestinationIsArray)
        {
          expressionsInsideLoop.Add(increment);
        }

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        // var enumerator = source.GetEnumerator();
        // while(enumerator.MoveNext()) { sourceItem = enumerator.Current; .. }
        var @foreach = Expression.Loop(
                        Expression.IfThenElse(
                        Expression.NotEqual(doMoveNextCall, Expression.Constant(false)),
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        //ifNotNullBlock.Add(@foreach);
        loopBlock.Add(@foreach);

        ReleaseParameter(sourceEnumerator);

      }

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      //ifNotNullBlock.Add(assignDestinationCollection);
      loopBlock.Add(assignDestinationCollection);

      Expression sourceNotNullCondition = Expression.NotEqual(accessSourceCollection, Expression.Constant(null));

      if (complexTypeMapping.Condition != null)
      {
        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(complexTypeMapping.Condition.Parameters.Single(), this.sourceParameter));

        sourceNotNullCondition = Expression.AndAlso(sourceNotNullCondition, complexTypeMapping.Condition.Body);
      }

      Expression ifEmptyCondition = null;

      Expression ifEmpty = null;

      if (facts.CanAssignSourceToDestination && facts.PreserveDestinationContents)
      {
        //var anyMethod = typeof(Enumerable).GetMethod("Any", new[] { typeof(IEnumerable<>) });

        var callAny = Expression.Call(null, anyMethod.MakeGenericMethod(facts.DestinationElementType), destinationCollection);

        var callReferenceEquals = Expression.Call(null, referenceEqualsMethod, accessSourceCollection, accessDestinationCollection);

        ifEmptyCondition = Expression.And(callAny, Expression.Not(callReferenceEquals));

        ifEmpty = Expression.Assign(accessDestinationCollection, accessSourceCollection);
      }
      else
      {
        ifEmptyCondition = Expression.Constant(true);
        ifEmpty = Expression.Empty();
      }

      var ifEmptyCheck = Expression.IfThenElse(ifEmptyCondition, Expression.Block(loopBlock), ifEmpty);

      ifNotNullBlock.Add(ifEmptyCheck);

      var ifNotNullCheck = Expression.IfThen(sourceNotNullCondition, Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

      ReleaseParameter(iteratorVar);

      ReleaseParameter(destinationCollection);

      ReleaseParameter(sourceCollectionItem);

      ReleaseParameter(destinationCollectionItem);
    }

    private void ProcessTypeModifierData(ParameterExpression param, List<Expression> expressions, MappingSides side)
    {
      var typeData = mapper.Data.TryGetTypeModifierData(param.Type, side);

      if (typeData != null)
      {
        var typeExpr = ProcessTypeModifierData(typeData, param);

        if (typeExpr != null)
        {
          expressions.Add(typeExpr);
        }
      }

      var vars = mapper.Data.GetAllVariablesForType(param.Type, side);

      foreach (var variable in vars)
      {
        var varExpr = Expression.Variable(variable.Type, "_" + variable.Name);
        newParameters.Add(varExpr);
        this.mapProcessor.Variables.Add(variable.Name, varExpr);
        Expression initVar;

        if (variable.Initialization != null)
        {
          initVar = Expression.Assign(varExpr, variable.Initialization.Body);
        }
        else
        {
          initVar = Expression.Assign(varExpr, Expression.Default(variable.Type));
        }

        expressions.Add(initVar);
      }
    }

    private static Expression GetEnumerableSizeAccessor(EnumerableMappingFacts facts, Expression accessSourceCollection)
    {
      Expression accessSourceCollectionSize;

      // If it's an array, we want to access .Length to know its size, if it's ICollection<T>, we want .Count 
      // and for anything else we have to use .Count(). 
      if (facts.SourceIsArray)
      {
        // source.Length
        accessSourceCollectionSize = Expression.Property(accessSourceCollection, "Length");
      }
      else
      {
        if (facts.SourceIsCollection)
        {
          var genericCollection = typeof(ICollection<>).MakeGenericType(facts.SourceElementType);

          var countProperty = genericCollection.GetProperty("Count");

          // source.Count
          accessSourceCollectionSize = Expression.Property(accessSourceCollection, countProperty);
        }
        else
        {
          var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(facts.SourceElementType);

          // source.Count()
          accessSourceCollectionSize = Expression.Call(null, countMethod.MakeGenericMethod(facts.SourceElementType), accessSourceCollection);
        }
      }
      return accessSourceCollectionSize;
    }

    private bool CanAssignSourceElementToDestination(ProposedTypeMapping complexTypeMapping, Type destinationCollectionElementType, Type sourceCollectionElementType)
    {
      var canAssignSourceElementToDest = destinationCollectionElementType.IsAssignableFrom(sourceCollectionElementType);

      if (canAssignSourceElementToDest)
      {
        if (this.IsClone && !sourceCollectionElementType.IsPrimitive && sourceCollectionElementType != typeof(string))
        {
          return false;
        }
      }
      else if (sourceCollectionElementType.IsGenericType
        && typeof(KeyValuePair<,>).IsAssignableFrom(sourceCollectionElementType.GetGenericTypeDefinition()))
      {
        return CanAssignSourceElementToDestination(complexTypeMapping, destinationCollectionElementType, sourceCollectionElementType.GetGenericArguments().Last());
      }


      return canAssignSourceElementToDest;
    }

    private static bool IsCollectionType(Type sourceMemberPropertyType)
    {
      if (typeof(ICollection).IsAssignableFrom(sourceMemberPropertyType))
      {
        return true;
      }
      else if (sourceMemberPropertyType.IsGenericType)
      {
        var genericArg = sourceMemberPropertyType.GetGenericArguments().First();

        return typeof(ICollection<>).MakeGenericType(genericArg).IsAssignableFrom(sourceMemberPropertyType);

      }

      return false;
    }

    private static bool IsListType(Type sourceMemberPropertyType)
    {

      if (typeof(IList).IsAssignableFrom(sourceMemberPropertyType))
      {
        return true;
      }
      else if (sourceMemberPropertyType.IsGenericType)
      {
        var genericArg = sourceMemberPropertyType.GetGenericArguments().First();

        return typeof(IList<>).MakeGenericType(genericArg).IsAssignableFrom(sourceMemberPropertyType);

      }

      return false;
    }

    private static bool TypeReceivesSpecialTreatment(Type type)
    {
      return type == typeof(string);
    }

    private Expression HandleSpecialType(Type sourceType, Type destinationType, ParameterExpression complexSource)
    {
      if (destinationType == typeof(string))
      {
        return HandleStringDestination(sourceType, complexSource);
      }

      return null;
    }

    private Expression HandleStringDestination(Type sourceType, ParameterExpression complexSource)
    {
      if (options.Conventions.CallToStringWhenDestinationIsString)
      {
        var callToStringOnSource = Expression.Call(complexSource, typeof(object).GetMethod("ToString", Type.EmptyTypes));

        return callToStringOnSource;
      }
      else
      {
        throw new CodeGenerationException("Cannot map {0} to System.String", sourceType);
      }
    }

    private Expression GetConstructorForType(Type t, ParameterExpression source, ParameterExpression destination)
    {
      LambdaExpression constructor = proposedMap.GetConstructor(t);

      if (constructor == null)
      {
        constructor = mapper.Data.GetConstructor(t);
      }

      if (constructor == null)
      {
        var parameterlessCtor = t.GetConstructor(Type.EmptyTypes);

        if (parameterlessCtor == null)
        {
          throw new CodeGenerationException("No parameterless constructor defined for type {0} and no custom constructor provided either.", t);
        }

        return Expression.New(t);
      }
      else
      {

        if (constructor.Body.Type != t)
        {
          throw new CodeGenerationException("Invalid LambdaExpression as constructor: does not return type {0}", t);
        }

        if (constructor.Parameters.Count == 2)
        {
          // TSource is always before TDestination
          var oldSrc = constructor.Parameters[0];
          var oldDest = constructor.Parameters[1];

          mapProcessor.ParametersToReplace.Add(new ExpressionTuple(oldSrc, source));
          mapProcessor.ParametersToReplace.Add(new ExpressionTuple(oldDest, destination));

          //constructor = (LambdaExpression)destVisitor.Visit(srcVisitor.Visit(constructor));

          return constructor.Body;
        }
        else if (constructor.Parameters.Count == 0)
        {
          return constructor.Body;
        }
        else
        {
          throw new CodeGenerationException("Invalid LambdaExpression as constructor: {0} is an invalid amount of parameters", constructor.Parameters.Count);
        }
      }

    }

    private void BuildComplexTypeMappingExpressions(
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping complexTypeMapping,
      List<Expression> expressions)
    {

      if (complexTypeMapping.Ignored)
      {
        return;
      }

      ParameterExpression complexSource = null, complexDest = null;

      complexSource = ObtainParameter(complexTypeMapping.SourceMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.SourceMember));

      complexDest = ObtainParameter(complexTypeMapping.DestinationMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.DestinationMember));

      //newParameters.Add(complexSource);
      //newParameters.Add(complexDest);

      var ifNotNullBlock = new List<Expression>();

      ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember)));

      ProcessTypeModifierData(complexSource, ifNotNullBlock, MappingSides.Source);

      var newType = complexTypeMapping.DestinationMember.PropertyOrFieldType;

      var accessDestinationMember = Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember);

      if (TypeReceivesSpecialTreatment(newType))
      {
        ifNotNullBlock.Add
        (
          Expression.Assign
          (
            accessDestinationMember,
            HandleSpecialType
            (
              complexTypeMapping.SourceMember.PropertyOrFieldType,
              newType,
              complexSource
            )
          )
        );
      }
      else
      {
        // var destinationType = new DestinationType();
        Expression assignDestType;

        var destinationMemberCtor = GetConstructorForType(newType, this.sourceParameter, this.destinationParameter);

        if (options.Conventions.ReuseNonNullComplexMembersOnDestination)
        {
          var checkIfDestMemberIsNotNull = Expression.NotEqual(accessDestinationMember, Expression.Default(complexTypeMapping.DestinationMember.PropertyOrFieldType));

          assignDestType = Expression.Assign(complexDest,
            Expression.Condition(checkIfDestMemberIsNotNull,
              accessDestinationMember,
              destinationMemberCtor));
        }
        else
        {
          assignDestType = Expression.Assign(complexDest, destinationMemberCtor);

        }

        ifNotNullBlock.Add(assignDestType);

        BuildTypeMappingExpressions(complexSource, complexDest, complexTypeMapping, ifNotNullBlock, complexTypeMapping.CustomMapping);

        // destination.Member = destinationType;
        ifNotNullBlock.Add(Expression.Assign(Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember), complexDest));
      }

      Expression condition;

      // If it's a value type, then a null check is not necessary, simply make it a 
      // if(true) which will get eliminated by the JIT compiler.
      if (!complexTypeMapping.SourceMember.PropertyOrFieldType.IsValueType || options.Conventions.IgnoreMembersWithNullValueOnSource)
      {
        condition = Expression.NotEqual(Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember), Expression.Default(complexTypeMapping.SourceMember.PropertyOrFieldType));
      }
      else
      {
        condition = Expression.Constant(true);
      }

      if (complexTypeMapping.Condition != null)
      {
        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(complexTypeMapping.Condition.Parameters.Single(), this.sourceParameter));

        condition = Expression.AndAlso(condition, complexTypeMapping.Condition.Body);

      }

      var ifNotNullCheck = Expression.IfThen(condition, Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

      ReleaseParameter(complexSource);
      ReleaseParameter(complexDest);

    }

    private static ModuleBuilder moduleBuilder;

    private static byte[] syncRoot = new byte[0];

    private TypeBuilder DefineMappingType(string name)
    {
      lock (syncRoot)
      {
        if (moduleBuilder == null)
        {
          var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ThisMemberFunctionsAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.RunAndCollect);

          moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
        }
      }
      var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);
      return typeBuilder;
    }

    private Delegate CompileExpression(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      if (this.options.Debug.DebugInformationEnabled)
      {
        this.DebugInformation = new DebugInformation
        {
          MappingExpression = expression
        };
      }

      if (this.options.Compilation.CompileToDynamicAssembly && !mapProcessor.NonPublicMembersAccessed)
      {
        var typeBuilder = DefineMappingType(string.Format("From_{0}_to_{1}_{2}", sourceType.Name, destinationType.Name, Guid.NewGuid().ToString("N")));

        var methodBuilder = typeBuilder.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Static);

        expression.CompileToMethod(methodBuilder);

        var resultingType = typeBuilder.CreateType();

        var function = Delegate.CreateDelegate(expression.Type, resultingType.GetMethod("Map"));

        return function;
      }
      else
      {
        // Much simpler, but the resulting delegate incurs an invocation overhead from experience
        return expression.Compile();
      }

    }

    private static Type GetMatchingFuncOverload(ProposedMap map)
    {
      switch (map.ParameterTypes.Count)
      {
        case 0:
          return typeof(Func<,,>).MakeGenericType(map.SourceType, map.DestinationType, map.DestinationType);
        case 1:
          return typeof(Func<,,,>).MakeGenericType(map.SourceType, map.DestinationType, map.ParameterTypes[0], map.DestinationType);
        case 2:
          return typeof(Func<,,,,>).MakeGenericType(map.SourceType, map.DestinationType, map.ParameterTypes[0], map.ParameterTypes[1], map.DestinationType);
        case 3:
          return typeof(Func<,,,,,,>).MakeGenericType(map.SourceType, map.DestinationType, map.ParameterTypes[0], map.ParameterTypes[1], map.ParameterTypes[2], map.DestinationType);
        default:
          throw new InvalidOperationException("No matching generic overload for Func found");
      }
    }

    private Expression ProcessTypeModifierData(TypeModifierData data, Expression typeParam)
    {
      if (data.ThrowIfCondition != null)
      {
        var param = data.ThrowIfCondition.Parameters.Single();

        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(param, typeParam));

        var message = Expression.Constant(data.Message);

        var throwException = Expression.Throw(Expression.New(typeof(MappingTerminatedException).GetConstructor(new[] { typeof(string) }), message));

        var ifNotConditionThrow = Expression.IfThen(data.ThrowIfCondition.Body, throwException);

        return ifNotConditionThrow;
      }

      return null;
    }

    public Delegate GenerateMappingFunction()
    {

      var destination = Expression.Parameter(proposedMap.DestinationType, "destination");
      var source = Expression.Parameter(proposedMap.SourceType, "source");

      var lambdaParameters = new List<ParameterExpression>();

      lambdaParameters.Add(source);
      lambdaParameters.Add(destination);

      this.sourceParameter = source;
      this.destinationParameter = destination;

      this.Parameters = new List<IndexedParameterExpression>();

      Parameters.Add(new IndexedParameterExpression { Parameter = sourceParameter, Index = 0 });

      int argCount = 1;

      foreach (var param in proposedMap.ParameterTypes)
      {
        var paramExpr = Expression.Parameter(param, "arg" + argCount);
        lambdaParameters.Add(paramExpr);

        this.Parameters.Add(new IndexedParameterExpression { Parameter = paramExpr, Index = argCount });

        argCount++;
      }

      Expression condition;

      // Check if the types we want to map are enumerable themselves
      if (CollectionTypeHelper.IsEnumerable(proposedMap.SourceType)
        && CollectionTypeHelper.IsEnumerable(proposedMap.DestinationType))
      {
        // If so, wrap another type mapping around it with the source and destination
        // members as null.
        proposedMap.ProposedTypeMapping = new ProposedTypeMapping
        {
          DestinationMember = null,
          SourceMember = null,
          ProposedTypeMappings = new List<ProposedTypeMapping>
          {
            proposedMap.ProposedTypeMapping,
          },
          IsEnumerable = true,
        };
        condition = Expression.Constant(true);
      }
      else
      {
        condition = Expression.NotEqual(source, Expression.Default(proposedMap.SourceType));

        if (!options.Safety.ThrowIfDestinationIsNull)
        {
          condition = Expression.AndAlso(condition, Expression.NotEqual(destination, Expression.Default(proposedMap.DestinationType)));
        }

      }

      var assignments = new List<Expression>();

      ProcessTypeModifierData(sourceParameter, assignments, MappingSides.Source);
      ProcessTypeModifierData(sourceParameter, assignments, MappingSides.Destination);

      BuildTypeMappingExpressions(source, destination, proposedMap.ProposedTypeMapping, assignments, proposedMap.ProposedTypeMapping.CustomMapping);

      if (!assignments.Any())
      {
        assignments.Add(Expression.Empty());
      }

      var block = Expression.Block(assignments);

      Expression ifSourceIsNull = Expression.Default(proposedMap.DestinationType);

      if (options.Safety.IfSourceIsNull == SourceObjectNullOptions.AllowNullReferenceExceptionWhenSourceIsNull)
      {
        condition = Expression.Constant(true);
      }
      else if (options.Safety.IfSourceIsNull == SourceObjectNullOptions.ReturnNullWhenSourceIsNull)
      {
        ifSourceIsNull = Expression.Assign(destination, Expression.Default(proposedMap.DestinationType));
      }

      var conditionCheck = Expression.IfThenElse(condition, block, ifSourceIsNull);

      var outerBlock = Expression.Block(newParameters, conditionCheck, destination);

      var funcType = GetMatchingFuncOverload(proposedMap);

      var lambda = Expression.Lambda
      (
        funcType,
        outerBlock,
        lambdaParameters
      );

      lambda = (LambdaExpression)mapProcessor.Process(lambda);

      return CompileExpression(proposedMap.SourceType, proposedMap.DestinationType, lambda);
    }
  }
}
