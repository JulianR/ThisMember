using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core.Exceptions
{
  public class IncompatibleMappingException : Exception
  {
    public PropertyOrFieldInfo MissingMember { get; set; }

    public IncompatibleMappingException(PropertyOrFieldInfo member)
      : base(string.Format("Member {0} cannot be mapped", member))
    {
      this.MissingMember = member;
    }

  }
}
