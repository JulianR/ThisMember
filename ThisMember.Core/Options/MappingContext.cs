using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core.Options
{
  public class MappingContext
  {
    public MappingContext(PropertyOrFieldInfo source, PropertyOrFieldInfo dest, int depth, IMemberMapper mapper)
    {
      Source = source;
      Destination = dest;
      Depth = depth;
      Mapper = mapper;
    }

    public PropertyOrFieldInfo Source { get; private set; }
    public PropertyOrFieldInfo Destination { get; private set; }
    public int Depth { get; private set; }
    public IMemberMapper Mapper { get; set; }
  }
}
