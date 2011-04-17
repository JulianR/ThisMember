using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;

namespace ThisMember.Core
{
  public class ProposedTypeMapping
  {
    public PropertyOrFieldInfo SourceMember { get; set; }
    public PropertyOrFieldInfo DestinationMember { get; set; }

    public CustomMapping CustomMapping { get; set; }

    public ProposedTypeMapping()
    {
      ProposedMappings = new List<ProposedMemberMapping>();
      ProposedTypeMappings = new List<ProposedTypeMapping>();
    }

    public bool IsEnumerable { get; set; }

    public IList<ProposedTypeMapping> ProposedTypeMappings { get; set; }

    public IList<ProposedMemberMapping> ProposedMappings { get; set; }

    public ProposedTypeMapping Clone()
    {
      return new ProposedTypeMapping
      {
        DestinationMember = this.DestinationMember,
        SourceMember = this.SourceMember,
        ProposedMappings = this.ProposedMappings,
        ProposedTypeMappings = this.ProposedTypeMappings
      };
    }

    public override bool Equals(object obj)
    {
      var other = obj as ProposedTypeMapping;

      if (other == null) return false;

      return Equals((ProposedTypeMapping)obj);
    }

    public bool Equals(ProposedTypeMapping mapping)
    {
      return this.DestinationMember == mapping.DestinationMember && this.SourceMember== mapping.SourceMember;
    }

    public override int GetHashCode()
    {
      return this.DestinationMember.GetHashCode() ^ this.SourceMember.GetHashCode();
    }
  }
}
