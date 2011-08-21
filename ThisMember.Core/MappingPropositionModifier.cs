using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class MappingPropositionModifier<TSource, TDestination>
  {
    private readonly ProposedMap<TSource, TDestination> map;
    private readonly IMappingProposition mapping;

    public MappingPropositionModifier(ProposedMap<TSource, TDestination> map, IMappingProposition mapping)
    {
      this.map = map;
      this.mapping = mapping;
    }

    public ProposedMap<TSource, TDestination> Ignore()
    {
      mapping.Ignored = true;
      return map;
    }

    public ProposedMap<TSource, TDestination> OnlyIf(Expression<Func<TSource, bool>> condition)
    {
      mapping.Condition = condition;
      return map;
    }

    public ProposedMap<TSource, TDestination> MapAs<TSourceReturn>(Expression<Func<TSource, TSourceReturn>> customMapping)
    {
      var body = customMapping.Body;

      if (map.ProposedTypeMapping.CustomMapping == null)
      {
        //int index = 0;
        map.ProposedTypeMapping.CustomMapping = new CustomMapping
        {
          SourceParameter = customMapping.Parameters.Single()
          //ArgumentParameters = (from p in customMapping.Parameters
          //              select new IndexedParameterExpression
          //              {
          //                Parameter = p,
          //                Index = index++
          //              }).ToList()
        };
      }
      else
      {
        body = new CustomMapping.ParameterVisitor((from p in customMapping.Parameters
                                                   select p).ToList(), 
                                                   map.ProposedTypeMapping.CustomMapping.ArgumentParameters.Select(a => a.Parameter).ToList()).Visit(body);
      }

      map.ProposedTypeMapping.CustomMapping.Members.Add(new MemberExpressionTuple { Member = mapping.DestinationMember, Expression = body });

      if (!map.ProposedTypeMapping.ProposedMappings.Select(p => p.DestinationMember).Contains(mapping.DestinationMember))
      {
        map.ProposedTypeMapping.ProposedMappings.Add
        (
          new ProposedMemberMapping
          {
            SourceMember = null,
            DestinationMember = mapping.DestinationMember
          }
        );
      }

      //map.ProposedTypeMapping.CustomMapping.Members

      return map;
      //mapping.
    }

  }
}
