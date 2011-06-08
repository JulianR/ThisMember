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
    IMappingStrategy MappingStrategy { get; set; }

    string Profile { get; set; }

    /// <summary>
    /// Allows you to set various options with regards to mapping and map generation.
    /// </summary>
    /// <remarks>Note that any option you change on how maps are generated may not have any effect on the maps that 
    /// have already been generated. Those are changed only after you call CreateMap again or call ClearMapCache.</remarks>
    MapperOptions Options { get; set; }

    IMapGenerator MapGenerator { get; set; }

    TDestination Map<TDestination>(object source) where TDestination : new();

    TSource Map<TSource>(TSource source) where TSource : new();

    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    ProposedMap CreateMapProposal(Type source, Type destination, MappingOptions options = null, LambdaExpression customMapping = null);

    ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    MemberMap CreateMap(Type source, Type destination, MappingOptions options = null, LambdaExpression customMapping = null);

    MemberMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    bool HasMap<TSource, TDestination>();

    bool HasMap(Type source, Type destination);

    MemberMap<TSource, TDestination> GetMap<TSource, TDestination>();

    MemberMap GetMap(Type source, Type destination);

    bool TryGetMap<TSource, TDestination>(out MemberMap<TSource, TDestination> map);

    bool TryGetMap(Type source, Type destination, out MemberMap map);

    void RegisterMap(MemberMap map);

    void ClearMapCache();

    IMemberMapper AddCustomConstructor<T>(Expression<Func<T>> ctor);

    IMemberMapper AddCustomConstructor(Type type, LambdaExpression ctor);

    LambdaExpression GetConstructor(Type t);

    TSource DeepClone<TSource>(TSource source) where TSource : new();

    IMapRepository MapRepository { get; set; }
  }
}
