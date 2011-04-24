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
  }
}
