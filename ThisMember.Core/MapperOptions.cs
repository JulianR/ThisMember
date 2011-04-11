using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core
{
  public class MapperOptions
  {
    public Action BeforeMapping { get; set; }
    public Action AfterMapping { get; set; }

    public static readonly MapperOptions Default = new MapperOptions();

  }
}
