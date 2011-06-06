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
    /// <summary>
    /// The member is only ignored if the profile of the mapper matches this property.
    /// </summary>
    public string Profile { get; set; }

    /// <summary>
    /// The member is only ignored if the source type in this mapping is of the specified type.
    /// </summary>
    public Type WhenSourceTypeIs { get; set; }
  }
}
