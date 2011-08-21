using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Exceptions;

namespace ThisMember.Test
{
  [TestClass]
  public class ParameterTests
  {

    class SourceType
    {
      public int ID { get; set; }
    }

    class DestinationType
    {
      public int ID { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InvokingMapWithoutParameterAsMapWithParameterThrows()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      var result = mapper.Map(new SourceType(), new DestinationType(), 1);
    }

    [TestMethod]
    public void InvokingMapWithParameterWorks()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new SourceType(), new DestinationType(), 1);
    }

    [TestMethod]
    public void ParameterIsUsed()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType, int>((src, i) => new DestinationType
      {
        ID = i
      }).FinalizeMap();

      var result = mapper.Map(new SourceType(), new DestinationType(), 10);

      Assert.AreEqual(10, result.ID);

      result = mapper.Map(new SourceType(), new DestinationType(), 15);

      Assert.AreEqual(15, result.ID);
    }

    [TestMethod]
    [ExpectedException(typeof(MapNotFoundException))]
    public void GetMapWithoutSupplyingParameterTypeThrowsMapNotFoundException()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType, int>((src, i) => new DestinationType
      {
        ID = i
      }).FinalizeMap();

      mapper.GetMap<SourceType, DestinationType>();
    }

    [TestMethod]
    public void TryGetMapWithoutSupplyingParameterTypeReturnsFalse()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType, int>((src, i) => new DestinationType
      {
        ID = i
      }).FinalizeMap();

      MemberMap<SourceType, DestinationType> map;

      Assert.IsFalse(mapper.TryGetMap<SourceType, DestinationType>(out map));
    }
  }
}
