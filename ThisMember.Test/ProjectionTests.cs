using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ProjectionTests
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
    public void SimpleProjectionWorks()
    {
      var mapper = new MemberMapper();

      var result = mapper.Project<SourceType, DestinationType>().Compile()(new SourceType { ID = 1 });

      Assert.AreEqual(1, result.ID);
    }

    class ComplexSourceType
    {
      public int ID { get; set; }
      public NestedSourceType Foo { get; set; }
    }

    class NestedSourceType
    {
      public string Name { get; set; }
    }

    class ComplexDestinationType
    {
      public int ID { get; set; }
      public NestedDestinationType Foo { get; set; }
    }

    class NestedDestinationType
    {
      public string Name { get; set; }
    }

    [TestMethod]
    public void NestedProjectionWorks()
    {
      var mapper = new MemberMapper();

      var result = mapper.Project<ComplexSourceType, ComplexDestinationType>().Compile()(
        new ComplexSourceType
        {
          ID = 1,
          Foo = new NestedSourceType
          {
            Name = "Foo"
          }
        });

      Assert.AreEqual(1, result.ID);
      Assert.AreEqual("Foo", result.Foo.Name);
    }

    class EnumerableSourceType
    {
      public int ID { get; set; }

      public List<NestedEnumerableSourceType> Foos { get; set; }
    }

    class EnumerableDestinationType
    {
      public int ID { get; set; }
      public List<NestedEnumerableDestinationType> Foos { get; set; }
    }

    class NestedEnumerableSourceType
    {
      public string Name { get; set; }
    }

    class NestedEnumerableDestinationType
    {
      public string Name { get; set; }
    }

    [TestMethod]
    public void ProjectionOfEnumerableTypesWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<EnumerableSourceType, EnumerableDestinationType>().Compile();

      var result = projection(new EnumerableSourceType
        {
          Foos = new List<NestedEnumerableSourceType>
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
        });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }
  }
}
