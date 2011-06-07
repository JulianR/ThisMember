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
    public IncompatibleMappingException(string message)
      : base(message)
    {
    }

  }
}
