using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{
  public interface IMappingProposition
  {
    bool Ignored { get; set; }
    LambdaExpression Condition { get; set; }
    PropertyOrFieldInfo DestinationMember { get; set; }
    PropertyOrFieldInfo SourceMember { get; set; }
  }
}
