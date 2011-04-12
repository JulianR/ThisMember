using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Exceptions
{
  public class CodeGenerationException : Exception
  {
    public CodeGenerationException(string message, params object[] args) : base(string.Format(message, args)) { }
  }
}
