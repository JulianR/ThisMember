using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ComplexMappingTests
  {


    public class Foo
    {
      public string Z { get; set; }
    }

    public class Bar
    {
      public string Z { get; set; }
    }

    public class SourceElement
    {
      public int X { get; set; }

      public List<Foo> Collection { get; set; }
    }

    public class DestinationElement
    {
      public int X { get; set; }

      public List<Bar> Collection { get; set; }
    }

    public class SourceType
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public IList<SourceElement> IDs { get; set; }
    }

    public class DestinationType
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public IEnumerable<DestinationElement> IDs { get; set; }
    }

    //[TestMethod]
    public void Test()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.PerformNullChecksOnCustomMappings = false;

      int i = 2;

      var map = mapper.CreateMapProposal<SourceType, DestinationType>(customMapping: (src) => new
      {
        //ID = src.IDs.Count + 100 + i,
        ID = (from x in Enumerable.Range(0, 100)
              select x).Sum() + i,
        Name = src.Name.Length.ToString() + " " + src.Name
      }).FinalizeMap();

      i++;

      //var map = mapper.CreateMap(typeof(SourceType), typeof(DestinationType)).FinalizeMap();

      var source = new SourceType
      {
        ID = 1,
        IDs = new List<SourceElement>
        {
          new SourceElement
          {
            X = 10,
            Collection = new List<Foo>
            {
              new Foo
              {
                Z = "string"
              },
              new Foo
              {
                Z = "string1"
              },
              new Foo
              {
                Z = "string2"
              }
            }
          }
        },
        Name = "X"
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

    }
  }
}
