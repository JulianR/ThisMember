using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Options;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core.Fluent
{
  public class DestinationTypeModifier<TDestination>
  {
    private readonly IMemberMapper mapper;

    public DestinationTypeModifier(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    public void UseMapperOptions(MapperOptions options)
    {
      mapper.Data.AddMapperOptions(typeof(TDestination), options, false);
    }
  }
}
