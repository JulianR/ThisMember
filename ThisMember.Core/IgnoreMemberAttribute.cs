using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core
{
  /// <summary>
  /// When placed above a property or a field, the member is ignored as a destination member.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IgnoreMemberAttribute : Attribute
  {
    public string Profile { get; set; }
  }
}
