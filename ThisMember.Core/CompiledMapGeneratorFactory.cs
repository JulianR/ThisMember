using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using ThisMember.Core.Options;

namespace ThisMember.Core
{
  internal class CompiledMapGeneratorFactory : IMapGeneratorFactory
  {
    public IMapGenerator GetGenerator(IMemberMapper mapper, ProposedMap proposedMap, MapperOptions options)
    {
      return new CompiledMapGenerator(mapper, proposedMap, options);
    }
  }
}
