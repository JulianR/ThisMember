using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  public class ProposedTypeMapping : IMappingProposition
  {
    public PropertyOrFieldInfo SourceMember { get; set; }
    public PropertyOrFieldInfo DestinationMember { get; set; }

    public CustomMapping CustomMapping { get; set; }

    public bool Ignored { get; set; }

    public LambdaExpression Condition { get; set; }

    internal bool DoNotCache { get; set; }

    public ProposedTypeMapping()
    {
      ProposedMappings = new List<ProposedMemberMapping>();
      ProposedTypeMappings = new List<ProposedTypeMapping>();
      IncompatibleMappings = new List<PropertyOrFieldInfo>();
    }

    public bool IsEnumerable { get; set; }

    public IList<ProposedTypeMapping> ProposedTypeMappings { get; set; }

    public IList<ProposedMemberMapping> ProposedMappings { get; set; }

    public IList<PropertyOrFieldInfo> IncompatibleMappings { get; set; }

    public ProposedTypeMapping Clone()
    {
      return new ProposedTypeMapping
      {
        DestinationMember = this.DestinationMember,
        SourceMember = this.SourceMember,
        ProposedMappings = this.ProposedMappings,
        ProposedTypeMappings = this.ProposedTypeMappings,
        IncompatibleMappings = this.IncompatibleMappings
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
      return this.DestinationMember == mapping.DestinationMember && this.SourceMember == mapping.SourceMember;
    }

    public override int GetHashCode()
    {
      return this.DestinationMember.GetHashCode() ^ this.SourceMember.GetHashCode();
    }
  }
}
