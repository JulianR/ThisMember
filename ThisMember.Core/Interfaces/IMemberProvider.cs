using System;
using System.Collections.Generic;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core.Interfaces
{
  public interface IMemberProvider
  {
    IEnumerable<PropertyOrFieldInfo> GetDestinationMembers();
    PropertyOrFieldInfo GetMatchingSourceMember(PropertyOrFieldInfo destinationProperty);
    bool IsMemberIgnored(Type sourceType, PropertyOrFieldInfo destinationProperty);
    ProposedHierarchicalMapping ProposeHierarchicalMapping(PropertyOrFieldInfo destinationMember);
  }
}
