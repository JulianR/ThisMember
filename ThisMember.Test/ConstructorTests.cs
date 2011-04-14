using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ConstructorTests
  {
    public class NestedSourceType
    {
      public int ID { get; set; }
    }

    public class NestedDestinationType
    {
      public NestedDestinationType(int id)
      {
        this.OtherID = id;
      }

      public int OtherID { get; set; }

      public int ID { get; set; }
    }

    public class SourceType
    {
      public NestedSourceType Foo { get; set; }
    }

    public class DestinationType
    {
      public NestedDestinationType Foo { get; set; }
    }

    [TestMethod]
    public void CustomConstructorIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
        .WithConstructorFor<NestedDestinationType>((src, dest) => new NestedDestinationType(1))
        .FinalizeMap();

      var source = new SourceType
      {
        Foo = new NestedSourceType
        {
          ID = 10
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);
      Assert.AreEqual(1, result.Foo.OtherID);
      Assert.AreEqual(10, result.Foo.ID);
    }

    [TestMethod]
    public void CustomConstructorOnMapperIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.AddCustomConstructor<NestedDestinationType>(() => new NestedDestinationType(1));

      var source = new SourceType
      {
        Foo = new NestedSourceType
        {
          ID = 10
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);
      Assert.AreEqual(1, result.Foo.OtherID);
      Assert.AreEqual(10, result.Foo.ID);
    }

    [TestMethod]
    public void CustomConstructorOnProposalTakesPrecedence()
    {
      var mapper = new MemberMapper();

      mapper.AddCustomConstructor<NestedDestinationType>(() => new NestedDestinationType(1));

      mapper.CreateMapProposal<SourceType, DestinationType>()
      .WithConstructorFor<NestedDestinationType>((src, dest) => new NestedDestinationType(2))
      .FinalizeMap();

      var source = new SourceType
      {
        Foo = new NestedSourceType
        {
          ID = 10
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);
      Assert.AreEqual(2, result.Foo.OtherID);
      Assert.AreEqual(10, result.Foo.ID);
    }

    [TestMethod]
    public void ParamsCanBeUsedWithCustomConstructor()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
      .WithConstructorFor<NestedDestinationType>((src, dest) => new NestedDestinationType(src.Foo.ID * 2))
      .FinalizeMap();

      var source = new SourceType
      {
        Foo = new NestedSourceType
        {
          ID = 10
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);
      Assert.AreEqual(source.Foo.ID * 2, result.Foo.OtherID);
      Assert.AreEqual(10, result.Foo.ID);
    }

    public static NestedDestinationType Inject()
    {
      return new NestedDestinationType(8);
    }

    [TestMethod]
    public void CustomConstructorsAllowDependencyInjection()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<SourceType, DestinationType>()
      .WithConstructorFor<NestedDestinationType>((src, dest) => Inject())
      .FinalizeMap();

      var source = new SourceType
      {
        Foo = new NestedSourceType
        {
          ID = 10
        }
      };

      var result = mapper.Map<SourceType, DestinationType>(source);
      Assert.AreEqual(8, result.Foo.OtherID);
      Assert.AreEqual(10, result.Foo.ID);
    }
  }
}
