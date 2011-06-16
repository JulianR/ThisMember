using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{

  public class ParameterTuple
  {
    public ParameterExpression OldParameter { get; set; }
    public ParameterExpression NewParameter { get; set; }

    public ParameterTuple(ParameterExpression oldParam, ParameterExpression newParam)
    {
      OldParameter = oldParam;
      NewParameter = newParam;
    }
  }

  public class MapProposalProcessor
  {
    private IMemberMapper mapper;

    public IList<ParameterTuple> ParametersToReplace { get; private set; }

    public bool NonPublicMembersAccessed { get; private set; }

    public MapProposalProcessor(IMemberMapper mapper)
    {
      ParametersToReplace = new List<ParameterTuple>();
      this.mapper = mapper;
    }

    

    private class ParameterVisitor : ExpressionVisitor
    {
      private IList<ParameterTuple> parameters;

      public ParameterVisitor(IList<ParameterTuple> parameters)
      {
        this.parameters = parameters;
      }

      private Expression ReplaceParameter(ParameterExpression parameter)
      {
        foreach (var param in parameters)
        {
          if (param.OldParameter == parameter)
          {
            return param.NewParameter;
          }
        }

        return parameter;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        return ReplaceParameter(node);
      }
    }

    public Expression Process(Expression expression)
    {
      var memberVisitor = new MemberVisitor(mapper);

      expression = memberVisitor.Visit(expression);

      var paramVisitor = new ParameterVisitor(this.ParametersToReplace);

      expression = paramVisitor.Visit(expression);

      var visibilityVisitor = new VisibilityVisitor();

      visibilityVisitor.Visit(expression);

      this.NonPublicMembersAccessed = visibilityVisitor.NonPublicMembersAccessed;

      return expression;
    }

    private class MemberVisitor : ExpressionVisitor
    {
      private IMemberMapper mapper;

      public MemberVisitor(IMemberMapper mapper)
      {
        this.mapper = mapper;
      }

      private static bool IsExceptionToNullCheck(MemberExpression memberNode)
      {
        if (memberNode.Member.Name == "Count"
          && memberNode.Member.DeclaringType.IsGenericType
          && typeof(ICollection<>).IsAssignableFrom(memberNode.Member.DeclaringType.GetGenericTypeDefinition()))
        {
          return true;
        }
        else if (memberNode.Member.Name == "Length"
          && memberNode.Member.DeclaringType == typeof(Array))
        {
          return true;
        }
        return false;
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

          if (memberNode.Expression.NodeType == ExpressionType.Parameter || IsExceptionToNullCheck(memberNode))
          {
            return expression;
          }
          else
          {
            newExpression = Expression.Condition(Expression.NotEqual(memberNode.Expression, Expression.Constant(null)),
              memberNode, Expression.Default(conditionalReturnType), conditionalReturnType);
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
      }
    }

    private class VisibilityVisitor : ExpressionVisitor
    {

      public bool NonPublicMembersAccessed { get; private set; }

      protected override Expression VisitConstant(ConstantExpression node)
      {
        if (!node.Type.IsPublic)
        {
          NonPublicMembersAccessed = true;

          return node;
        }

        return base.VisitConstant(node);
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
        if (!node.Method.IsPublic)
        {
          NonPublicMembersAccessed = true;
          return node;
        }

        // A nested class, we're taking no chances here
        if (!node.Method.DeclaringType.IsPublic || node.Method.DeclaringType.Name.Contains("+"))
        {
          NonPublicMembersAccessed = true;
          return node;
        }

        return base.VisitMethodCall(node);
      }

      protected override Expression VisitLambda<T>(Expression<T> node)
      {

        var delType = typeof(T);

        var genericParams = delType.GetGenericArguments();

        var sourceType = genericParams[0];

        var destType = genericParams[1];

        if (!IsPublicClass(sourceType))
        {
          this.NonPublicMembersAccessed = true;
          return node;
        }
        else if (!IsPublicClass(destType))
        {
          this.NonPublicMembersAccessed = true;
          return node;
        }


        return base.VisitLambda<T>(node);
      }

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
    }


  }
}
