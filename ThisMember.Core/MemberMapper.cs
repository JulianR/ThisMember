using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using ThisMember.Core.Exceptions;
using ThisMember.Core.Options;
using ThisMember.Core.Misc;
using System.Threading;

namespace ThisMember.Core
{
  public class MemberMapper : IMemberMapper
  {
    /// <summary>
    /// Allows you to set various options with regards to mapping and map generation.
    /// 
    /// Note that any option you change on how maps are generated may not have any effect on the maps that 
    /// have already been generated. Those are changed only after you call CreateMap again or call ClearMapCache.
    /// </summary>
    public MapperOptions Options { get; set; }

    public event Action<IMemberMapper, TypePair, object> BeforeMapping;
    public event Action<IMemberMapper, TypePair, object> AfterMapping;

    /// <summary>
    /// Where the IMemberMapper gets its IMapGenerator from.
    /// </summary>
    public IMapGeneratorFactory MapGeneratorFactory { get; set; }

    public IProjectionGeneratorFactory ProjectionGeneratorFactory { get; set; }

    private Dictionary<TypePair, MemberMap> maps = new Dictionary<TypePair, MemberMap>();

    private byte[] mapsWriteLock = new byte[0];
    private byte[] projectionsWriteLock = new byte[0];

    private Dictionary<TypePair, Projection> projections = new Dictionary<TypePair, Projection>();


    public MemberMapper()
      : this(new DefaultMemberMapperConfiguration())
    {
    }

    public MemberMapper(IMemberMapperConfiguration config)
    {
      this.MappingStrategy = config.GetMappingStrategy(this);

      this.MapGeneratorFactory = config.GetMapGenerator(this);

      this.ProjectionGeneratorFactory = config.GetProjectionGenerator(this);

      this.Options = config.GetOptions(this);

      this.Data = new MapperDataAccessor(this);
    }

    public MemberMapper(MapperOptions options = null, IMappingStrategy strategy = null, IMapGeneratorFactory generator = null, IProjectionGeneratorFactory projection = null)
    {
      this.MappingStrategy = strategy ?? new DefaultMappingStrategy(this);

      this.MapGeneratorFactory = generator ?? new CompiledMapGeneratorFactory();

      this.ProjectionGeneratorFactory = projection ?? new DefaultProjectionGeneratorFactory();

      this.Options = options ?? new MapperOptions();

      this.Data = new MapperDataAccessor(this);
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
        BeforeMapping(this, pair, source);
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

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MemberOptions options = null)
    {
      var proposedMap = this.MappingStrategy.CreateMapProposal<TSource, TDestination>(options, customMapping);

      return proposedMap;
    }

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public ProposedMap CreateMapProposal(Type source, Type destination, LambdaExpression customMapping = null, MemberOptions options = null)
    {
      var pair = new TypePair(source, destination);

      var proposedMap = this.MappingStrategy.CreateMapProposal(pair, options, customMapping);

      return proposedMap;
    }

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping = null, MemberOptions options = null)
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

      if (BeforeMapping != null) BeforeMapping(this, pair, source);

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
      lock (mapsWriteLock)
      {
        var newMaps = new Dictionary<TypePair, MemberMap>(this.maps);
        newMaps[new TypePair(map.SourceType, map.DestinationType)] = map;
        Interlocked.CompareExchange(ref this.maps, newMaps, this.maps);
      }
    }

    public TSource Map<TSource>(TSource source) where TSource : new()
    {
      var destination = new TSource();

      return Map(source, destination);
    }

    /// <summary>
    /// The strategy that determines how to map types.
    /// </summary>
    public IMappingStrategy MappingStrategy { get; set; }

    /// <summary>
    /// Creates and finalizes a map that may no longer be modified.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public MemberMap CreateMap(Type source, Type destination, LambdaExpression customMapping = null, MemberOptions options = null)
    {
      return CreateMapProposal(source, destination, customMapping, options).FinalizeMap();
    }

    /// <summary>
    /// Creates and finalizes a map that may no longer be modified.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public MemberMap<TSource, TDestination> CreateMap<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MemberOptions options = null)
    {
      return (MemberMap<TSource, TDestination>)CreateMapProposal<TSource, TDestination>(customMapping, options).FinalizeMap();
    }

    /// <summary>
    /// Creates and finalizes a map that may no longer be modified.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public MemberMap<TSource, TDestination, TParam> CreateMap<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping = null, MemberOptions options = null)
    {
      return (MemberMap<TSource, TDestination, TParam>)CreateMapProposal<TSource, TDestination, TParam>(customMapping, options).FinalizeMap();
    }

    /// <summary>
    /// Creates and finalizes a projection from one type to another that may no longer be modified.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public Projection CreateProjection(Type source, Type destination, LambdaExpression customMapping = null, MemberOptions options = null)
    {
      return CreateMapProposal(source, destination, customMapping, options).FinalizeProjection();
    }

    /// <summary>
    /// Creates and finalizes a projection from one type to another that may no longer be modified.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    public Projection<TSource, TDestination> CreateProjection<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MemberOptions options = null)
    {
      return (Projection<TSource, TDestination>)CreateMapProposal<TSource, TDestination>(customMapping, options).FinalizeProjection();
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

      if (genericMap == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called without parameters. Use another overload of Map or recreate the map without a parameter.", typeof(TSource), typeof(TDestination)));
      }

      return genericMap;
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

      if (genericMap == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called with parameters. Use another overload of Map or recreate the map with a parameter.", typeof(TSource), typeof(TDestination)));
      }

      return genericMap;
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
          return false;
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
          return false;
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

      return genericProjection;
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
      Projection projection;

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      if (this.projections.TryGetValue(pair, out projection))
      {
        map = projection as Projection<TSource, TDestination>;

        if (map == null)
        {
          return false;
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

    /// <summary>
    /// Clears out the map cache, forcing all maps to be recreated.
    /// </summary>
    public void ClearMapCache()
    {
      lock (this.mapsWriteLock)
      {
        var newMaps = new Dictionary<TypePair, MemberMap>();

        Interlocked.CompareExchange(ref this.maps, newMaps, this.maps);
      }

      this.MappingStrategy.ClearMapCache();
    }

    /// <summary>
    /// Adds a constructor for a type that is used for all mappings
    /// </summary>
    public IMemberMapper AddCustomConstructor<T>(Expression<Func<T>> ctor)
    {
      return AddCustomConstructor(typeof(T), ctor);
    }

    /// <summary>
    /// Adds a constructor for a type that is used for all mappings
    /// </summary>
    public IMemberMapper AddCustomConstructor(Type type, LambdaExpression ctor)
    {
      this.Data.AddCustomConstructor(type, ctor);

      return this;
    }

    private LambdaExpression GetConstructor(Type t)
    {
      return this.Data.GetConstructor(t);
    }

    /// <summary>
    /// The profile the map operates under, used in conjuction with a MapCollection
    /// </summary>
    public string Profile { get; set; }

    /// <summary>
    /// Creates a deep clone of the given source object, mapping the entire object graph.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public object DeepClone(object obj)
    {
      if (!this.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
      {
        throw new InvalidOperationException("This mapper has been configured not to perform any cloning by setting Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource to false");
      }

      if (obj == null)
      {
        throw new ArgumentNullException("obj");
      }

      var type = obj.GetType();

      var instance = Activator.CreateInstance(type);

      return Map(obj, instance);
    }

    /// <summary>
    /// Creates a deep clone of the given source object, mapping the entire object graph.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public TSource DeepClone<TSource>(TSource source) where TSource : new()
    {
      if (!this.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
      {
        throw new InvalidOperationException("This mapper has been configured not to perform any cloning by setting Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource to false");
      }

      return Map<TSource, TSource>(source);
    }

    /// <summary>
    /// A map repository is an optional location that is checked by the mapper for map definitions.
    /// </summary>
    public IMapRepository MapRepository { get; set; }

    /// <summary>
    /// Creates an expression that returns TDestination as output from a given TSource (a 'projection').
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
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
      lock (projectionsWriteLock)
      {
        var newProjections = new Dictionary<TypePair, Projection>(projections);
        newProjections[new TypePair(projection.SourceType, projection.DestinationType)] = projection;
        Interlocked.CompareExchange(ref this.projections, newProjections, this.projections);
      }
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
      if (BeforeMapping != null) BeforeMapping(this, pair, source);

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
        map = MappingStrategy.CreateMapProposal<TSource, TDestination, TParam>().FinalizeMap();
      }
      if (BeforeMapping != null) BeforeMapping(this, pair, source);

      var func = map.MappingFunction as Func<TSource, TDestination, TParam, TDestination>;

      if (func == null)
      {
        throw new InvalidOperationException(string.Format("The mapping from {0} to {1} is not configured to be called with a parameter. Use another overload of Map or recreate the map with a parameter.", pair.SourceType, pair.DestinationType));
      }

      var result = func(source, destination, param);

      if (AfterMapping != null) AfterMapping(this, pair, result);

      return result;
    }

    public Fluent.SourceTypeModifier<TSource> ForSourceType<TSource>()
    {
      return new Fluent.SourceTypeModifier<TSource>(this);
    }

    public Misc.MapperDataAccessor Data
    {
      get;
      private set;
    }

    public MemberOptions DefaultMemberOptions { get; set; }

    public Fluent.DestinationTypeModifier<TDestination> ForDestinationType<TDestination>()
    {
      return new Fluent.DestinationTypeModifier<TDestination>(this);
    }
  }
}
