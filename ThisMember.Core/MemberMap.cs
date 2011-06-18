using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  /// <summary>
  /// The final, compiled map for a certain source and destination type.
  /// </summary>
  public class MemberMap
  {

    public Type SourceType
    {
      get;
      set;
    }

    public Type DestinationType
    {
      get;
      set;
    }

    /// <summary>
    /// The delegate that is capable of performing the actual mapping
    /// </summary>
    public Delegate MappingFunction
    {
      get;
      set;
    }

  }

  /// <summary>
  /// The final, compiled map for a certain source and destination type.
  /// </summary>
  public class MemberMap<TSource, TDestination> : MemberMap
  {

    public new Func<TSource, TDestination, TDestination> MappingFunction
    {
      get;
      set;
    }

  }
}
