using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Exceptions
{
  public class RecursiveRelationshipException : Exception
  {
    public RecursiveRelationshipException(TypePair pair) : base(string.Format("Stumbled on a recursive relationship when trying to map {0}", pair))
    { 
    }
  }
}
