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

  class Expressions
  {

    public static Delegate CreateMethod(Expression<Func<Source, Destination, object>> expr)
    {
      var x = CustomMapping.GetCustomMapping(typeof(Destination), expr);

      var lambda = expr as LambdaExpression;

      //var newType = lambda.Body as NewExpression;

      var source = Expression.Parameter(typeof(Source), "source");
      var dest = Expression.Parameter(typeof(Destination), "destination");

      var visitor = new Visitor(expr.Parameters[0], source);
      var visitor1 = new Visitor(expr.Parameters[1], dest);

      var newExp = visitor.Visit(expr);
      newExp = visitor1.Visit(newExp);

      var block = Expression.Block(expr, dest);

      var lambda1 = Expression.Lambda<Func<Source, Destination, Destination>>(block, source, dest);

      return lambda1.Compile();

    }
  }
}
