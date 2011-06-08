using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ProposedMapMutationTests
  {

    public class NestedSourceType
    {
      public string Foo { get; set; }
    }

    public class NestedDestinationType
    {
      public string Foo { get; set; }
    }

    public class SourceType
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public NestedSourceType Nested { get; set; }
    }

    public class DestinationType
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public NestedDestinationType Nested { get; set; }
    }

    public class SourceTypeCollection
    {
      public IEnumerable<NestedSourceType> Nested { get; set; }
    }

    public class DestinationTypeCollection
    {
      public List<NestedDestinationType> Nested { get; set; }
    }

    [TestMethod]
    public void PropertiesCanBeIgnored()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(src => src.Name).Ignore()
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Name);
      Assert.AreEqual("Bla", result.Nested.Foo);

    }

    [TestMethod]
    public void NestedPropertyMembersCanBeIgnored()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(src => src.Nested.Foo).Ignore()
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Nested.Foo);

    }

    [TestMethod]
    public void NestedPropertiesCanBeIgnored()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(src => src.Nested).Ignore()
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Nested);

    }

    [TestMethod]
    public void MultiplePropertiesCanBeIgnored()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(dest => dest.Nested.Foo).Ignore()
        .ForMember(dest => dest.Name).Ignore()
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Nested.Foo);
      Assert.IsNull(result.Name);

    }

    [TestMethod]
    public void MappingConditionIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(dest => dest.Name).OnlyIf(src => src.ID == 0)
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Name);

      source.ID = 0;

      result = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreEqual("X", result.Name);

    }

    [TestMethod]
    public void MappingConditionIsRespectedForNestedMembers()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .ForMember(dest => dest.Nested).OnlyIf(src => src.ID == 0)
        .FinalizeMap();

      var source = new SourceType
      {
        ID = 10,
        Name = "X",
        Nested = new NestedSourceType
        {
          Foo = "Bla"
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result.Nested);

      source.ID = 0;

      result = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreEqual("Bla", result.Nested.Foo);

    }

    //[TestMethod]
    public void MappingConditionIsRespectedForNestedCollectionMembers()
    {
      var mapper = new MemberMapper();

      int i = 10;

      mapper.CreateMapProposal<SourceTypeCollection, DestinationTypeCollection>()
        .ForMember(dest => dest.Nested).OnlyIf(src => i == 0)
        .FinalizeMap();

      var source = new SourceTypeCollection
      {
        Nested = new List<NestedSourceType> 
        { 
          new NestedSourceType
          {
            Foo = "Bla"
          }
        }
      };

      var result = mapper.Map<SourceTypeCollection, DestinationTypeCollection>(source);

      Assert.IsNull(result.Nested);

      //i = 0;

      result = mapper.Map<SourceTypeCollection, DestinationTypeCollection>(source);

      Assert.AreEqual("Bla", result.Nested.Single().Foo);

    }

    [TestMethod]
    public void NestedCollectionPropertiesCanBeIgnored()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceTypeCollection, DestinationTypeCollection>()
        .ForMember(dest => dest.Nested).Ignore()
        .FinalizeMap();

      var source = new SourceTypeCollection
      {
        Nested = new List<NestedSourceType> 
        { 
          new NestedSourceType
          {
            Foo = "Bla"
          }
        }
      };

      var result = mapper.Map<SourceTypeCollection, DestinationTypeCollection>(source);

      Assert.IsNull(result.Nested);

    }
  }
}
