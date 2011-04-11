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

    public CustomMapping CustomMapping { get; set; }

    public IMapGenerator MapGenerator { get; set; }

    private readonly IMemberMapper mapper;

    public ProposedMap(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    public MemberMap FinalizeMap()
    {
      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = this.MapGenerator.GenerateMappingFunction(this);

      mapper.RegisterMap(map);

      return map;
    }

    public ProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap
  {

    public ProposedMap(IMemberMapper mapper)
      : base(mapper)
    {
    }

    public ProposedMap<TSource, TDestination> AddExpression<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      throw new NotImplementedException();
    }
  }
}
