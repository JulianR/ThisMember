using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ThisMember.Core.Exceptions
{
  public class MemberNotFoundException : Exception
  {
    public MemberNotFoundException(MemberInfo member)
      : base(string.Format("Member not found in the proposed map", member))
    {
    }
  }
}
