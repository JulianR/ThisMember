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

namespace ThisMember.Core
{
  internal class CompiledMapGenerator : IMapGenerator
  {

    private class SourceAndDestinationParameter
    {
      public ParameterExpression Source { get; set; }
      public ParameterExpression Destination { get; set; }
    }

    private class MemberVisitor : ExpressionVisitor
    {
      private readonly IMemberMapper mapper;
      public MemberVisitor(IMemberMapper mapper)
      {
        this.mapper = mapper;
      }

      private Expression ConvertToConditionals(Type conditionalReturnType, Expression expression, Expression newExpression)
      {

        var memberNode = expression as MemberExpression;

        if (memberNode == null)
        {
          return newExpression ?? expression;
        }

        if (newExpression == null)
        {
          newExpression = Expression.Condition(Expression.NotEqual(memberNode.Expression, Expression.Constant(null)),
            memberNode, Expression.Default(conditionalReturnType), conditionalReturnType);

          if (memberNode.Expression.NodeType == ExpressionType.Parameter)
          {
            return newExpression;
          }
        }
        else
        {

          if (memberNode.Expression.NodeType == ExpressionType.Parameter)
          {
            return newExpression;
          }
          else
          {

            newExpression = Expression.Condition(Expression.NotEqual(memberNode.Expression, Expression.Constant(null)),
              newExpression, Expression.Default(conditionalReturnType), conditionalReturnType);
          }
        }

        return ConvertToConditionals(conditionalReturnType, memberNode.Expression, newExpression);

      }

      protected override Expression VisitMember(MemberExpression node)
      {
        if (mapper.Options.Safety.PerformNullChecksOnCustomMappings)
        {
          return ConvertToConditionals(node.Type, node, null);
        }

        return base.VisitMember(node);

        //return Expression.Condition(Expression.NotEqual(node.Expression, Expression.Constant(null)), node, Expression.Default(node.Type), node.Type);

        //return base.VisitMember(node);
      }
    }

    private class ParameterVisitor : ExpressionVisitor
    {

      private ParameterExpression oldParam, newParam;
      public ParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
      {
        this.oldParam = oldParam;
        this.newParam = newParam;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        if (node.Equals(oldParam))
        {
          return newParam;
        }
        return node;

        //return base.VisitParameter(node);
      }
    }

    private int currentID = 1;

    private string GetParameterName(PropertyOrFieldInfo member)
    {
      return member.Name + "#" + currentID++;
    }

    private string GetCollectionName()
    {
      return "collection#" + currentID++;
    }

    private string GetEnumeratorName()
    {
      return "enumerator#" + currentID++;
    }

    private string GetCollectionElementName()
    {
      return "item#" + currentID++;
    }

    private string GetIteratorVarName()
    {
      return "i#" + currentID++;
    }

    private void BuildTypeMappingExpressions
    (
      SourceAndDestinationParameter rootParams,
      ProposedMap map, ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping typeMapping,
      List<Expression> expressions,
      List<ParameterExpression> newParams,
      CustomMapping customMapping = null
    )
    {

      foreach (var member in typeMapping.ProposedMappings)
      {
        BuildSimpleTypeMappingExpressions(rootParams, map, source, destination, member, expressions, newParams, customMapping);
      }

      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {
        if (typeMapping.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexTypeMapping))
        {
          BuildCollectionComplexTypeMappingExpressions(rootParams, map, source, destination, complexTypeMapping, expressions, newParams);
        }
        else
        {
          BuildNonCollectionComplexTypeMappingExpressions(rootParams, map, source, destination, complexTypeMapping, expressions, newParams);
        }
      }
    }



    private BinaryExpression AssignSimpleProperty(MemberExpression destination, Expression source)
    {
      if (destination.Type.IsNullableValueType() && !source.Type.IsNullableValueType())
      {
        var nullableType = destination.Type.GetGenericArguments().Single();

        source = Expression.New(destination.Type.GetConstructor(new[] { nullableType }), source);
      }
      else if (!destination.Type.IsNullableValueType() && source.Type.IsNullableValueType())
      {
        var nullableType = source.Type.GetGenericArguments().Single();

        source = Expression.Condition(Expression.IsTrue(Expression.Property(source, "HasValue")), Expression.Property(source, "Value"), Expression.Default(nullableType));
      }

      return Expression.Assign(destination, source);
    }

    private void BuildSimpleTypeMappingExpressions
    (
      SourceAndDestinationParameter rootParams,
      ProposedMap map,
      ParameterExpression source,
      ParameterExpression destination,
      ProposedMemberMapping member,
      List<Expression> expressions,
      List<ParameterExpression> newParams,
      CustomMapping customMapping = null
    )
    {
      ParameterVisitor paramVisitor;

      if (member.Ignored)
      {
        return;
      }

      Expression condition = null;

      if (member.Condition != null)
      {
        paramVisitor = new ParameterVisitor(member.Condition.Parameters.Single(), rootParams.Source);

        condition = paramVisitor.Visit(member.Condition.Body);
      }

      var destMember = Expression.PropertyOrField(destination, member.DestinationMember.Name);

      BinaryExpression assignSourceToDest;

      Expression customExpression;

      if (customMapping != null && (customExpression = customMapping.GetExpressionForMember(member.DestinationMember)) != null)
      {
        paramVisitor = new ParameterVisitor(customMapping.Parameter, source);

        var memberVisitor = new MemberVisitor(this.mapper);

        customExpression = memberVisitor.Visit(customExpression);

        customExpression = paramVisitor.Visit(customExpression);

        assignSourceToDest = Expression.Assign(destMember, customExpression);
      }
      else
      {
        Expression sourceExpression = Expression.PropertyOrField(source, member.SourceMember.Name);
        assignSourceToDest = AssignSimpleProperty(destMember, sourceExpression);
      }

      if (condition != null)
      {
        var ifCondition = Expression.IfThen(condition, assignSourceToDest);
        expressions.Add(ifCondition);
      }
      else
      {
        expressions.Add(assignSourceToDest);
      }
    }

    private void BuildCollectionComplexTypeMappingExpressions
    (
      SourceAndDestinationParameter rootParams,
      ProposedMap map,
      ParameterExpression source,
      ParameterExpression destination,
      ProposedTypeMapping complexTypeMapping,
      List<Expression> expressions,
      List<ParameterExpression> newParams
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

      var sourceElementSameAsDestination = destinationCollectionElementType.IsAssignableFrom(sourceCollectionElementType);


      Type destinationCollectionType;
      ParameterExpression destinationCollection;

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

      if (destinationMemberPropertyType.IsArray)
      {
        destinationCollectionType = destinationMemberPropertyType;

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        var createDestinationCollection = Expression.New(destinationCollectionType.GetConstructors().Single(), accessSourceCollectionSize);

        // destination = new DestinationType[source.Length/Count/Count()]
        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }
      else
      {
        destinationCollectionType = typeof(List<>).MakeGenericType(destinationCollectionElementType);

        var createDestinationCollection = Expression.New(destinationCollectionType);

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        // destination = new List<DestinationType>();
        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }

      var sourceCollectionItem = Expression.Parameter(sourceCollectionElementType, GetCollectionElementName());

      var expressionsInsideLoop = new List<Expression>();

      var destinationCollectionItem = Expression.Parameter(destinationCollectionElementType, GetCollectionElementName());

      BinaryExpression assignNewItemToDestinationItem;

      // The elements in the collection are not of types that are assignable to eachother
      // so we have to create a new item and do additional mapping (most likely).
      if (!sourceElementSameAsDestination)
      {
        var createNewDestinationCollectionItem = GetConstructorForType(map, destinationCollectionElementType, rootParams.Source, rootParams.Destination);
        // var destinationItem = new DestinationItem();
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);
      }
      else
      {
        // var destinationItem = sourceItem;
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, sourceCollectionItem);
      }
      newParams.Add(sourceCollectionItem);
      newParams.Add(destinationCollectionItem);

      var @break = Expression.Label();

      var iteratorVar = Expression.Parameter(typeof(int), GetIteratorVarName());

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
        var addMethod = destinationCollectionType.GetMethod("Add", new[] { destinationCollectionElementType });
        var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

        // destination.Add(destinationItem);
        assignItemToDestination = callAddOnDestinationCollection;

      }

      // If it's an IList, we want to iterate through it using a good old for-loop for speed.
      if (typeof(IList).IsAssignableFrom(sourceMemberPropertyType)
        || (sourceMemberPropertyType.IsGenericType && typeof(IList<>).IsAssignableFrom(sourceMemberPropertyType.GetGenericTypeDefinition())))
      {


        newParams.Add(iteratorVar);

        var assignZeroToIteratorVar = Expression.Assign(iteratorVar, Expression.Constant(0));

        ifNotNullBlock.Add(assignZeroToIteratorVar);

        // i < source.Length/Count
        var terminationCondition = Expression.LessThan(iteratorVar, accessSourceCollectionSize);

        var indexer = sourceMemberPropertyType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);

        var accessSourceCollectionByIndex = Expression.MakeIndex(accessSourceCollection, indexer, new[] { iteratorVar });

        var assignCurrent = Expression.Assign(sourceCollectionItem, accessSourceCollectionByIndex);

        expressionsInsideLoop.Add(assignCurrent);

        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(rootParams, map, sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams, complexTypeMapping.CustomMapping);

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

        var getEnumeratorOnSourceMethod = sourceMemberPropertyType.GetMethod("GetEnumerator", Type.EmptyTypes);

        var sourceEnumeratorType = getEnumeratorOnSourceMethod.ReturnType;

        var sourceEnumerator = Expression.Parameter(sourceEnumeratorType, GetEnumeratorName());

        if (destinationMemberPropertyType.IsArray)
        {
          newParams.Add(iteratorVar);
        }

        newParams.Add(sourceEnumerator);

        var doMoveNextCall = Expression.Call(sourceEnumerator, typeof(IEnumerator).GetMethod("MoveNext"));

        var assignToEnum = Expression.Assign(sourceEnumerator, Expression.Call(accessSourceCollection, getEnumeratorOnSourceMethod));

        ifNotNullBlock.Add(assignToEnum);

        var assignCurrent = Expression.Assign(sourceCollectionItem, Expression.Property(sourceEnumerator, "Current"));

        expressionsInsideLoop.Add(assignCurrent);
        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(rootParams, map, sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams, complexTypeMapping.CustomMapping);

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

      }

      // destination.Collection = newCollection OR return destination, if destination is enumerable itself
      Expression accessDestinationCollection = complexTypeMapping.DestinationMember != null ? (Expression)Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember) : destination;

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      ifNotNullBlock.Add(assignDestinationCollection);


      Expression condition = Expression.NotEqual(accessSourceCollection, Expression.Constant(null));

      if (complexTypeMapping.Condition != null)
      {
        var visitor = new ParameterVisitor(complexTypeMapping.Condition.Parameters.Single(), rootParams.Source);

        condition = Expression.AndAlso(condition, visitor.Visit(complexTypeMapping.Condition.Body));

      }

      var ifNotNullCheck = Expression.IfThen(condition, Expression.Block(ifNotNullBlock));


      expressions.Add(ifNotNullCheck);

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

    private Expression GetConstructorForType(ProposedMap map, Type t, ParameterExpression source, ParameterExpression destination)
    {
      LambdaExpression constructor = map.GetConstructor(t);

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

          var srcVisitor = new ParameterVisitor(oldSrc, source);
          var destVisitor = new ParameterVisitor(oldDest, destination);

          constructor = (LambdaExpression)destVisitor.Visit(srcVisitor.Visit(constructor));

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

    private void BuildNonCollectionComplexTypeMappingExpressions(SourceAndDestinationParameter rootParams, ProposedMap map, ParameterExpression source, ParameterExpression destination, ProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      if (complexTypeMapping.Ignored)
      {
        return;
      }

      ParameterExpression complexSource = null, complexDest = null;

      complexSource = Expression.Parameter(complexTypeMapping.SourceMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.SourceMember));

      complexDest = Expression.Parameter(complexTypeMapping.DestinationMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.DestinationMember));

      newParams.Add(complexSource);
      newParams.Add(complexDest);

      var ifNotNullBlock = new List<Expression>();

      ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.SourceMember.Name)));

      var newType = complexTypeMapping.DestinationMember.PropertyOrFieldType;

      var accessDestinationMember = Expression.PropertyOrField(destination, complexTypeMapping.DestinationMember.Name);

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

        var destinationMemberCtor = GetConstructorForType(map, newType, rootParams.Source, rootParams.Destination);

        if (mapper.Options.Conventions.ReuseNonNullComplexMembersOnDestination)
        {
          var checkIfDestMemberIsNotNull = Expression.NotEqual(accessDestinationMember, Expression.Constant(null));

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

        BuildTypeMappingExpressions(rootParams, map, complexSource, complexDest, complexTypeMapping, ifNotNullBlock, newParams, complexTypeMapping.CustomMapping);

        // destination.Member = destinationType;
        ifNotNullBlock.Add(Expression.Assign(Expression.PropertyOrField(destination, complexTypeMapping.DestinationMember.Name), complexDest));
      }

      Expression condition;

      // If it's a value type, then a null check is not necessary, simply make it a 
      // if(true) which will get eliminated by the JIT compiler.
      if (!complexTypeMapping.SourceMember.PropertyOrFieldType.IsValueType)
      {
        condition = Expression.NotEqual(Expression.Property(source, complexTypeMapping.SourceMember.Name), Expression.Constant(null));
      }
      else
      {
        condition = Expression.Constant(true);
      }

      if (complexTypeMapping.Condition != null)
      {
        var visitor = new ParameterVisitor(complexTypeMapping.Condition.Parameters.Single(), rootParams.Source);

        condition = Expression.AndAlso(condition, visitor.Visit(complexTypeMapping.Condition.Body));

      }

      var ifNotNullCheck = Expression.IfThen(condition, Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

    }

    private static ModuleBuilder moduleBuilder;

    private static bool IsPublicClass(Type t)
    {
      // For the purposes this method is used for, also consider generic types to be 'non-public'
      if ((!t.IsPublic && !t.IsNestedPublic) || t.IsGenericType)
      {
        return false;
      }

      int lastIndex = t.FullName.LastIndexOf('+');

      // Resolve the containing type of a nested class and check if it's public
      if (lastIndex > 0)
      {
        var containgTypeName = t.FullName.Substring(0, lastIndex);

        var containingType = Type.GetType(containgTypeName + "," + t.Assembly);

        if (containingType != null)
        {
          return containingType.IsPublic;
        }

        return false;
      }
      else
      {
        return t.IsPublic;
      }
    }

    private static byte[] syncRoot = new byte[0];

    private Delegate CompileExpression(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      if (!this.mapper.Options.Safety.DoNotCompileToDynamicAssembly && IsPublicClass(sourceType) && IsPublicClass(destinationType))
      {
        lock (syncRoot)
        {
          if (moduleBuilder == null)
          {
            //var ps = new PermissionSet(PermissionState.None);
            //ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.Infrastructure));
            //ps.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

            //var domain = AppDomain.CreateDomain("MemberMapper",
            //  null,
            //  new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory },
            //  ps,
            //  typeof(Type).Assembly.Evidence.GetHostEvidence<StrongName>());

            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ThisMemberFunctionsAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);

            moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
          }
        }

        var typeBuilder = moduleBuilder.DefineType(string.Format("From_{0}_to_{1}_{2}", sourceType.Name, destinationType.Name, Guid.NewGuid().ToString("N"), TypeAttributes.Public));

        var methodBuilder = typeBuilder.DefineMethod("Map", MethodAttributes.Public | MethodAttributes.Static);

        expression.CompileToMethod(methodBuilder);

        var resultingType = typeBuilder.CreateType();

        var function = Delegate.CreateDelegate(expression.Type, resultingType.GetMethod("Map"));

        return function;
      }
      else
      {
        // Much simpler, but the resulting code from experience can also be 10x slower for unknown reasons
        return expression.Compile();
      }

    }

    private readonly IMemberMapper mapper;

    public CompiledMapGenerator(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    public Delegate GenerateMappingFunction(ProposedMap proposedMap)
    {
      var destination = Expression.Parameter(proposedMap.DestinationType, "destination");
      var source = Expression.Parameter(proposedMap.SourceType, "source");

      var assignments = new List<Expression>();

      var newParams = new List<ParameterExpression>();

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
        condition = Expression.NotEqual(source, Expression.Constant(null));
      }

      var tuple = new SourceAndDestinationParameter
      {
        Source = source,
        Destination = destination
      };

      BuildTypeMappingExpressions(tuple, proposedMap, source, destination, proposedMap.ProposedTypeMapping, assignments, newParams, proposedMap.ProposedTypeMapping.CustomMapping);

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

      var outerBlock = Expression.Block(newParams, conditionCheck, destination);

      var funcType = typeof(Func<,,>).MakeGenericType(proposedMap.SourceType, proposedMap.DestinationType, proposedMap.DestinationType);

      var lambda = Expression.Lambda
      (
        funcType,
        outerBlock,
        source, destination
      );

      return CompileExpression(proposedMap.SourceType, proposedMap.DestinationType, lambda);


    }

  }
}
