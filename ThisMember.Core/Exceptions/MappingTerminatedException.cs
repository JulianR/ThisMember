using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Exceptions
{
  public class MappingTerminatedException : Exception
  {
    public MappingTerminatedException(string msg) : base(msg) { }
  }
}
