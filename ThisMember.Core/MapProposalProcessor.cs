using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core
{

  public class ParameterTuple
  {
    public ParameterExpression OldParameter { get;set;}
    public ParameterExpression NewParameter { get;set;}

    public ParameterTuple(ParameterExpression oldParam, ParameterExpression newParam)
    {
      OldParameter = oldParam;
      NewParameter = newParam;
    }
  }

  public class MapProposalProcessor : ExpressionVisitor
  {
    public IList<ParameterTuple> Parameters { get; set; }

    public MapProposalProcessor()
    {
      Parameters = new List<ParameterTuple>();
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
      foreach (var param in Parameters)
      {
        if (param.OldParameter == node)
        {
          return param.NewParameter;
        }
      }

      return node;
    }
  }
}
