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
      [IgnoreMemberAttribute]
      public int ID { get; set; }
    }

    public class DestinationTypeWithProfile
    {
      [IgnoreMemberAttribute(Profile = "update")]
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

    public class OtherSourceType
    {
      public int ID { get; set; }
    }

    public class OtherDestinationType
    {
      [IgnoreMemberAttribute(WhenSourceTypeIs = typeof(OtherSourceType))]
      public int ID { get; set; }
    }

    [TestMethod]
    public void IgnoreAttributeRespectsSourceTypeFilter()
    {
      var mapper = new MemberMapper();

      var source = new OtherSourceType
      {
        ID = 10
      };

      var result = mapper.Map<OtherSourceType, OtherDestinationType>(source);

      Assert.AreNotEqual(10, result.ID);

      var source1 = new SourceType
      {
        ID = 10
      };

      var result1 = mapper.Map<SourceType, OtherDestinationType>(source1);

      Assert.AreEqual(10, result1.ID);


    }
  }
}
