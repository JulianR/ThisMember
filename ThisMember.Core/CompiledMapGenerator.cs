using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Collections;

namespace ThisMember.Core
{
  internal class CompiledMapGenerator : IMapGenerator
  {
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

    private void BuildTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, ProposedTypeMapping typeMapping, List<Expression> expressions, List<ParameterExpression> newParams, CustomMapping customMapping = null)
    {

      foreach (var member in typeMapping.ProposedMappings)
      {
        BuildSimpleTypeMappingExpressions(source, destination, member, expressions, newParams, customMapping);
      }

      foreach (var complexTypeMapping in typeMapping.ProposedTypeMappings)
      {
        if (typeMapping.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexTypeMapping))
        {
          BuildCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions, newParams);
        }
        else
        {
          BuildNonCollectionComplexTypeMappingExpressions(source, destination, complexTypeMapping, expressions, newParams);
        }
      }
    }

    private void BuildSimpleTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, ProposedMemberMapping member, List<Expression> expressions, List<ParameterExpression> newParams, CustomMapping customMapping = null)
    {
      Expression sourceExpression = Expression.PropertyOrField(source, member.SourceMember.Name);
      var destMember = Expression.PropertyOrField(destination, member.DestinationMember.Name);
      
      BinaryExpression assignSourceToDest;

      Expression customExpression;

      if (customMapping != null && (customExpression = customMapping.GetExpressionForMember(member.DestinationMember)) != null)
      {
        var visitor = new ParameterVisitor(customMapping.Parameter, source);

        customExpression = visitor.Visit(customExpression);

        assignSourceToDest = Expression.Assign(destMember, customExpression);

      }
      else
      {
        assignSourceToDest = Expression.Assign(destMember, sourceExpression);
      }

      expressions.Add(assignSourceToDest);
    }

    private void BuildCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, ProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {


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

      var sourceElementSameAsDestination = destinationCollectionElementType == sourceCollectionElementType;


      Type destinationCollectionType;
      ParameterExpression destinationCollection;

      Expression accessSourceCollection = complexTypeMapping.SourceMember != null ? (Expression)Expression.MakeMemberAccess(source, complexTypeMapping.SourceMember) : source;

      Expression accessSourceCollectionSize;

      if (sourceMemberPropertyType.IsArray)
      {
        accessSourceCollectionSize = Expression.Property(accessSourceCollection, "Length");
      }
      else
      {

        var genericCollection = typeof(ICollection<>).MakeGenericType(sourceCollectionElementType);

        if (genericCollection.IsAssignableFrom(sourceMemberPropertyType))
        {
          var countProperty = genericCollection.GetProperty("Count");

          accessSourceCollectionSize = Expression.Property(accessSourceCollection, countProperty);
        }
        else
        {

          var countMethod = (from m in typeof(Enumerable).GetMethods()
                             where m.Name == "Count" && m.IsGenericMethod
                             && m.GetParameters().Length == 1
                             select m).FirstOrDefault();

          var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(sourceCollectionElementType);

          accessSourceCollectionSize = Expression.Call(null, countMethod.MakeGenericMethod(sourceCollectionElementType), accessSourceCollection);
        }
      }

      if (destinationMemberPropertyType.IsArray)
      {
        destinationCollectionType = destinationMemberPropertyType;

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        var createDestinationCollection = Expression.New(destinationCollectionType.GetConstructors().Single(), accessSourceCollectionSize);

        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }
      else
      {
        destinationCollectionType = typeof(List<>).MakeGenericType(destinationCollectionElementType);

        var createDestinationCollection = Expression.New(destinationCollectionType);

        destinationCollection = Expression.Parameter(destinationCollectionType, GetCollectionName());

        newParams.Add(destinationCollection);

        var assignNewCollectionToDestination = Expression.Assign(destinationCollection, createDestinationCollection);

        ifNotNullBlock.Add(assignNewCollectionToDestination);
      }

      var sourceCollectionItem = Expression.Parameter(sourceCollectionElementType, GetCollectionElementName());

      var expressionsInsideLoop = new List<Expression>();

      var destinationCollectionItem = Expression.Parameter(destinationCollectionElementType, GetCollectionElementName());

      BinaryExpression assignNewItemToDestinationItem;

      if (!sourceElementSameAsDestination)
      {
        var createNewDestinationCollectionItem = Expression.New(destinationCollectionElementType);
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, createNewDestinationCollectionItem);
      }
      else
      {
        assignNewItemToDestinationItem = Expression.Assign(destinationCollectionItem, sourceCollectionItem);
      }
      newParams.Add(sourceCollectionItem);
      newParams.Add(destinationCollectionItem);

      var @break = Expression.Label();

      var iteratorVar = Expression.Parameter(typeof(int), GetIteratorVarName());

      var increment = Expression.PostIncrementAssign(iteratorVar);

      Expression assignItemToDestination;

      if (destinationCollectionType.IsArray)
      {
        var accessDestinationCollectionByIndex = Expression.MakeIndex(destinationCollection, null, new[] { iteratorVar });

        var assignDestinationItemToArray = Expression.Assign(accessDestinationCollectionByIndex, destinationCollectionItem);

        assignItemToDestination = assignDestinationItemToArray;
      }
      else
      {
        var addMethod = destinationCollectionType.GetMethod("Add", new[] { destinationCollectionElementType });
        var callAddOnDestinationCollection = Expression.Call(destinationCollection, addMethod, destinationCollectionItem);

        assignItemToDestination = callAddOnDestinationCollection;

      }

      if (typeof(IList).IsAssignableFrom(sourceMemberPropertyType)
        || (sourceMemberPropertyType.IsGenericType && typeof(IList<>).IsAssignableFrom(sourceMemberPropertyType.GetGenericTypeDefinition())))
      {


        newParams.Add(iteratorVar);

        var assignZeroToIteratorVar = Expression.Assign(iteratorVar, Expression.Constant(0));

        ifNotNullBlock.Add(assignZeroToIteratorVar);

        var terminationCondition = Expression.LessThan(iteratorVar, accessSourceCollectionSize);

        var indexer = sourceMemberPropertyType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);

        var accessSourceCollectionByIndex = Expression.MakeIndex(accessSourceCollection, indexer, new[] { iteratorVar });

        var assignCurrent = Expression.Assign(sourceCollectionItem, accessSourceCollectionByIndex);

        expressionsInsideLoop.Add(assignCurrent);

        expressionsInsideLoop.Add(assignNewItemToDestinationItem);

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams);

        expressionsInsideLoop.Add(assignItemToDestination);

        expressionsInsideLoop.Add(increment);

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        var @foreach = Expression.Loop(
                        Expression.IfThenElse(
                        terminationCondition,
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        ifNotNullBlock.Add(@foreach);
      }
      else
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

        BuildTypeMappingExpressions(sourceCollectionItem, destinationCollectionItem, complexTypeMapping, expressionsInsideLoop, newParams);

        expressionsInsideLoop.Add(assignItemToDestination);

        if (destinationMemberPropertyType.IsArray)
        {
          expressionsInsideLoop.Add(increment);
        }

        var blockInsideLoop = Expression.Block(expressionsInsideLoop);

        var @foreach = Expression.Loop(
                        Expression.IfThenElse(
                        Expression.NotEqual(doMoveNextCall, Expression.Constant(false)),
                            blockInsideLoop
                        , Expression.Break(@break)), @break);

        ifNotNullBlock.Add(@foreach);

      }

      Expression accessDestinationCollection = complexTypeMapping.DestinationMember != null ? (Expression) Expression.MakeMemberAccess(destination, complexTypeMapping.DestinationMember) : destination;

      var assignDestinationCollection = Expression.Assign(accessDestinationCollection, destinationCollection);

      ifNotNullBlock.Add(assignDestinationCollection);

      var ifNotNullCheck = Expression.IfThen(Expression.NotEqual(accessSourceCollection, Expression.Constant(null)), Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

    }

    private void BuildNonCollectionComplexTypeMappingExpressions(ParameterExpression source, ParameterExpression destination, ProposedTypeMapping complexTypeMapping, List<Expression> expressions, List<ParameterExpression> newParams)
    {

      ParameterExpression complexSource = null, complexDest = null;

      complexSource = Expression.Parameter(complexTypeMapping.SourceMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.SourceMember));

      complexDest = Expression.Parameter(complexTypeMapping.DestinationMember.PropertyOrFieldType, GetParameterName(complexTypeMapping.DestinationMember));

      newParams.Add(complexSource);
      newParams.Add(complexDest);

      var ifNotNullBlock = new List<Expression>();

      ifNotNullBlock.Add(Expression.Assign(complexSource, Expression.Property(source, complexTypeMapping.SourceMember.Name)));

      var newType = complexTypeMapping.DestinationMember.PropertyOrFieldType;

      ifNotNullBlock.Add(Expression.Assign(complexDest, Expression.New(newType)));

      BuildTypeMappingExpressions(complexSource, complexDest, complexTypeMapping, ifNotNullBlock, newParams);

      ifNotNullBlock.Add(Expression.Assign(Expression.PropertyOrField(destination, complexTypeMapping.DestinationMember.Name), complexDest));

      Expression equalToNull = null;

      if (!complexTypeMapping.SourceMember.PropertyOrFieldType.IsValueType)
      {
        equalToNull = Expression.NotEqual(Expression.Property(source, complexTypeMapping.SourceMember.Name), Expression.Constant(null));
      }
      else
      {
        equalToNull = Expression.Constant(true);
      }

      var ifNotNullCheck = Expression.IfThen(equalToNull, Expression.Block(ifNotNullBlock));

      expressions.Add(ifNotNullCheck);

    }

    private static ModuleBuilder moduleBuilder;

    private static bool IsPublicClass(Type t)
    {

      if ((!t.IsPublic && !t.IsNestedPublic) || t.IsGenericType)
      {
        return false;
      }

      int lastIndex = t.FullName.LastIndexOf('+');

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

    private static Delegate CompileExpression(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      if (IsPublicClass(sourceType) && IsPublicClass(destinationType))
      {
        if (moduleBuilder == null)
        {
          var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("ThisMemberFunctionsAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);

          moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
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
        return expression.Compile();
      }

    }

    public Delegate GenerateMappingFunction(ProposedMap proposedMap)
    {
      var destination = Expression.Parameter(proposedMap.DestinationType, "destination");
      var source = Expression.Parameter(proposedMap.SourceType, "source");

      var assignments = new List<Expression>();

      var newParams = new List<ParameterExpression>();

      Expression condition;

      if (typeof(IEnumerable).IsAssignableFrom(proposedMap.SourceType)
        && typeof(IEnumerable).IsAssignableFrom(proposedMap.DestinationType))
      {

        proposedMap.ProposedTypeMapping = new ProposedTypeMapping
        {
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

      BuildTypeMappingExpressions(source, destination, proposedMap.ProposedTypeMapping, assignments, newParams, proposedMap.CustomMapping);

      var block = Expression.Block(assignments);

      var conditionCheck = Expression.IfThen(condition, block);

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
