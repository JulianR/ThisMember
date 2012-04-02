using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  /// <summary>
  /// The base class that you should inherit from if you wish to create your own IMapRepository implementation.
  /// This class takes care of certain difficulties for you. 
  /// </summary>
  public abstract class MapRepositoryBase : IMapRepository
  {

    private class MapFuncWrapper
    {
      public Func<IMemberMapper, MemberOptions, ProposedMap> CreateMapFunction { get; set; }
      public bool InUse { get; set; }
    }

    private Dictionary<TypePair, MapFuncWrapper> cache = new Dictionary<TypePair, MapFuncWrapper>();

    public MapRepositoryBase()
    {
      InitMaps();
    }

    protected abstract void InitMaps();

    /// <summary>
    /// Defines what a map should look like for a certain source and destination type. 
    /// You do this by providing a function that creates it on the IMemberMapper. This does not yet create
    /// the map, just tells the IMemberMapper how to do so. 
    /// </summary>
    /// <param name="action">The function that describes how the map should be created.</param>
    protected void DefineMap<TSource, TDestination>(Func<IMemberMapper, MemberOptions, ProposedMap<TSource, TDestination>> action)
    {
      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      lock (cache)
      {
        if (!cache.ContainsKey(pair))
        {
          cache.Add(pair, new MapFuncWrapper { CreateMapFunction = action });
        }
        else
        {
          throw new InvalidOperationException("Map repository already contains map for types " + pair);
        }
      }
    }

    /// <summary>
    /// Checks if the mapper repository contains a map and if so, returns it as an out parameter.
    /// </summary>
    /// <returns></returns>
    public bool TryGetMap(IMemberMapper mapper, MemberOptions options, TypePair pair, out ProposedMap map)
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

    /// <summary>
    /// Checks if the mapper repository contains a map and if so, returns it as an out parameter.
    /// </summary>
    /// <returns></returns>
    public bool TryGetMap<TSource, TDestination>(IMemberMapper mapper, MemberOptions options, out ProposedMap<TSource, TDestination> map)
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
