using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Interfaces
{
  public interface IProjectionGeneratorFactory
  {
    IProjectionGenerator GetGenerator(IMemberMapper mapper);
  }
}
