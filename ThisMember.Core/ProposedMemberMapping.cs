using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;

namespace ThisMember.Core
{
  public class ProposedMemberMapping
  {
    public PropertyOrFieldInfo SourceMember { get; set; }
    public PropertyOrFieldInfo DestinationMember { get; set; }

    public override bool Equals(object obj)
    {
      var other = obj as ProposedMemberMapping;

      if (other == null) return false;

      return Equals((ProposedMemberMapping)obj);
    }

    public bool Equals(ProposedMemberMapping mapping)
    {
      return this.DestinationMember == mapping.DestinationMember && this.SourceMember == mapping.SourceMember;
    }

    public override int GetHashCode()
    {
      return this.DestinationMember.GetHashCode() ^ this.SourceMember.GetHashCode();
    }
  }
}
