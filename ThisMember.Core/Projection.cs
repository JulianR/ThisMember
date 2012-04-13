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
  public abstract class Projection
  {
    private readonly Type sourceType, destinationType;

    public Type SourceType
    {
      get
      {
        return sourceType;
      }
    }

    public Type DestinationType
    {
      get
      {
        return destinationType;
      }
    }

    private readonly LambdaExpression expression;

    public Projection(Type sourceType, Type destinationType, LambdaExpression expression)
    {
      this.sourceType = sourceType;
      this.destinationType = destinationType;
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

    public Projection(Expression<Func<TSource, TDestination>> expression)
      : base(typeof(TSource), typeof(TDestination), expression)
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
