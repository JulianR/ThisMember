using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using ThisMember.Core.Exceptions;
using ThisMember.Core.Options;

namespace ThisMember.Core
{
  public class MemberMapper : IMemberMapper
  {
    public MapperOptions Options { get; set; }

    public event Action<IMemberMapper, TypePair> BeforeMapping;
    public event Action<IMemberMapper, TypePair, object> AfterMapping;

    public IMapGeneratorFactory MapGeneratorFactory { get; set; }

    public IProjectionGeneratorFactory ProjectionGeneratorFactory { get; set; }

    private readonly Dictionary<TypePair, MemberMap> maps = new Dictionary<TypePair, MemberMap>();

    private readonly Dictionary<TypePair, Projection> projections = new Dictionary<TypePair, Projection>();


    public MemberMapper() : this(new DefaultMemberMapperConfiguration())
    {
    }

    public MemberMapper(IMemberMapperConfiguration config)
    {
      this.MappingStrategy = config.GetMappingStrategy(this);

      this.MapGeneratorFactory = config.GetMapGenerator(this);

      this.ProjectionGeneratorFactory = config.GetProjectionGenerator(this);

      this.Options = config.GetOptions(this);
    }

    public MemberMapper(MapperOptions options = null, IMappingStrategy strategy = null, IMapGeneratorFactory generator = null, IProjectionGeneratorFactory projection = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy(this);

      this.MapGeneratorFactory = generator ?? new CompiledMapGeneratorFactory();

      this.ProjectionGeneratorFactory = projection ?? new DefaultProjectionGeneratorFactory();

      this.Options = options ?? new MapperOptions();
    }

    private MemberMap<TSource, TDestination> ToGenericMemberMap<TSource, TDestination>(MemberMap map)
    {
      var newMap = new MemberMap<TSource, TDestination>();

      newMap.DestinationType = map.DestinationType;
      newMap.SourceType = map.SourceType;
      newMap.MappingFunction = map.MappingFunction as Func<TSource, TDestination, TDestination>;
      newMap.DebugInformation = map.DebugInformation;

      if (newMap.MappingFunction == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called without parameters. Use another overload of Map or recreate the map without a parameter.", typeof(TSource), typeof(TDestination)));
      }

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      lock (this.maps)
      {
        if (this.maps.ContainsKey(pair))
        {
          this.maps.Remove(pair);
          this.maps.Add(pair, newMap);
        }
      }
      return newMap;
    }

    private MemberMap<TSource, TDestination, TParam> ToGenericMemberMap<TSource, TDestination, TParam>(MemberMap map)
    {
      var newMap = new MemberMap<TSource, TDestination, TParam>();

      newMap.DestinationType = map.DestinationType;
      newMap.SourceType = map.SourceType;
      newMap.MappingFunction = map.MappingFunction as Func<TSource, TDestination, TParam, TDestination>;

      if (newMap.MappingFunction == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called without parameters. Use another overload of Map or recreate the map without a parameter.", typeof(TSource), typeof(TDestination)));
      }

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      lock (this.maps)
      {
        if (this.maps.ContainsKey(pair))
        {
          this.maps.Remove(pair);
          this.maps.Add(pair, newMap);
        }
      }
      return newMap;
    }

    private static bool TryToGenericMemberMap<TSource, TDestination>(MemberMap map, out MemberMap<TSource, TDestination> genericMap)
    {
      genericMap = new MemberMap<TSource, TDestination>();

      genericMap.DestinationType = map.DestinationType;
      genericMap.SourceType = map.SourceType;
      genericMap.MappingFunction = map.MappingFunction as Func<TSource, TDestination, TDestination>;

      if (genericMap.MappingFunction == null)
      {
        return false;
      }

      ((MemberMap)genericMap).MappingFunction = map.MappingFunction;

      return true;
    }

    private static bool TryToGenericMemberMap<TSource, TDestination, TParam>(MemberMap map, out MemberMap<TSource, TDestination, TParam> genericMap)
    {
      genericMap = new MemberMap<TSource, TDestination, TParam>();

      genericMap.DestinationType = map.DestinationType;
      genericMap.SourceType = map.SourceType;
      genericMap.MappingFunction = map.MappingFunction as Func<TSource, TDestination, TParam, TDestination>;

      if (genericMap.MappingFunction == null)
      {
        return false;
      }

      ((MemberMap)genericMap).MappingFunction = map.MappingFunction;

      return true;
    }

    private static Projection<TSource, TDestination> ToGenericProjection<TSource, TDestination>(Projection projection)
    {
      var newMap = new Projection<TSource, TDestination>();

      newMap.DestinationType = projection.DestinationType;
      newMap.SourceType = projection.SourceType;
      newMap.Expression = (Expression<Func<TSource, TDestination>>)projection.Expression;

      ((Projection)newMap).Expression = projection.Expression;

      return newMap;
    }

    public TDestination Map<TDestination>(object source) where TDestination : new()
    {
      if (source == null)
      {
        var option = this.Options.Safety.IfSourceIsNull;

        if (option == SourceObjectNullOptions.ReturnDestinationObject)
        {
          return new TDestination();
        }
        else if (option == SourceObjectNullOptions.ReturnNullWhenSourceIsNull)
        {
          return default(TDestination);
        }
      }

      var pair = new TypePair(source.GetType(), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair).FinalizeMap();
      }

      var destination = new TDestination();

      if (BeforeMapping != null)
      {
        BeforeMapping(this, pair);
      }

      var result = (TDestination)map.MappingFunction.DynamicInvoke(source, destination);

      if (AfterMapping != null)
      {
        AfterMapping(this, pair, result);
      }

      return result;
    }

    public TDestination Map<TSource, TDestination>(TSource source) where TDestination : new()
    {
      TDestination destination = new TDestination();

      return Map(source, destination);
    }

    public TDestination Map<TSource, TDestination, TParam>(TSource source, TParam param) where TDestination : new()
    {
      TDestination destination = new TDestination();

      return Map(source, destination, param);
    }

    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null)
    {
      var proposedMap = this.MappingStrategy.CreateMapProposal<TSource, TDestination>(options, customMapping);

      return proposedMap;
    }

    public ProposedMap CreateMapProposal(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null)
    {
      var pair = new TypePair(source, destination);

      var proposedMap = this.MappingStrategy.CreateMapProposal(pair, options, customMapping);

      return proposedMap;
    }

    public ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping = null, MappingOptions options = null)
    {
      var proposedMap = this.MappingStrategy.CreateMapProposal<TSource, TDestination, TParam>(options, customMapping);

      return proposedMap;
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
      if (destination == null && this.Options.Safety.ThrowIfDestinationIsNull)
      {
        throw new ArgumentNullException("destination");
      }

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair).FinalizeMap();
      }

      if (BeforeMapping != null) BeforeMapping(this, pair);

      var func = map.MappingFunction as Func<TSource, TDestination, TDestination>;

      if (func == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called without parameters. Use another overload of Map or recreate the map without a parameter.", pair.SourceType, pair.DestinationType));
      }

      var result = func(source, destination);

      if (AfterMapping != null) AfterMapping(this, pair, result);

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

    public MemberMap CreateMap(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null)
    {
      return CreateMapProposal(source, destination, customMapping, options).FinalizeMap();
    }

    public MemberMap<TSource, TDestination> CreateMap<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null)
    {
      return ToGenericMemberMap<TSource, TDestination>(CreateMapProposal<TSource, TDestination>(customMapping, options).FinalizeMap());
    }

    public MemberMap<TSource, TDestination, TParam> CreateMap<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping, MappingOptions options = null)
    {
      return ToGenericMemberMap<TSource, TDestination, TParam>(CreateMapProposal<TSource, TDestination, TParam>(customMapping, options).FinalizeMap());
    }

    public Projection CreateProjection(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null)
    {
      return CreateMapProposal(source, destination, customMapping, options).FinalizeProjection();
    }

    public Projection<TSource, TDestination> CreateProjection<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null)
    {
      return ToGenericProjection<TSource, TDestination>(CreateMapProposal<TSource, TDestination>(customMapping, options).FinalizeProjection());
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

    public MemberMap<TSource, TDestination, TParam> GetMap<TSource, TDestination, TParam>()
    {
      MemberMap map;

      if (!this.maps.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out map))
      {
        throw new MapNotFoundException(typeof(TSource), typeof(TDestination));
      }

      var genericMap = map as MemberMap<TSource, TDestination, TParam>;

      return genericMap ?? ToGenericMemberMap<TSource, TDestination, TParam>(map);
    }

    public bool TryGetMap<TSource, TDestination>(out MemberMap<TSource, TDestination> map)
    {
      MemberMap nonGeneric;

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      if (this.maps.TryGetValue(pair, out nonGeneric))
      {
        map = nonGeneric as MemberMap<TSource, TDestination>;

        if (map == null)
        {
          var success = TryToGenericMemberMap<TSource, TDestination>(nonGeneric, out map);

          if (!success)
          {
            return false;
          }

          lock (this.maps)
          {
            this.maps.Remove(pair);
            this.maps.Add(pair, map);
          }
        }

        return true;
      }

      map = null;

      return false;
    }

    public bool TryGetMap(Type source, Type destination, out MemberMap map)
    {
      if (!this.maps.TryGetValue(new TypePair(source, destination), out map))
      {
        return false;
      }
      return true;
    }

    public bool TryGetMap<TSource, TDestination, TParam>(out MemberMap<TSource, TDestination, TParam> map)
    {
      MemberMap nonGeneric;

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      if (this.maps.TryGetValue(pair, out nonGeneric))
      {
        map = nonGeneric as MemberMap<TSource, TDestination, TParam>;

        if (map == null)
        {
          var success = TryToGenericMemberMap<TSource, TDestination, TParam>(nonGeneric, out map);

          if (!success)
          {
            return false;
          }

          lock (this.maps)
          {
            this.maps.Remove(pair);
            this.maps.Add(pair, map);
          }
        }

        return true;
      }

      map = null;

      return false;
    }


    public bool HasProjection<TSource, TDestination>()
    {
      return this.projections.ContainsKey(new TypePair(typeof(TSource), typeof(TDestination)));
    }

    public bool HasProjection(Type source, Type destination)
    {
      return this.projections.ContainsKey(new TypePair(source, destination));
    }

    public Projection<TSource, TDestination> GetProjection<TSource, TDestination>()
    {
      Projection projection;

      if (!this.projections.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out projection))
      {
        throw new MapNotFoundException(typeof(TSource), typeof(TDestination));
      }

      var genericProjection = projection as Projection<TSource, TDestination>;

      return genericProjection ?? ToGenericProjection<TSource, TDestination>(projection);
    }

    public Projection GetProjection(Type source, Type destination)
    {
      Projection projection;

      if (!this.projections.TryGetValue(new TypePair(source, destination), out projection))
      {
        throw new MapNotFoundException(source, destination);
      }
      return projection;
    }

    public bool TryGetProjection<TSource, TDestination>(out Projection<TSource, TDestination> map)
    {
      Projection nonGeneric;

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      if (this.projections.TryGetValue(pair, out nonGeneric))
      {
        map = nonGeneric as Projection<TSource, TDestination>;

        if (map == null)
        {
          map = ToGenericProjection<TSource, TDestination>(nonGeneric);

          lock (this.maps)
          {
            this.projections.Remove(pair);
            this.projections.Add(pair, map);
          }
        }

        return true;
      }

      map = null;

      return false;
    }

    public bool TryGetProjection(Type source, Type destination, out Projection map)
    {
      if (!this.projections.TryGetValue(new TypePair(source, destination), out map))
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


    public TSource DeepClone<TSource>(TSource source) where TSource : new()
    {
      if (!this.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
      {
        throw new InvalidOperationException("This mapper has been configured not to perform any cloning by setting Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource to false");
      }

      return Map<TSource, TSource>(source);
    }

    public IMapRepository MapRepository { get; set; }

    public Expression<Func<TSource, TDestination>> Project<TSource, TDestination>()
    {
      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      Projection projection;

      if (!this.projections.TryGetValue(pair, out projection))
      {
        projection = MappingStrategy.CreateMapProposal(pair).FinalizeProjection();
      }
      return (Expression<Func<TSource, TDestination>>)projection.Expression;
    }

    public void RegisterProjection(Projection projection)
    {
      this.projections[new TypePair(projection.SourceType, projection.DestinationType)] = projection;
    }


    public object Map(object source, object destination)
    {
      if (destination == null && this.Options.Safety.ThrowIfDestinationIsNull)
      {
        throw new ArgumentNullException("destination");
      }

      var pair = new TypePair(source.GetType(), destination.GetType());

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair).FinalizeMap();
      }
      if (BeforeMapping != null) BeforeMapping(this, pair);

      var result = map.MappingFunction.DynamicInvoke(source, destination);

      if (AfterMapping != null) AfterMapping(this, pair, result);

      return result;
    }

    public TDestination Map<TSource, TDestination, TParam>(TSource source, TDestination destination, TParam param)
    {
      if (destination == null && this.Options.Safety.ThrowIfDestinationIsNull)
      {
        throw new ArgumentNullException("destination");
      }

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      MemberMap map;

      if (!this.maps.TryGetValue(pair, out map))
      {
        map = MappingStrategy.CreateMapProposal(pair, parameters: typeof(TParam)).FinalizeMap();
      }
      if (BeforeMapping != null) BeforeMapping(this, pair);

      var func = map.MappingFunction as Func<TSource, TDestination, TParam, TDestination>;

      if (func == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called with a parameter. Use another overload of Map or recreate the map with a parameter.", pair.SourceType, pair.DestinationType));
      }

      var result = func(source, destination, param);

      if (AfterMapping != null) AfterMapping(this, pair, result);

      return result;
    }

  }
}
