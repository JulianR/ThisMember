using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Interfaces;

namespace ThisMember.Test
{
  [TestClass]
  public class MapRepositoryTests
  {
    [TestMethod]
    public void RepositoryIsUsedWhenSupplied()
    {
      var mapper = new MemberMapper();

      var repo = new TestRepository();

      mapper.MapRepository = repo;

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = "1" });

      Assert.AreEqual(1, result.Test);

    }

    [TestMethod]
    public void RepositoryIsUsedForNestedTypeWhenSupplied()
    {
      var mapper = new MemberMapper();

      var repo = new TestRepository();

      mapper.MapRepository = repo;

      var result = mapper.Map<SourceTypeWithNested, DestinationTypeWithNested>(new SourceTypeWithNested { Foo = new SourceTypeNested { ID = "1" } });

      Assert.AreEqual(1, result.Foo.Test);

    }

    class SourceType
    {
      public string ID { get; set; }
    }

    class DestinationType
    {
      public int Test { get; set; }
    }

    class SourceTypeWithNested
    {
      public SourceTypeNested Foo { get; set; }
    }

    class DestinationTypeWithNested
    {
      public DestinationTypeNested Foo { get; set; }
    }

    class SourceTypeNested
    {
      public string ID { get; set; }
    }

    class DestinationTypeNested
    {
      public int Test { get; set; }
    }

    private class TestRepository : MapRepositoryBase
    {
      protected override void InitMaps()
      {
        DefineMap<SourceType, DestinationType>((mapper, options) =>
        {
          return mapper.CreateMapProposal<SourceType, DestinationType>(customMapping: src => new DestinationType
          {
            Test = int.Parse(src.ID)
          });
        });

        DefineMap<SourceTypeNested, DestinationTypeNested>((mapper, options) =>
        {
          return mapper.CreateMapProposal<SourceTypeNested, DestinationTypeNested>(customMapping: src => new DestinationType
          {
            Test = int.Parse(src.ID)
          });
        });
      }
    }



  }




}
