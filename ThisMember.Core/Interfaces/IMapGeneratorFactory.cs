using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Options;

namespace ThisMember.Core.Interfaces
{
  public interface IMapGeneratorFactory
  {
    IMapGenerator GetGenerator(IMemberMapper mapper, ProposedMap proposedMap, MapperOptions options);
  }
}
