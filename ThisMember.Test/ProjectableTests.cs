using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ProjectableTests
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
    public void AsProjectableToListWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        },
        new SourceType
        {
          ID = 20
        }
      };

      var result = source.AsQueryable().AsProjectable().ToList(mapper.Project<SourceType, DestinationType>());

      Assert.IsTrue(source.Select(s => s.ID).SequenceEqual(result.Select(s => s.ID)));
    }

    [TestMethod]
    public void AsProjectableSingleWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        }
      };

      var result = source.AsQueryable().AsProjectable().Single(mapper.Project<SourceType, DestinationType>());

      Assert.AreEqual(10, result.ID);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AsProjectableSingleThrowsWhenMoreThanOne()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        },
        new SourceType
        {
          ID = 20
        }
      };

      var result = source.AsQueryable().AsProjectable().Single(mapper.Project<SourceType, DestinationType>());

      Assert.AreEqual(1, result.ID);
    }

    [TestMethod]
    public void AsProjectableFirstWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        },
         new SourceType
        {
          ID = 20
        }
      };

      var result = source.AsQueryable().AsProjectable().First(mapper.Project<SourceType, DestinationType>());

      Assert.AreEqual(10, result.ID);
    }

    [TestMethod]
    public void AsProjectableFirstOrDefaultWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        },
         new SourceType
        {
          ID = 20
        }
      };

      var result = source.AsQueryable().AsProjectable().FirstOrDefault(mapper.Project<SourceType, DestinationType>());

      Assert.AreEqual(10, result.ID);
    }

    [TestMethod]
    public void AsProjectableSingleOrDefaultWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        }
      };

      var result = source.AsQueryable().AsProjectable().SingleOrDefault(mapper.Project<SourceType, DestinationType>());

      Assert.AreEqual(10, result.ID);
    }


    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AsProjectableSingleThrowsWhenLessThanOne()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
      };

      var result = source.AsQueryable().AsProjectable().Single(mapper.Project<SourceType, DestinationType>());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AsProjectableFirstThrowsWhenLessThanOne()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
      };

      var result = source.AsQueryable().AsProjectable().First(mapper.Project<SourceType, DestinationType>());
    }

    [TestMethod]
    public void AsProjectableSingleOrDefaultReturnsDefaultWhenLessThanOne()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
      };

      var result = source.AsQueryable().AsProjectable().SingleOrDefault(mapper.Project<SourceType, DestinationType>());

      Assert.IsNull(result);
    }

    [TestMethod]
    public void AsProjectableFirstOrDefaultReturnsDefaultWhenLessThanOne()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
      };

      var result = source.AsQueryable().AsProjectable().FirstOrDefault(mapper.Project<SourceType, DestinationType>());

      Assert.IsNull(result);
    }

    [TestMethod]
    public void PagingOnProjectableWorks()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceType>
      {
        new SourceType
        {
          ID = 10
        },
        new SourceType
        {
          ID = 20
        },
        new SourceType
        {
          ID = 30
        }
      };

      var result = source.AsQueryable().AsCollectionProjectable().Page(mapper.Project<SourceType, DestinationType>(), 0, 2);

      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(10, result.First().ID);
      Assert.AreEqual(20, result.Skip(1).First().ID);

    }

  }
}
