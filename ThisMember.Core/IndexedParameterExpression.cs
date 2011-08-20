using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  public class IndexedParameterExpression
  {
    public int Index { get; set; }
    public ParameterExpression Parameter { get; set; }
    public string Name { get; set; }
  }
}
