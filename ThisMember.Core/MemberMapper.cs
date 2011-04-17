using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using ThisMember.Core.Exceptions;

namespace ThisMember.Core
{

  public class MemberMapper : IMemberMapper
  {
    public MapperOptions Options { get; set; }

    public MemberMapper(MapperOptions options = null, IMappingStrategy strategy = null, IMapGenerator generator = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy(this);

      this.MappingStrategy.MapGenerator = generator ?? new CompiledMapGenerator(this);

      this.Options = options ?? new MapperOptions();
    }

    private Dictionary<TypePair, MemberMap> maps = new Dictionary<TypePair, MemberMap>();

    private static MemberMap<TSource, TDestination> ToGenericMemberMap<TSource, TDestination>(MemberMap map)
    {
      var newMap = new MemberMap<TSource, TDestination>();

      newMap.DestinationType = map.DestinationType;
      newMap.SourceType = map.SourceType;
      newMap.MappingFunction = (Func<TSource, TDestination, TDestination>)map.MappingFunction;

      return newMap;
    }

    public TDestination Map<TDestination>(object source) where TDestination : new()
    {
      var pair = new TypePair(source.GetType(), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair).FinalizeMap();
      }

      var destination = new TDestination();

      if (Options.BeforeMapping != null) Options.BeforeMapping(this, pair);

      var result = (TDestination)map.MappingFunction.DynamicInvoke(source, destination);

      if (Options.AfterMapping != null) Options.AfterMapping(this, pair, result);

      return result;
    }

    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : new()
    {
      TDestination destination = default(TDestination);

      if (source != null)
      {
        destination = new TDestination();
      }

      return Map(source, destination);
    }

    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      var proposedMap = this.MappingStrategy.CreateMapProposal<TSource, TDestination>(options, customMapping);

      return proposedMap;
    }

    public ProposedMap CreateMapProposal(Type source, Type destination, MappingOptions options = null)
    {

      var pair = new TypePair(source, destination);

      var proposedMap = this.MappingStrategy.CreateMapProposal(pair, options);

      return proposedMap;

    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair).FinalizeMap();
      }
      if (Options.BeforeMapping != null) Options.BeforeMapping(this, pair);

      var result = ((Func<TSource, TDestination, TDestination>)map.MappingFunction)(source, destination);

      if (Options.AfterMapping != null) Options.AfterMapping(this, pair, result);

      return result;

    }


    public void RegisterMap(MemberMap map)
    {
      this.maps[new TypePair(map.SourceType, map.DestinationType)] = map;
    }

    public TSource Map<TSource>(TSource source) where TSource : new()
    {
      var destination = new TSource();

      return Map(source, destination);
    }

    public IMappingStrategy MappingStrategy { get; set; }

    public MemberMap CreateMap(Type source, Type destination, MappingOptions options = null)
    {
      return CreateMapProposal(source, destination, options).FinalizeMap();
    }

    public MemberMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      return ToGenericMemberMap<TSource, TDestination>(CreateMapProposal<TSource, TDestination>(options, customMapping).FinalizeMap());
    }

    public bool HasMap<TSource, TDestination>()
    {
      return this.maps.ContainsKey(new TypePair(typeof(TSource), typeof(TDestination)));
    }

    public bool HasMap(Type source, Type destination)
    {
      return this.maps.ContainsKey(new TypePair(source, destination));
    }

    public MemberMap<TSource, TDestination> GetMap<TSource, TDestination>()
    {
      MemberMap map;

      if (!this.maps.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out map))
      {
        throw new MapNotFoundException(typeof(TSource), typeof(TDestination));
      }

      var genericMap = map as MemberMap<TSource, TDestination>;

      return genericMap ?? ToGenericMemberMap<TSource, TDestination>(map);
    }

    public MemberMap GetMap(Type source, Type destination)
    {
      MemberMap map;

      if (!this.maps.TryGetValue(new TypePair(source, destination), out map))
      {
        throw new MapNotFoundException(source, destination);
      }
      return map;
    }

    public bool TryGetMap<TSource, TDestination>(out MemberMap<TSource, TDestination> map)
    {
      MemberMap nonGeneric;

      if (!this.maps.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out nonGeneric))
      {
        map = nonGeneric as MemberMap<TSource, TDestination>;

        if (map == null)
        {
          map = ToGenericMemberMap<TSource, TDestination>(map);
        }

        return false;
      }

      map = null;

      return true;
    }

    public bool TryGetMap(Type source, Type destination, out MemberMap map)
    {
      if (!this.maps.TryGetValue(new TypePair(source, destination), out map))
      {
        return false;
      }
      return true;
    }

    public void ClearMapCache()
    {
      this.maps.Clear();
      this.MappingStrategy.ClearMapCache();
    }

    private Dictionary<Type, LambdaExpression> constructorCache = new Dictionary<Type, LambdaExpression>();

    public IMemberMapper AddCustomConstructor<T>(Expression<Func<T>> ctor)
    {
      constructorCache[typeof(T)] = ctor;
      return this;
    }

    public IMemberMapper AddCustomConstructor(Type type, LambdaExpression ctor)
    {
      constructorCache[type] = ctor;
      return this;
    }

    public LambdaExpression GetConstructor(Type t)
    {
      LambdaExpression e;
      constructorCache.TryGetValue(t, out e);
      return e;
    }

    public string Profile { get; set; }
  }
}
