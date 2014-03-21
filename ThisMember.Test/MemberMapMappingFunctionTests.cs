using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class MemberMapMappingFunctionTests
  {
    class Source
    {
    }

    class Destination
    {
    }

    [TestMethod]
    public void MappingFunctionIsSetCorrectly()
    {
      var mapper = new MemberMapper();

      var map = mapper.CreateMap(typeof(Source), typeof(Destination));

      Assert.IsNotNull(map.MappingFunction);
      Assert.IsNotNull(map as MemberMap<Source, Destination>);
      Assert.IsNotNull(((MemberMap<Source, Destination>)map).MappingFunction);
    }
  }
}
