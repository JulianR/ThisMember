using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{
  public interface IMapGenerator
  {
    Delegate GenerateMappingFunction(ProposedMap map);
    DebugInformation DebugInformation { get; }
  }
}
