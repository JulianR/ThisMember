using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IgnoreMember : Attribute
  {
    public string Profile { get; set; }
  }
}
