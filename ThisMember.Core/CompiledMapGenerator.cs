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
    private readonly MapExpressionProcessor mapProcessor;
    private IList<ParameterExpression> newParameters;
    private ProposedMap proposedMap;
    private IList<IndexedParameterExpression> Parameters { get; set; }

    public CompiledMapGenerator(IMemberMapper mapper)
    {
      this.mapper = mapper;

      this.mapProcessor = new MapExpressionProcessor(mapper);

      this.newParameters = new List<ParameterExpression>();
    }

    private int currentID = 0;

    private string GetParameterName(PropertyOrFieldInfo member)
    {
      return member.Name;
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

      if (typeof(IEnumerable).IsAssignableFrom(t))
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
        BuildMemberAssignmentExpressions(source, destination, member, expressions, customMapping);
      }

      // Nested type mappings
      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {
        // If it's a collection type
        if (typeMapping.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexTypeMapping))
        {
          BuildCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions, typeMapping.IsEnumerable);
        }
        else
        {
          // If it's not a collection but just a nested type
          BuildNonCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions);
        }
      }
    }


    /// <summary>
    /// Assigns an expression that can be pretty much anythig to a destination mapping.
    /// </summary>
    /// <returns></returns>
    private BinaryExpression AssignSimpleProperty(MemberExpression destination, Expression source)
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
      else if (source.Type.IsClass && mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource
        && source.Type.IsAssignableFrom(destination.Type))
      {
        source = Expression.Condition(Expression.NotEqual(source, Expression.Constant(null)), source, destination);
      }
      else if (!source.Type.IsAssignableFrom(destination.Type))
      {
        // cast
        source = Expression.Convert(source, destination.Type);
      }

      return Expression.Assign(destination, source);
    }

    private Expression HandleSourceNullableValueType(MemberExpression destination, Expression source)
    {
      var nullableType = source.Type.GetGenericArguments().Single();

      // If the source is null then ignore it if option is turned on, preserving the 
      // original value.
      Expression elseClause = mapper.Options.Conventions.IgnoreMembersWithNullValueOnSource ?
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

      Expression assignSourceToDest;

      Expression customExpression;

      if (customMapping != null && (customExpression = customMapping.GetExpressionForMember(member.DestinationMember)) != null)
      {
        assignSourceToDest = Expression.Assign(destMember, customExpression);
      }
      else
      {
        Expression sourceExpression = Expression.MakeMemberAccess(source, member.SourceMember);
        assignSourceToDest = AssignSimpleProperty(destMember, sourceExpression);
      }

      Expression assignConversionToDest = null;

      if (customMapping != null)
      {
        var conversionFunction = customMapping.GetConversionFunction(member.SourceMember, member.DestinationMember);

        this.mapProcessor.ParametersToReplace.Add(new ExpressionTuple(conversionFunction.Parameters.Single(),destMember));

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

    /// <summary>
    /// Generates the loop that maps any IEnumerable type
    /// </summary>
    private void BuildCollectionComplexTypeMappingExpressions
    (
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping complexTypeMapping,
      List<Expression> expressions,
      bool isEnumerable
    )
    {
      if (complexTypeMapping.Ignored)
      {
        return;
      }

      Type sourceMemberPropertyType, destinationMemberPropertyType;

      if (complexTypeMapping.SourceMember != null)
      {
        sourceMemberPropertyType = complexTypeMapping.SourceMember.PropertyOrFieldType;
      }
      else
      {
        sourceMemberPropertyType = source.Type;
      }

      if (complexTypeMapping.DestinationMember != null)
      {
        destinationMemberPropertyType = complexTypeMapping.DestinationMember.PropertyOrFieldType;
      }
      else
      {
        destinationMemberPropertyType = destination.Type;
      }

      var ifNotNullBlock = new List<Expression>();

      var destinationCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(destinationMemberPropertyType);

      var sourceCollectionElementType = CollectionTypeHelper.GetTypeInsideEnumerable(sourceMemberPropertyType);

      var canAssignSourceElementToDest = CanAssignSourceElementToDestination(complexTypeMapping, destinationCollectionElementType, sourceCollectionElementType);

      Type destinationCollectionType;
      ParameterExpression destinationCollection;

      // If SourceMember is null, it means that the root type that is being mapped is enumerable itself.
      Expression accessSourceCollection = complexTypeMapping.SourceMember != null ? (Expression)Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember) : source;

      Expression accessSourceCollectionSize;

      // If it's an array, we want to access .Length to know its size, if it's ICollection<T>, we want .Count 
      // and for anything else we have to use .Count(). 
      if (sourceMemberPropertyType.IsArray)
      {
        // source.Length
        accessSourceCollectionSize = Expression.Property(accessSourceCollection, "Length");
      }
      else
      {

        var genericCollection = typeof(ICollection<>).MakeGenericType(sourceCollectionElementType);

        if (genericCollection.IsAssignableFrom(sourceMemberPropertyType))
        {
          var countProperty = genericCollection.GetProperty("Count");

          // source.Count
          accessSourceCollectionSize = Expression.Property(accessSourceCollection, countProperty);
        }
        else
        {

          var countMethod = (from m in typeof(Enumerable).GetMethods()
                             where m.Name == "Count" && m.IsGenericMethod
                             && m.GetParameters().Length == 1
                             select m).FirstOrDefault();

          var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(sourceCollectionElementType);

          // source.Count()
          accessSourceCollectionSize = Expression.Call(null, countMethod.MakeGenericMethod(sourceCollectionElementType), accessSourceCollection);
        }
      }

      // destination.Collection = newCollection OR return destination, if destination is enumerable itself
      Expression accessDestinationCollection = complexTypeMapping.DestinationMember != null ? (Expression)Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember) : destination;

      if (destinationMemberPropertyType.IsArray)
      {
        destinationCollectionType = destinationMemberPropertyType;

        //destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        destinationCollection = ObtainParameter(destinationCollectionType);

        //newParameters.Add(destinationCollection);

        var createDestinationCollection = Expression.New(destinationCollectionType.GetConstructors().Single(), accessSourceCollectionSize);

        // destination = new DestinationType[source.Length/Count/Count()]
        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }
      else
      {
        destinationCollectionType = typeof(List<>).MakeGenericType(destinationCollectionElementType);

        Expression assignListTypeToParameter;

        var createDestinationCollection = Expression.New(destinationCollectionType);

        // If it's an IList but not an array we want to check if the destination property isn't null
        // and if it isn't, we want to reuse it.
        if (mapper.Options.Conventions.PreserveDestinationListContents
          && IsCollectionType(accessDestinationCollection.Type)
          && !accessDestinationCollection.Type.IsArray)
        {

          Expression reuseCondition;

          if (mapper.Options.Safety.EnsureCollectionIsNotArrayType)
          {
            reuseCondition = Expression.And(Expression.NotEqual(accessDestinationCollection, Expression.Constant(null)),
              Expression.IsFalse(Expression.TypeIs(accessDestinationCollection, destinationCollectionElementType.MakeArrayType())));
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

      //var sourceCollectionItem = Expression.Parameter(sourceCollectionElementType, GetCollectionElementName());
      var sourceCollectionItem = ObtainParameter(sourceCollectionElementType, "item");

      var expressionsInsideLoop = new List<Expression>();

      //var destinationCollectionItem = Expression.Parameter(destinationCollectionElementType, GetCollectionElementName());

      var destinationCollectionItem = ObtainParameter(destinationCollectionElementType, "item");

      BinaryExpression assignNewItemToDestinationItem;

      // The elements in the collection are not of types that are assignable to eachother
      // so we have to create a new item and do additional mapping (most likely).
      if (!canAssignSourceElementToDest)
      {
        var createNewDestinationCollectionItem = GetConstructorForType(destinationCollectionElementType, this.sourceParameter, this.destinationParameter);
        // var destinationItem = new DestinationItem();
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);
      }
      else
      {
        // var destinationItem = sourceItem;
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, sourceCollectionItem);
      }

      //newParameters.Add(sourceCollectionItem);
      //newParameters.Add(destinationCollectionItem);

      var @break = Expression.Label();

      ParameterExpression iteratorVar = ObtainParameter(typeof(int)); //Expression.Parameter(typeof(int), GetIteratorVarName());

      // i++
      var increment = Expression.PostIncrementAssign(iteratorVar);

      Expression assignItemToDestination;

      if (destinationCollectionType.IsArray)
      {
        // destination[i]
        var accessDestinationCollectionByIndex = Expression.MakeIndex(destinationCollection, null, new[] { iteratorVar });
        // destination[i] = destinationItem;
        var assignDestinationItemToArray = Expression.Assign(accessDestinationCollectionByIndex, destinationCollectionItem);

        assignItemToDestination = assignDestinationItemToArray;
      }
      else
      {
        var addMethod = typeof(ICollection<>).MakeGenericType(destinationCollectionElementType).GetMethod("Add", new[] { destinationCollectionElementType });
        // var addMethod = destinationCollectionType.GetMethod("Add", new[] { destinationCollectionElementType });
        var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

        // destination.Add(destinationItem);
        assignItemToDestination = callAddOnDestinationCollection;

      }

      // If it's an IList, we want to iterate through it using a good old for-loop for maximum efficiency.
      if (IsListType(sourceMemberPropertyType))
      {
        var assignZeroToIteratorVar = Expression.Assign(iteratorVar, Expression.Constant(0));

        ifNotNullBlock.Add(assignZeroToIteratorVar);

        // i < source.Length/Count
        var terminationCondition = Expression.LessThan(iteratorVar, accessSourceCollectionSize);

        var indexer = sourceMemberPropertyType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);

        var accessSourceCollectionByIndex = Expression.MakeIndex(accessSourceCollection, indexer, new[] { iteratorVar });

        var assignCurrent = Expression.Assign(sourceCollectionItem, accessSourceCollectionByIndex);

        expressionsInsideLoop.Add(assignCurrent);

        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

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

        ifNotNullBlock.Add(@for);
      }
      else // If it's any normal IEnumerable, use this faux foreach loop
      {
        var getEnumeratorOnSourceMethod = typeof(IEnumerable<>).MakeGenericType(sourceCollectionElementType).GetMethod("GetEnumerator", Type.EmptyTypes);

        var sourceEnumeratorType = getEnumeratorOnSourceMethod.ReturnType;

        var sourceEnumerator = ObtainParameter(sourceEnumeratorType);

        var doMoveNextCall = Expression.Call(sourceEnumerator, typeof(IEnumerator).GetMethod("MoveNext"));

        var assignToEnum = Expression.Assign(sourceEnumerator, Expression.Call(accessSourceCollection, getEnumeratorOnSourceMethod));

        ifNotNullBlock.Add(assignToEnum);

        var assignCurrent = Expression.Assign(sourceCollectionItem, Expression.Property(sourceEnumerator, "Current"));

        expressionsInsideLoop.Add(assignCurrent);
        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, complexTypeMapping.CustomMapping);

        expressionsInsideLoop.Add(assignItemToDestination);

        if (destinationMemberPropertyType.IsArray)
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

        ifNotNullBlock.Add(@foreach);

        ReleaseParameter(sourceEnumerator);

      }

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      ifNotNullBlock.Add(assignDestinationCollection);


      Expression sourceNotNullCondition = Expression.NotEqual(accessSourceCollection, Expression.Constant(null));

      if (complexTypeMapping.Condition != null)
      {
        mapProcessor.ParametersToReplace.Add(new ExpressionTuple(complexTypeMapping.Condition.Parameters.Single(), this.sourceParameter));

        sourceNotNullCondition = Expression.AndAlso(sourceNotNullCondition, complexTypeMapping.Condition.Body);
      }

      var ifNotNullCheck = Expression.IfThen(sourceNotNullCondition, Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

      ReleaseParameter(iteratorVar);

      ReleaseParameter(destinationCollection);

      ReleaseParameter(sourceCollectionItem);

      ReleaseParameter(destinationCollectionItem);
    }

    private bool CanAssignSourceElementToDestination(ProposedTypeMapping complexTypeMapping, Type destinationCollectionElementType, Type sourceCollectionElementType)
    {
      var canAssignSourceElementToDest = destinationCollectionElementType.IsAssignableFrom(sourceCollectionElementType);

      if (canAssignSourceElementToDest)
      {
        if (!sourceCollectionElementType.IsPrimitive && sourceCollectionElementType != typeof(string))
        {
          canAssignSourceElementToDest = false;
        }
        else if (complexTypeMapping.SourceMember != null && !sourceCollectionElementType.IsPrimitive && sourceCollectionElementType != typeof(string))
        {
          if (complexTypeMapping.SourceMember.DeclaringType == complexTypeMapping.DestinationMember.DeclaringType && mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
          {
            canAssignSourceElementToDest = false;
          }
        }
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
      return type == typeof(string) || type == typeof(DateTime);
    }

    private Expression HandleSpecialType(Type sourceType, Type destinationType, ParameterExpression complexSource)
    {
      if (destinationType == typeof(string))
      {
        return HandleStringDestination(sourceType, complexSource);
      }
      else if (destinationType == typeof(DateTime) && sourceType == typeof(string))
      {
        return HandleDateTimeDestination(complexSource);
      }

      return null;
    }

    private Expression HandleDateTimeDestination(ParameterExpression complexSource)
    {
      if (mapper.Options.Conventions.DateTime.ParseStringsToDateTime)
      {
        var culture = mapper.Options.Conventions.DateTime.ParseCulture ?? CultureInfo.CurrentCulture;

        var cultureExpression = Expression.New(typeof(CultureInfo).GetConstructor(new[] { typeof(string) }), Expression.Constant(culture.Name));

        var parseMethod = typeof(DateTime).GetMethod("Parse", new[] { typeof(string), typeof(IFormatProvider) });

        return Expression.Call(null, parseMethod, complexSource, cultureExpression);
      }
      else
      {
        throw new CodeGenerationException("Cannot map System.String to System.DateTime");
      }
    }

    private Expression HandleStringDestination(Type sourceType, ParameterExpression complexSource)
    {
      if (mapper.Options.Conventions.CallToStringWhenDestinationIsString)
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
        constructor = mapper.GetConstructor(t);
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

    private void BuildNonCollectionComplexTypeMappingExpressions(
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

      ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.SourceMember.Name)));

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

        if (mapper.Options.Conventions.ReuseNonNullComplexMembersOnDestination)
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
      if (!complexTypeMapping.SourceMember.PropertyOrFieldType.IsValueType || mapper.Options.Conventions.IgnoreMembersWithNullValueOnSource)
      {
        condition = Expression.NotEqual(Expression.Property(source, complexTypeMapping.SourceMember.Name), Expression.Default(complexTypeMapping.SourceMember.PropertyOrFieldType));
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
          var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ThisMemberFunctionsAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.RunAndSave);

          moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
        }
      }
      var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);
      return typeBuilder;
    }

    private Delegate CompileExpression(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      if (this.mapper.Options.Compilation.CompileToDynamicAssembly && !mapProcessor.NonPublicMembersAccessed)
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


    public Delegate GenerateMappingFunction(ProposedMap proposedMap)
    {

      var destination = Expression.Parameter(proposedMap.DestinationType, "destination");
      var source = Expression.Parameter(proposedMap.SourceType, "source");

      var lambdaParameters = new List<ParameterExpression>();

      lambdaParameters.Add(source);
      lambdaParameters.Add(destination);


      this.sourceParameter = source;
      this.destinationParameter = destination;
      this.proposedMap = proposedMap;

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

      var assignments = new List<Expression>();


      Expression condition;

      // Check if the types we want to map are enumerable themselves
      if (typeof(IEnumerable).IsAssignableFrom(proposedMap.SourceType)
        && typeof(IEnumerable).IsAssignableFrom(proposedMap.DestinationType))
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

        if (!mapper.Options.Safety.ThrowIfDestinationIsNull)
        {
          condition = Expression.AndAlso(condition, Expression.NotEqual(destination, Expression.Default(proposedMap.DestinationType)));
        }

      }

      BuildTypeMappingExpressions(source, destination, proposedMap.ProposedTypeMapping, assignments, proposedMap.ProposedTypeMapping.CustomMapping);

      if (!assignments.Any())
      {
        assignments.Add(Expression.Empty());
      }

      var block = Expression.Block(assignments);

      Expression ifSourceIsNull = Expression.Default(proposedMap.DestinationType);

      if (mapper.Options.Safety.IfSourceIsNull == SourceObjectNullOptions.AllowNullReferenceExceptionWhenSourceIsNull)
      {
        condition = Expression.Constant(true);
      }
      else if (mapper.Options.Safety.IfSourceIsNull == SourceObjectNullOptions.ReturnNullWhenSourceIsNull)
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
