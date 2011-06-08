using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public abstract class MapRepositoryBase : IMapRepository
  {

    private class MapFuncWrapper
    {
      public Func<IMemberMapper, MappingOptions, ProposedMap> CreateMapFunction { get; set; }
      public bool InUse { get; set; }
    }

    private Dictionary<TypePair, MapFuncWrapper> cache = new Dictionary<TypePair, MapFuncWrapper>();

    public MapRepositoryBase()
    {
      InitMaps();
    }

    protected abstract void InitMaps();

    //protected void CreateMap<TSource, TDestination>(Func<IMemberMapper, MappingOptions, ProposedMap> action)
    //{
    //  cache.Add(new TypePair(typeof(TSource), typeof(TDestination)), new MapFuncWrapper { CreateMapFunction = action });
    //}

    protected void DefineMap<TSource, TDestination>(Func<IMemberMapper, MappingOptions, ProposedMap<TSource, TDestination>> action)
    {
      cache.Add(new TypePair(typeof(TSource), typeof(TDestination)), new MapFuncWrapper { CreateMapFunction = action });
    }

    public bool TryGetMap(IMemberMapper mapper, MappingOptions options, TypePair pair, out ProposedMap map)
    {
      MapFuncWrapper action;
      if (cache.TryGetValue(pair, out action))
      {

        lock (action)
        {

          if (action.InUse)
          {
            map = null;
            return false;
          }

          try
          {
            action.InUse = true;
            map = action.CreateMapFunction(mapper, options);
          }
          finally
          {
            action.InUse = false;
          }
          return true;
        }
      }

      map = null;

      return false;
    }


    public bool TryGetMap<TSource, TDestination>(IMemberMapper mapper, MappingOptions options, out ProposedMap<TSource, TDestination> map)
    {
      MapFuncWrapper action;
      if (cache.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out action))
      {
        lock (action)
        {

          if (action.InUse)
          {
            map = null;
            return false;
          }

          try
          {
            action.InUse = true;
            map = (ProposedMap<TSource, TDestination>)action.CreateMapFunction(mapper, options);
          }
          finally
          {
            action.InUse = false;
          }
          return true;
        }
      }

      map = null;

      return false;
    }
  }
}
