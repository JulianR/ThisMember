using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class MappingProfilesTests
  {

    public class SourceType
    {
      public int ID { get; set; }
    }

    public class DestinationType
    {
      public int ID { get; set; }
    }

    [TestMethod]
    public void CallingNonExistentProfileCreatesIt()
    {
      var profiles = new MapCollection();

      var result = profiles["create"].Map<DestinationType>(new SourceType { ID = 10 });

      Assert.AreEqual(10, result.ID);
    }

    [TestMethod]
    public void SettingProfileSetsIt()
    {
      var profiles = new MapCollection();

      var mapper = new MemberMapper();

      profiles["create"] = mapper;

      Assert.AreEqual(mapper, profiles["create"]);
    }
  }
}
