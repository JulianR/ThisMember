using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core;
using ThisMember.Core.Interfaces;

namespace ThisMember.ConsoleHost
{

  class Source
  {
    public int Value { get; set; }
    public string String { get; set; }
  }

  class Destination
  {
    public string String { get; set; }
    public int Val { get; set; }
  }

  class Visitor : ExpressionVisitor
  {

    private ParameterExpression oldParam, newParam;
    public Visitor(ParameterExpression oldParam, ParameterExpression newParam)
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
}
