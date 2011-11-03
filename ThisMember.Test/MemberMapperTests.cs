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
    public void ClearMapCacheIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>(customMapping: src => new
      {
        ID = src.ID * 10,
        Name = src.Name + src.Name
      });

      var source = new SourceType
      {
        ID = 10,
        Name = "x"
      };

      var result = mapper.Map<DestinationType>(source);

      Assert.AreEqual(100, result.ID);
      Assert.AreEqual("xx", result.Name);

      mapper.ClearMapCache();

      result = mapper.Map<DestinationType>(source);

      Assert.AreEqual(10, result.ID);
      Assert.AreEqual("x", result.Name);
    }

    [TestMethod]
    public void ExpectedMembersAreMapped()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal(typeof(SourceType), typeof(DestinationType)).FinalizeMap();

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

      var proposed = mapper.CreateMapProposal(typeof(ComplexSourceType), typeof(ComplexDestinationType),
      options: (s, p, option, depth) =>
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

      var proposed = mapper.CreateMapProposal(typeof(NestedSourceType), typeof(NestedDestinationType),
      options: (s, p, option, depth) =>
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
    public void TryGetMapGenericWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = 1, Name = "X" });

      MemberMap<SourceType, DestinationType> map;

      Assert.IsTrue(mapper.TryGetMap<SourceType, DestinationType>(out map));
    }


    [TestMethod]
    public void TryGetMapNonGenericWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = 1, Name = "X" });

      MemberMap map;

      Assert.IsTrue(mapper.TryGetMap(typeof(SourceType), typeof(DestinationType), out map));
    }

    [TestMethod]
    public void HasMapGenericWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = 1, Name = "X" });

      Assert.IsTrue(mapper.HasMap<SourceType, DestinationType>());
    }

    [TestMethod]
    public void HasMapNonGenericWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = 1, Name = "X" });

      Assert.IsTrue(mapper.HasMap(typeof(SourceType), typeof(DestinationType)));
    }

    [TestMethod]
    public void HasMapWithoutCreateMapReturnsFalse()
    {
      var mapper = new MemberMapper();

      var result = mapper.CreateMapProposal<SourceType, DestinationType>();

      Assert.IsFalse(mapper.HasMap(typeof(SourceType), typeof(DestinationType)));
    }

    private class CustomConventionSource
    {
      public int Type { get; set; }
      public int TypeID { get; set; }
    }

    private class CustomConventionDestination
    {
      public int Foo { get; set; }
      public int TypeID { get; set; }
    }

    [TestMethod]
    public void MappingOptionsAllowCustomConventions()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<CustomConventionSource, CustomConventionDestination>(
        options: (source, dest, options, depth) =>
        {
          if (source != null && source.Name.EndsWith("ID"))
          {
            var sourceType = source.DeclaringType;
            var destType = dest.DeclaringType;

            options.MapProperty(sourceType.GetProperty("Type"), destType.GetProperty("Foo"));
          }
        });

      var result = mapper.Map(new CustomConventionSource { TypeID = 1, Type = 10 }, new CustomConventionDestination());

      Assert.AreEqual(10, result.Foo);
      Assert.AreNotEqual(1, result.TypeID);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CustomConventionsCannotUseMembersOnDifferentType()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<CustomConventionSource, CustomConventionDestination>(
        options: (source, dest, options, depth) =>
        {
          if (source != null)
          {
            options.MapProperty(typeof(string).GetProperties().First(), dest);
          }
        });
    }
  }
}
