using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  internal class DefaultProjectionGeneratorFactory : IProjectionGeneratorFactory
  {
    public IProjectionGenerator GetGenerator(IMemberMapper mapper)
    {
      return new DefaultProjectionGenerator(mapper);
    }

  }
}
