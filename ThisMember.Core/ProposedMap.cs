using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace ThisMember.Core
{
  public class ProposedMap
  {
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    //public CustomMapping CustomMapping { get; set; }

    public IMapGenerator MapGenerator { get; set; }

    private readonly IMemberMapper mapper;

    public ProposedMap(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    protected Dictionary<Type, LambdaExpression> constructorCache = new Dictionary<Type, LambdaExpression>();

    public MemberMap FinalizeMap()
    {
      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = this.MapGenerator.GenerateMappingFunction(this);

      mapper.RegisterMap(map);

      return map;
    }

    public ProposedMap WithConstructorFor<T>(LambdaExpression constructor)
    {
      constructorCache[typeof(T)] = constructor;
      return this;
    }

    public ProposedMap WithConstructorFor(Type type, LambdaExpression constructor)
    {
      constructorCache[type] = constructor;
      return this;
    }

    public LambdaExpression GetConstructor(Type type)
    {
      LambdaExpression e;
      constructorCache.TryGetValue(type, out e);
      return e;
    }

    public ProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap
  {

    public ProposedMap(IMemberMapper mapper)
      : base(mapper)
    {
    }

    public ProposedMap<TSource, TDestination> AddMapping<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      return this;
    }

    public ProposedMap<TSource, TDestination> WithConstructorFor<T>(Expression<Func<TSource, TDestination, T>> constructor)
    {
      constructorCache.Add(typeof(T), constructor);
      return this;
    }
  }
}
