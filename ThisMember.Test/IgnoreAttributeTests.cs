using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class IgnoreAttributeTests
  {

    public class SourceType
    {
      public int ID { get; set; }
    }

    public class DestinationType
    {
      [IgnoreMember]
      public int ID { get; set; }
    }

    public class DestinationTypeWithProfile
    {
      [IgnoreMember(Profile = "update")]
      public int ID { get; set; }
    }

    [TestMethod]
    public void IgnoreAttributeIsRespected()
    {
      var mapper = new MemberMapper();

      var source = new SourceType
      {
        ID = 10
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreNotEqual(10, result.ID);
    }

    [TestMethod]
    public void IgnoreAttributeIsIgnoredForMapperWithOtherProfile()
    {
      var mapper = new MemberMapper { Profile = "create" };

      var source = new SourceType
      {
        ID = 10
      };

      var result = mapper.Map<SourceType, DestinationTypeWithProfile>(source);

      Assert.AreEqual(10, result.ID);
    }

    [TestMethod]
    public void IgnoreAttributeIsRespectedForMapperWithSameProfile()
    {
      var mapper = new MemberMapper { Profile = "update" };

      var source = new SourceType
      {
        ID = 10
      };

      var result = mapper.Map<SourceType, DestinationTypeWithProfile>(source);

      Assert.AreNotEqual(10, result.ID);
    }
  }
}
