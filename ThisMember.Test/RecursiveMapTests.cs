using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class RecursiveMapTests
  {

    public class SourceType
    {
      public int ID { get; set; }
      public IList<SourceType> Children { get; set; }
    }

    public class DestinationType
    {
      public int ID { get; set; }
      public IList<DestinationType> Children { get; set; }
    }

    [TestMethod]
    public void RecursiveRelationshipsAreMappedCorrectly()
    {
      var map = new MemberMapper();

      var source = new SourceType
      {
        ID = 10,
        Children = new List<SourceType>
        {
          new SourceType
          {
            ID = 11,
          },
          new SourceType
          {
            ID = 12,
            Children = new List<SourceType>
            {
              new SourceType 
              {
                ID = 13
              }
            }
          }

        }
      };

      var result = map.Map<SourceType, DestinationType>(source);

    }
  }
}
