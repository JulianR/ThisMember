using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  /// <summary>
  /// Describes a projection from one type to another, as an expression.
  /// </summary>
  public class Projection
  {
    public Type SourceType { get; set; }

    public Type DestinationType { get; set; }

    private readonly LambdaExpression expression;

    public Projection(LambdaExpression expression)
    {
      this.expression = expression;
    }

    public LambdaExpression Expression
    {
      get
      {
        return expression;
      }
    }
  }

  /// <summary>
  /// Describes a projection from one type to another, as an expression.
  /// </summary>
  public class Projection<TSource, TDestination> : Projection
  {
    private readonly Expression<Func<TSource, TDestination>> expression;

    public Projection(Expression<Func<TSource, TDestination>> expression) : base(expression)
    {
      this.expression = expression;
    }

    public new Expression<Func<TSource, TDestination>> Expression
    {
      get
      {
        return this.expression;
      }
    }
  }
}
