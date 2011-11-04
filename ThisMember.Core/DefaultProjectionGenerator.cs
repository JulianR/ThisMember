using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace ThisMember.Core
{
  public class DefaultProjectionGenerator : IProjectionGenerator
  {
    private ParameterExpression sourceParam;
    private readonly IMemberMapper mapper;

    private static MethodInfo selectMethod;
    private ProjectionProcessor processor;

    public DefaultProjectionGenerator(IMemberMapper mapper)
    {
      this.mapper = mapper;
      this.processor = new ProjectionProcessor(mapper);
    }

    public LambdaExpression GetProjection(ProposedMap map)
    {
      sourceParam = Expression.Parameter(map.SourceType, "source");

      var initRoot = BuildProjectionExpression(sourceParam, map.DestinationType, map.ProposedTypeMapping);

      var funcType = typeof(Func<,>).MakeGenericType(map.SourceType, map.DestinationType);

      var lambda = Expression.Lambda
      (
        funcType,
        initRoot,
        sourceParam
      );

      lambda = (LambdaExpression)processor.Process(lambda);

      return lambda;
    }

    private MemberInitExpression BuildProjectionExpression(Expression sourceAccess, Type destinationType, ProposedTypeMapping proposedMap)
    {
      var memberBindings = new List<MemberBinding>();

      foreach (var member in proposedMap.ProposedMappings)
      {
        BuildMemberAssignmentExpressions(sourceAccess, memberBindings, member, proposedMap.CustomMapping);
      }

      foreach (var complexMember in proposedMap.ProposedTypeMappings)
      {
        if (complexMember.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexMember))
        {
          if (mapper.Options.Projection.MapCollectionMembers)
          {
            BuildCollectionComplexTypeExpression(sourceAccess, memberBindings, complexMember);
          }
        }
        else
        {
          BuildComplexTypeExpression(sourceAccess, memberBindings, complexMember);
        }
      }

      var initDestination = Expression.MemberInit(Expression.New(destinationType), memberBindings);

      return initDestination;

    }


    private static MethodInfo GetSelectMethod()
    {
      if (selectMethod == null)
      {
        selectMethod = (from m in typeof(Enumerable).GetMethods()
                        where m.Name == "Select"
                        && typeof(Func<,>).IsAssignableFrom(m.GetParameters()[1].ParameterType.GetGenericTypeDefinition())
                        select m).Single();
      }
      return selectMethod;
    }



    private void BuildCollectionComplexTypeExpression(Expression sourceAccess, List<MemberBinding> memberBindings, ProposedTypeMapping complexMember)
    {
      if (complexMember.Ignored)
      {
        return;
      }

      var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(complexMember.SourceMember.PropertyOrFieldType);

      var genericSourceEnumerable = typeof(IEnumerable<>).MakeGenericType(typeOfSourceEnumerable);

      var typeOfDestEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(complexMember.DestinationMember.PropertyOrFieldType);

      var genericDestEnumerable = typeof(IEnumerable<>).MakeGenericType(typeOfSourceEnumerable);

      var selectMethod = GetSelectMethod().MakeGenericMethod(typeOfSourceEnumerable, typeOfDestEnumerable);

      var selectParam = Expression.Parameter(typeOfSourceEnumerable, "src");

      var bindings = new List<MemberBinding>();

      var memberInit = BuildProjectionExpression(selectParam, typeOfDestEnumerable, complexMember);

      var funcType = typeof(Func<,>).MakeGenericType(typeOfSourceEnumerable, typeOfDestEnumerable);

      var memberInitLambda = Expression.Lambda(funcType, memberInit, selectParam);

      var accessMember = Expression.MakeMemberAccess(sourceAccess, complexMember.SourceMember);

      var callSelect = Expression.Call(null, selectMethod, accessMember, memberInitLambda);

      Expression finalExpression;

      var conversionMethod = DetermineIEnumerableConversionMethod(complexMember.DestinationMember.PropertyOrFieldType, typeOfSourceEnumerable, typeOfDestEnumerable);

      if (conversionMethod != null)
      {
        finalExpression = Expression.Call(null, conversionMethod, callSelect);
      }
      else
      {
        finalExpression = callSelect;
      }


      var bindSourceToDest = Expression.Bind(complexMember.DestinationMember, finalExpression);
      memberBindings.Add(bindSourceToDest);

      //BuildComplexTypeExpression(selectParam, bindings, complexMember);
    }

    private MethodInfo DetermineIEnumerableConversionMethod(Type destinationCollectionType, Type sourceItem, Type destItem)
    {
      if (destinationCollectionType.IsArray)
      {
        return typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(destItem);
      }
      else if (typeof(IList<>).MakeGenericType(destItem).IsAssignableFrom(destinationCollectionType)
        || typeof(ICollection<>).MakeGenericType(destItem).IsAssignableFrom(destinationCollectionType))
      {
        return typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(destItem);
      }
      return null;
    }

    private void BuildComplexTypeExpression(Expression sourceAccess, List<MemberBinding> memberBindings, ProposedTypeMapping complexMember)
    {
      Expression accessMember = Expression.MakeMemberAccess(sourceAccess, complexMember.SourceMember);

      var type = complexMember.DestinationMember.PropertyOrFieldType;

      var memberInit = BuildProjectionExpression(accessMember, type, complexMember);

      var bindSourceToDest = Expression.Bind(complexMember.DestinationMember, memberInit);
      memberBindings.Add(bindSourceToDest);
    }

    private void BuildMemberAssignmentExpressions(Expression sourceAccess, List<MemberBinding> memberBindings, ProposedMemberMapping member, CustomMapping customMapping)
    {
      if (member.Ignored)
      {
        return;
      }

      Expression customExpression;

      MemberAssignment bindSourceToDest;

      if (customMapping != null && (customExpression = customMapping.GetExpressionForMember(member.DestinationMember)) != null)
      {
        processor.ParametersToReplace.Add(new ProjectionExpressionTuple(customMapping.SourceParameter, sourceAccess));

        bindSourceToDest = Expression.Bind(member.DestinationMember, customExpression);
      }
      else
      {
        Expression accessMember = Expression.MakeMemberAccess(sourceAccess, member.SourceMember);

        accessMember = HandleNullableValueTypes(member, accessMember);

        bindSourceToDest = Expression.Bind(member.DestinationMember, accessMember);
      }

      memberBindings.Add(bindSourceToDest);
    }

    private static Expression HandleNullableValueTypes(ProposedMemberMapping member, Expression accessMember)
    {
      if (member.DestinationMember.PropertyOrFieldType.IsNullableValueType() &&
        !member.SourceMember.PropertyOrFieldType.IsNullableValueType())
      {
        var nullableType = member
          .DestinationMember
          .PropertyOrFieldType
          .GetGenericArguments()
          .Single();

        accessMember = Expression.New(member.DestinationMember.PropertyOrFieldType.GetConstructor(new[] { nullableType }), accessMember);
      }
      else if (!member.DestinationMember.PropertyOrFieldType.IsNullableValueType()
        && member.SourceMember.PropertyOrFieldType.IsNullableValueType())
      {
        var nullableType = member.SourceMember.PropertyOrFieldType.GetGenericArguments().Single();


        accessMember = Expression.Condition(Expression.Property(accessMember, "HasValue"),
          Expression.Property(accessMember, "Value"),
          Expression.Default(member.DestinationMember.PropertyOrFieldType));
      }
      return accessMember;
    }
  }
}
