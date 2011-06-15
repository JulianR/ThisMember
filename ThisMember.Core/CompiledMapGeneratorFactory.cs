using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class CompiledMapGeneratorFactory : IMapGeneratorFactory
  {
    public IMapGenerator GetGenerator(IMemberMapper mapper)
    {
      return new CompiledMapGenerator(mapper);
    }
  }
}
