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

      return paramVisitor.Visit(expression);
    }

    private class MemberVisitor : ExpressionVisitor
    {
      private IMemberMapper mapper;

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

          if (memberNode.Expression.NodeType == ExpressionType.Parameter)
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


  }
}
