using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{

  public delegate void MappingOptions(PropertyOrFieldInfo source, PropertyOrFieldInfo destination, MappingOption option);

  public interface IMappingStrategy
  {
    ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null);

    ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    IMapGenerator MapGenerator { get; set; }

    void ClearMapCache();
  }
}
