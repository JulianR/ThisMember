using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  internal class ProjectionExpressionTuple
  {
    public ParameterExpression OldParameter { get; set; }
    public Expression NewExpression { get; set; }

    public ProjectionExpressionTuple(ParameterExpression oldParam, Expression newParam)
    {
      OldParameter = oldParam;
      NewExpression = newParam;
    }
  }

  internal class ProjectionProcessor
  {
    public IList<ProjectionExpressionTuple> ParametersToReplace { get; private set; }

    public IMemberMapper MemberMapper { get; private set; }

    public ProjectionProcessor(IMemberMapper mapper)
    {
      ParametersToReplace = new List<ProjectionExpressionTuple>();
      this.MemberMapper = mapper;
    }

    public Expression Process(Expression expression)
    {
      var paramVisitor = new ParameterVisitor(this.ParametersToReplace);

      expression = paramVisitor.Visit(expression);

      return expression;
    }

    private class ParameterVisitor : ExpressionVisitor
    {
      private IList<ProjectionExpressionTuple> parameters;

      public ParameterVisitor(IList<ProjectionExpressionTuple> parameters)
      {
        this.parameters = parameters;
      }

      private Expression ReplaceParameter(ParameterExpression parameter)
      {
        foreach (var param in parameters)
        {
          if (param.OldParameter == parameter)
          {
            return param.NewExpression;
          }
        }

        return parameter;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        return ReplaceParameter(node);
      }
    }
  }
}
