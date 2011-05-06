using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;

namespace ThisMember.Core.Exceptions
{
  public class IncompatibleMappingException : Exception
  {
    public PropertyOrFieldInfo MissingMember { get; set; }

    public IncompatibleMappingException(PropertyOrFieldInfo member)
      : base(string.Format("Member {0}.{1} cannot be mapped", ((MemberInfo)member).DeclaringType.Name, member.Name))
    {
      this.MissingMember = member;
    }

  }
}
