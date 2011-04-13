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
        this.ID = id;
      }

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

    }
  }
}
