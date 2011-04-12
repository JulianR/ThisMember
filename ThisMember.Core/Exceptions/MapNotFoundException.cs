using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Exceptions
{
  public class MapNotFoundException : Exception
  {

    public Type SourceType { get; set; }

    public Type DestinationType { get; set; }

    public MapNotFoundException(Type source, Type destination)
      : base(string.Format("No mapping between {0} and {1} exists", source, destination))
    {
      this.SourceType = source;
      this.DestinationType = destination;
    }
  }
}
