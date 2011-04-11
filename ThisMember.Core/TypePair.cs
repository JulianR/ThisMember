using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core
{
  public class TypePair : IEquatable<TypePair>
  {
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    public TypePair(Type source, Type destination)
    {
      this.SourceType = source;
      this.DestinationType = destination;
    }

    public bool Equals(TypePair other)
    {
      return object.ReferenceEquals(this.SourceType.UnderlyingSystemType, other.SourceType.UnderlyingSystemType) && object.ReferenceEquals(this.DestinationType.UnderlyingSystemType, other.DestinationType.UnderlyingSystemType);
    }

    public override int GetHashCode()
    {
      return this.SourceType.GetHashCode();
    }
  }
}
