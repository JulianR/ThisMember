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
    private Func<TSource, TDestination, TDestination> mappingFunction;

    public new Func<TSource, TDestination, TDestination> MappingFunction
    {
      get
      {
        return this.mappingFunction;
      }
      set
      {
        this.mappingFunction = value;
        ((MemberMap)this).MappingFunction = value;
      }
    }

  }

  /// <summary>
  /// The final, compiled map for a certain source and destination type.
  /// </summary>
  public class MemberMap<TSource, TDestination, TParam> : MemberMap
  {
    private Func<TSource, TDestination, TParam, TDestination> mappingFunction;

    public new Func<TSource, TDestination, TParam, TDestination> MappingFunction
    {
      get
      {
        return this.mappingFunction;
      }
      set
      {
        this.mappingFunction = value;
        ((MemberMap)this).MappingFunction = value;
      }
    }

  }
}
