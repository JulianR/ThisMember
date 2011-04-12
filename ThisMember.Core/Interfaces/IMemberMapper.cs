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

    MapperOptions Options { get; set; }

    TDestination Map<TDestination>(object source) where TDestination : new();

    TSource Map<TSource>(TSource source) where TSource : new();

    TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    ProposedMap CreateMap(Type source, Type destination, MappingOptions options = null);

    ProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    MemberMap CreateAndFinalizeMap(Type source, Type destination, MappingOptions options = null);

    MemberMap<TSource, TDestination> CreateAndFinalizeMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    bool HasMap<TSource, TDestination>();

    bool HasMap(Type source, Type destination);

    MemberMap<TSource, TDestination> GetMap<TSource, TDestination>();

    MemberMap GetMap(Type source, Type destination);

    bool TryGetMap<TSource, TDestination>(out MemberMap<TSource, TDestination> map);

    bool TryGetMap(Type source, Type destination, out MemberMap map);

    void RegisterMap(MemberMap map);

  }
}
