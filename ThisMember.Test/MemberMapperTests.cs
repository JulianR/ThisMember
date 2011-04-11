using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class MemberMapperTests
  {

    private class SourceType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    private class DestinationType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    [TestMethod]
    public void ExpectedMembersAreMapped()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap(typeof(SourceType), typeof(DestinationType)).FinalizeMap();

      var source = new SourceType
      {
        ID = 5,
        Name = "Source"
      };

      var destination = new DestinationType();

      destination = mapper.Map(source, destination);

      Assert.AreEqual(destination.ID, 5);
      Assert.AreEqual(destination.Name, "Source");

    }

    [TestMethod]
    public void NoExplicitMapCreationStillResultsInMap()
    {
      var mapper = new MemberMapper();

      var source = new SourceType
      {
        ID = 5,
        Name = "Source"
      };

      var destination = new DestinationType();

      destination = mapper.Map(source, destination);

      Assert.AreEqual(destination.ID, 5);
      Assert.AreEqual(destination.Name, "Source");

    }

    [TestMethod]
    public void NoDestinationCreatesDestinationInstance()
    {
      var mapper = new MemberMapper();

      var source = new SourceType
      {
        ID = 5,
        Name = "Source"
      };

      var destination = mapper.Map(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.AreEqual(destination.Name, "Source");

    }

    [TestMethod]
    public void NoDestinationCreatesDestinationInstanceWithExplicitTypeParams()
    {
      var mapper = new MemberMapper();

      var source = new SourceType
      {
        ID = 5,
        Name = "Source"
      };

      var destination = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.AreEqual(destination.Name, "Source");

    }

    class NestedSourceType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    class ComplexSourceType
    {
      public int ID { get; set; }
      public NestedSourceType Complex { get; set; }
    }

    class NestedDestinationType
    {
      public int ID { get; set; }
      public string Name { get; set; }
    }

    class ComplexDestinationType
    {
      public int ID { get; set; }
      public NestedDestinationType Complex { get; set; }
    }

    [TestMethod]
    public void ComplexTypeIsCorrectlyMapped()
    {
      var mapper = new MemberMapper();

      var source = new ComplexSourceType
      {
        ID = 5,
        Complex = new NestedSourceType
        {
          ID = 10,
          Name = "test"
        }
      };

      var destination = mapper.Map<ComplexSourceType, ComplexDestinationType>(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.IsNotNull(destination.Complex);
      Assert.AreEqual(destination.Complex.Name, "test");
      Assert.AreEqual(destination.Complex.ID, 10);

    }

    [TestMethod]
    public void ComplexTypeMappingHandlesNullValues()
    {
      var mapper = new MemberMapper();

      var source = new ComplexSourceType
      {
        ID = 5,
        Complex = null
      };

      var destination = mapper.Map<ComplexSourceType, ComplexDestinationType>(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.IsNull(destination.Complex);

    }

    [TestMethod]
    public void ComplexTypeMappingRespectsExistingMapping()
    {
      var mapper = new MemberMapper();

      var proposed = mapper.CreateMap(typeof(ComplexSourceType), typeof(ComplexDestinationType),
      (s, p, option) =>
      {
        if (s.Name == "Name")
        {
          option.IgnoreMember();
        }
      });

      proposed.FinalizeMap();

      var source = new ComplexSourceType
      {
        ID = 5,
        Complex = new NestedSourceType
        {
          ID = 10,
          Name = "test"
        }
      };

      var destination = mapper.Map<ComplexSourceType, ComplexDestinationType>(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.IsNotNull(destination.Complex);
      Assert.AreNotEqual(destination.Complex.Name, source.Complex.Name);

    }

    [TestMethod]
    public void ComplexTypeMappingRespectsExistingMappingForOtherTypes()
    {
      var mapper = new MemberMapper();

      var proposed = mapper.CreateMap(typeof(NestedSourceType), typeof(NestedDestinationType),
      (s, p, option) =>
      {
        if (s.Name == "Name")
        {
          option.IgnoreMember();
        }
      });

      proposed.FinalizeMap();

      var source = new ComplexSourceType
      {
        ID = 5,
        Complex = new NestedSourceType
        {
          ID = 10,
          Name = "test"
        }
      };

      var destination = mapper.Map<ComplexSourceType, ComplexDestinationType>(source);

      Assert.AreEqual(destination.ID, 5);
      Assert.IsNotNull(destination.Complex);
      Assert.AreNotEqual(destination.Complex.Name, source.Complex.Name);

    }


  }
}
