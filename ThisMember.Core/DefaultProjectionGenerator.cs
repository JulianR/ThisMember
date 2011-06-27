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

    public DefaultProjectionGenerator(IMemberMapper mapper)
    {
      this.mapper = mapper;
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

      return lambda;
    }

    private MemberInitExpression BuildProjectionExpression(Expression sourceAccess, Type destinationType, ProposedTypeMapping proposedMap)
    {
      var memberBindings = new List<MemberBinding>();

      foreach (var member in proposedMap.ProposedMappings)
      {
        BuildMemberAssignmentExpressions(sourceAccess, memberBindings, member);
      }

      foreach (var complexMember in proposedMap.ProposedTypeMappings)
      {
        if (complexMember.IsEnumerable || CollectionTypeHelper.IsEnumerable(complexMember))
        {
          BuildCollectionComplexTypeExpression(sourceAccess, memberBindings, complexMember);
        }
        else
        {
          BuildComplexTypeExpression(sourceAccess, memberBindings, complexMember);
        }
      }

      var initDestination = Expression.MemberInit(Expression.New(destinationType), memberBindings);

      return initDestination;

    }

    private static MethodInfo selectMethod;

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


      BuildComplexTypeExpression(selectParam, bindings, complexMember, true);
    }

    private void BuildComplexTypeExpression(Expression sourceAccess, List<MemberBinding> memberBindings, ProposedTypeMapping complexMember, bool fromCollection = false)
    {
      Expression accessMember;

      if (!fromCollection)
      {
        accessMember = Expression.MakeMemberAccess(sourceAccess, complexMember.SourceMember);
      }
      else
      {
        accessMember = sourceAccess;
      }

      var memberInit = BuildProjectionExpression(accessMember, complexMember.DestinationMember.PropertyOrFieldType, complexMember);

      var bindSourceToDest = Expression.Bind(complexMember.DestinationMember, memberInit);
      memberBindings.Add(bindSourceToDest);
    }

    private static void BuildMemberAssignmentExpressions(Expression sourceAccess, List<MemberBinding> memberBindings, ProposedMemberMapping member)
    {
      if (member.Ignored)
      {
        return;
      }

      var accessMember = Expression.MakeMemberAccess(sourceAccess, member.SourceMember);

      var bindSourceToDest = Expression.Bind(member.DestinationMember, accessMember);

      memberBindings.Add(bindSourceToDest);
    }
  }
}
