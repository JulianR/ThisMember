using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  internal class CompiledMapGeneratorFactory : IMapGeneratorFactory
  {
    public IMapGenerator GetGenerator(IMemberMapper mapper, ProposedMap proposedMap)
    {
      return new CompiledMapGenerator(mapper, proposedMap);
    }
  }
}
