using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class CustomMappingAutoConversionTests
  {

    public class SourceNested
    {
      public string Foobar { get; set; }
    }

    class SourceType
    {
      public SourceNested Foo { get; set; }
    }

    class DestinationNested
    {
      public string Foobar { get; set; }
    }

    class DestinationType
    {
      public DestinationNested Bar { get; set; }
    }

    //[TestMethod] 
    public void AutoConvertWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>(src => new
      {
        Bar = src.Foo
      });

    }
  }
}
