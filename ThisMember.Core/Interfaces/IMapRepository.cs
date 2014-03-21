using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Interfaces
{
  public interface IMapRepository
  {
    bool TryGetMap(IMemberMapper mapper, MemberOptions options, TypePair pair, out ProposedMap map);
    bool TryGetMap<TSource, TDestination>(IMemberMapper mapper, MemberOptions options, out ProposedMap<TSource, TDestination> map);
  }
}
