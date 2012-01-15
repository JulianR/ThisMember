using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class ProposedHierarchicalMapping
  {
    private readonly IList<PropertyOrFieldInfo> hierarchy;

    public ProposedHierarchicalMapping(IList<PropertyOrFieldInfo> hierarchy)
    {
      this.hierarchy = hierarchy;
    }

    public Type ReturnType
    {
      get
      {
        return hierarchy.Last().PropertyOrFieldType;
      }
    }

    public IEnumerable<PropertyOrFieldInfo> Members
    {
      get
      {
        foreach (var member in hierarchy)
        {
          yield return member;
        }
      }
    }

  }
}
