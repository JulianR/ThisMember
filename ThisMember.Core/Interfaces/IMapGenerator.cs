using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Interfaces
{
  public interface IMapGenerator
  {
    Delegate GenerateMappingFunction(ProposedMap map);
  }
}
