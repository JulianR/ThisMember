using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{
  public interface IMemberMapper
  {
    /// <summary>
    /// The strategy that determines how to map types.
    /// </summary>
    IMappingStrategy MappingStrategy { get; set; }

    /// <summary>
    /// The profile the map operates under, used in conjuction with a MapCollection
    /// </summary>
    string Profile { get; set; }

    /// <summary>
    /// Allows you to set various options with regards to mapping and map generation.
    /// </summary>
    /// <remarks>Note that any option you change on how maps are generated may not have any effect on the maps that 
    /// have already been generated. Those are changed only after you call CreateMap again or call ClearMapCache.</remarks>
    MapperOptions Options { get; set; }

    /// <summary>
    /// Where the IMemberMapper gets its IMapGenerator from.
    /// </summary>
    IMapGeneratorFactory MapGeneratorFactory { get; set; }

    TDestination Map<TDestination>(object source) where TDestination : new();

    TSource Map<TSource>(TSource source) where TSource : new();

    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    TDestination Map<TSource, TDestination, TParam>(TSource source, TDestination destination, TParam param);

    TDestination Map<TSource, TDestination, TParam>(TSource source, TParam param) where TDestination : new();

    object Map(object source, object destination);

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    ProposedMap CreateMapProposal(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null);

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null);

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping, MappingOptions options = null);

    /// <summary>
    /// Creates and finalizes a map that may no longer be modified.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    MemberMap CreateMap(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null);

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    MemberMap<TSource, TDestination> CreateMap<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null);

    /// <summary>
    /// Creates a map proposal that may be modified later.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    MemberMap<TSource, TDestination, TParam> CreateMap<TSource, TDestination, TParam>(Expression<Func<TSource, TParam, object>> customMapping, MappingOptions options = null);

    /// <summary>
    /// Creates and finalizes a projection from one type to another that may no longer be modified.
    /// </summary>
    /// <param name="source">The source type</param>
    /// <param name="destination">The destination type</param>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    Projection CreateProjection(Type source, Type destination, LambdaExpression customMapping = null, MappingOptions options = null);

    /// <summary>
    /// Creates and finalizes a projection from one type to another that may no longer be modified.
    /// </summary>
    /// <param name="options">The general mapping convention supplied by a user that is applied to all members</param>
    /// <param name="customMapping">A lambda expression that describes a custom map supplied by a user</param>
    /// <returns></returns>
    Projection<TSource, TDestination> CreateProjection<TSource, TDestination>(Expression<Func<TSource, object>> customMapping = null, MappingOptions options = null);

    bool HasMap<TSource, TDestination>();

    bool HasMap(Type source, Type destination);

    MemberMap<TSource, TDestination> GetMap<TSource, TDestination>();

    MemberMap GetMap(Type source, Type destination);

    MemberMap<TSource, TDestination, TParam> GetMap<TSource, TDestination, TParam>();

    bool TryGetMap<TSource, TDestination>(out MemberMap<TSource, TDestination> map);

    bool TryGetMap(Type source, Type destination, out MemberMap map);

    bool TryGetMap<TSource, TDestination, TParam>(out MemberMap<TSource, TDestination, TParam> map);

    bool HasProjection<TSource, TDestination>();

    bool HasProjection(Type source, Type destination);

    Projection<TSource, TDestination> GetProjection<TSource, TDestination>();

    Projection GetProjection(Type source, Type destination);

    bool TryGetProjection<TSource, TDestination>(out Projection<TSource, TDestination> map);

    bool TryGetProjection(Type source, Type destination, out Projection map);

    void RegisterMap(MemberMap map);

    void RegisterProjection(Projection projection);

    void ClearMapCache();

    /// <summary>
    /// Adds a constructor for a type that is used for all mappings
    /// </summary>
    IMemberMapper AddCustomConstructor<T>(Expression<Func<T>> ctor);

    /// <summary>
    /// Adds a constructor for a type that is used for all mappings
    /// </summary>
    IMemberMapper AddCustomConstructor(Type type, LambdaExpression ctor);

    LambdaExpression GetConstructor(Type t);

    /// <summary>
    /// Creates a deep clone of the given source object, mapping the entire object graph.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    TSource DeepClone<TSource>(TSource source) where TSource : new();

    /// <summary>
    /// A map repository is an optional location that is checked by the mapper for map definitions.
    /// </summary>
    IMapRepository MapRepository { get; set; }

    Expression<Func<TSource, TDestination>> Project<TSource, TDestination>();

    IProjectionGeneratorFactory ProjectionGeneratorFactory { get; set; }

    event Action<IMemberMapper, TypePair> BeforeMapping;

    event Action<IMemberMapper, TypePair, object> AfterMapping;

  }
}
