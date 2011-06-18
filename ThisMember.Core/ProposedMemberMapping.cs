using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  /// <summary>
  /// Describes the proposed mapping from a SourceMember to a DestinationMember
  /// </summary>
  public class ProposedMemberMapping : IMappingProposition
  {
    public PropertyOrFieldInfo SourceMember { get; set; }
    public PropertyOrFieldInfo DestinationMember { get; set; }

    public bool Ignored { get; set; }

    /// <summary>
    /// The condition that needs to be met for the member to be mapped
    /// </summary>
    public LambdaExpression Condition { get; set; }

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
