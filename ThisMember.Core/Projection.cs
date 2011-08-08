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

    public LambdaExpression Expression { get; set; }
  }

  /// <summary>
  /// Describes a projection from one type to another, as an expression.
  /// </summary>
  public class Projection<TSource, TDestination> : Projection
  {
    public new Expression<Func<TSource, TDestination>> Expression { get; set; }
  }
}
