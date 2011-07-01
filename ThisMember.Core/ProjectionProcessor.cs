using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  internal class ProjectionProcessor
  {
    public IList<ParameterTuple> ParametersToReplace { get; private set; }

    public IMemberMapper MemberMapper { get; private set; }

    public ProjectionProcessor(IMemberMapper mapper)
    {
      ParametersToReplace = new List<ParameterTuple>();
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
  }
}
