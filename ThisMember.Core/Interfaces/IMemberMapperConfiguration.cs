using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Options;

namespace ThisMember.Core.Interfaces
{
  public interface IMemberMapperConfiguration
  {
    MapperOptions GetOptions(IMemberMapper mapper);
    IMappingStrategy GetMappingStrategy(IMemberMapper mapper);
    IMapGeneratorFactory GetMapGenerator(IMemberMapper mapper);
    IProjectionGeneratorFactory GetProjectionGenerator(IMemberMapper mapper);
  }
}
