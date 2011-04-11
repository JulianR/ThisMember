using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class MemberMap
  {
    public MemberMap FinalizeMap()
    {
      return this;
    }

    public ProposedTypeMapping ProposedTypeMapping
    {
      get { throw new NotImplementedException(); }
      set { }
    }

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

    public Delegate MappingFunction
    {
      get;
      set;
    }

    public MemberMap<TSource, TDestination> ToGeneric<TSource, TDestination>()
    {
      var map = new MemberMap<TSource, TDestination>();

      map.DestinationType = this.DestinationType;
      map.SourceType = this.SourceType;
      map.MappingFunction = (Func<TSource, TDestination, TDestination>) this.MappingFunction;

      return map;
    }

    public IMapGenerator MapGenerator
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }


  }

  public class MemberMap<TSource, TDestination> : MemberMap
  {

    public ProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(System.Linq.Expressions.Expression<Func<TSource, TSourceReturn>> source, System.Linq.Expressions.Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      throw new NotImplementedException();
    }

    public new Func<TSource, TDestination, TDestination> MappingFunction
    {
      get;
      set;
    }

  }
}
