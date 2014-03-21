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
  public abstract class MemberMap
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

    private readonly Delegate mappingFunction;

    /// <summary>
    /// The delegate that is capable of performing the actual mapping
    /// </summary>
    public Delegate MappingFunction
    {
      get
      {
        return mappingFunction;
      }
    }

    public DebugInformation DebugInformation { get; set; }

    protected MemberMap(Type source, Type destination, Delegate mappingFunction)
    {
      this.sourceType = source;
      this.destinationType = destination;
      this.mappingFunction = mappingFunction;
    }

  }

  /// <summary>
  /// The final, compiled map for a certain source and destination type.
  /// </summary>
  public class MemberMap<TSource, TDestination> : MemberMap
  {
    private readonly Func<TSource, TDestination, TDestination> mappingFunction;

    public MemberMap(Func<TSource, TDestination, TDestination> mappingFunction)
      : base(typeof(TSource), typeof(TDestination), mappingFunction)
    {
      this.mappingFunction = mappingFunction;
    }

    public new Func<TSource, TDestination, TDestination> MappingFunction
    {
      get
      {
        return this.mappingFunction;
      }
    }

  }

  /// <summary>
  /// The final, compiled map for a certain source and destination type.
  /// </summary>
  public class MemberMap<TSource, TDestination, TParam> : MemberMap
  {
    private readonly Func<TSource, TDestination, TParam, TDestination> mappingFunction;

    public MemberMap(Func<TSource, TDestination, TParam, TDestination> mappingFunction)
      : base(typeof(TSource), typeof(TDestination), mappingFunction)
    {
      this.mappingFunction = mappingFunction;
    }

    public new Func<TSource, TDestination, TParam, TDestination> MappingFunction
    {
      get
      {
        return this.mappingFunction;
      }
    }

  }
}
