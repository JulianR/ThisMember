using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class DebugInformationTests
  {

    class SourceType
    {
      public int Foo { get; set; }
    }

    class DestType
    {
      public int Foo;
    }

    [TestMethod]
    public void DebugInformationIsIncluded()
    {
      var mapper = new MemberMapper();
      mapper.Options.Debug.DebugInformationEnabled = true;

      mapper.CreateMap<SourceType, DestType>();

      var map = mapper.GetMap<SourceType, DestType>();

      Assert.IsNotNull(map.DebugInformation);
      Assert.IsNotNull(map.DebugInformation.MappingExpression);

    }
  }
}
