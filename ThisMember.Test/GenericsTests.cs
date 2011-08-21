using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class GenericsTests
  {
    class SourceType
    {

    }

    class DestinationType
    {

    }

    [TestMethod]
    public void HasMapWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      Assert.IsTrue(mapper.HasMap<SourceType, DestinationType>());

    }

    [TestMethod]
    public void HasMapWorksWithParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType, int>((s, i) => new DestinationType
      {

      });

      Assert.IsTrue(mapper.HasMap<SourceType, DestinationType>());
    }

    [TestMethod]
    public void GetMapWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      Assert.IsNotNull(mapper.GetMap<SourceType, DestinationType>());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetMapThrowsForMapWithParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType, int>((s, i) => new DestinationType
      {

      });

      mapper.GetMap<SourceType, DestinationType>();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetMapForMapWithParameterThrowsForMapWithoutParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      mapper.GetMap<SourceType, DestinationType, int>();
    }

    [TestMethod]
    public void TryGetMapThrowsForMapWithParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType, int>((s, i) => new DestinationType
      {

      });
      MemberMap<SourceType, DestinationType> map;

      Assert.IsFalse(mapper.TryGetMap<SourceType, DestinationType>(out map));
    }

    [TestMethod]
    public void TryGetMapForMapWithParameterThrowsForMapWithoutParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      MemberMap<SourceType, DestinationType, int> map;

      Assert.IsFalse(mapper.TryGetMap<SourceType, DestinationType, int>(out map));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void MapThrowsForMapWithParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType, int>((s, i) => new DestinationType
      {

      });

      mapper.Map<SourceType, DestinationType>(new SourceType());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void MapForMapWithParameterThrowsForMapWithoutParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>();

      mapper.Map<SourceType, DestinationType, int>(new SourceType(), 0);
    }


    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NonGenericMapForMapWithParameterThrowsForMapWithoutParameter()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap(typeof(SourceType), typeof(DestinationType));

      mapper.Map<SourceType, DestinationType, int>(new SourceType(), 0);
    }
  }
}
