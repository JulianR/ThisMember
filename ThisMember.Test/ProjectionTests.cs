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

    class ListType
    {
      public int ID { get; set; }

      public List<NestedEnumerableSourceType> Foos { get; set; }
    }

    class OtherIListType
    {
      public int ID { get; set; }
      public IList<NestedEnumerableDestinationType> Foos { get; set; }
    }

    class IListType
    {
      public int ID { get; set; }

      public IList<NestedEnumerableSourceType> Foos { get; set; }
    }

    class OtherListType
    {
      public int ID { get; set; }
      public List<NestedEnumerableDestinationType> Foos { get; set; }
    }

    class ArrayType
    {
      public int ID { get; set; }
      public NestedEnumerableSourceType[] Foos { get; set; }
    }

    class OtherArrayType
    {
      public int ID { get; set; }
      public NestedEnumerableDestinationType[] Foos { get; set; }
    }

    class IEnumerableType
    {
      public int ID { get; set; }
      public IEnumerable<NestedEnumerableSourceType> Foos { get; set; }
    }

    class OtherIEnumerableType
    {
      public int ID { get; set; }
      public IEnumerable<NestedEnumerableDestinationType> Foos { get; set; }
    }

    class NestedEnumerableSourceType
    {
      public string Name { get; set; }
    }

    class NestedEnumerableDestinationType
    {
      public string Name { get; set; }
    }

    class OtherICollectionType
    {
      public int ID { get; set; }
      public ICollection<NestedEnumerableDestinationType> Foos { get; set; }
    }

    class ICollectionType
    {
      public int ID { get; set; }

      public ICollection<NestedEnumerableSourceType> Foos { get; set; }
    }

    [TestMethod]
    public void ProjectionOfListToListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ListType, OtherListType>().Compile();

      var result = projection(new ListType
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

    [TestMethod]
    public void ProjectionOfArrayToListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ArrayType, OtherListType>().Compile();

      var result = projection(new ArrayType
      {
        Foos = new NestedEnumerableSourceType[]
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }

    [TestMethod]
    public void ProjectionOfListToArrayWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ListType, OtherArrayType>().Compile();

      var result = projection(new ListType
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

    [TestMethod]
    public void ProjectionOfArrayToArrayWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ArrayType, OtherArrayType>().Compile();

      var result = projection(new ArrayType
      {
        Foos = new NestedEnumerableSourceType[]
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }

    [TestMethod]
    public void ProjectionOfArrayToIEnumerableWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ArrayType, OtherIEnumerableType>().Compile();

      var result = projection(new ArrayType
      {
        Foos = new NestedEnumerableSourceType[]
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }

    [TestMethod]
    public void ProjectionOfIEnumerableToArrayWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IEnumerableType, OtherArrayType>().Compile();

      var result = projection(new IEnumerableType
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

    [TestMethod]
    public void ProjectionOfIEnumerableToIEnumerableWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IEnumerableType, OtherIEnumerableType>().Compile();

      var result = projection(new IEnumerableType
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

    [TestMethod]
    public void ProjectionOfListToIEnumerableWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ListType, OtherIEnumerableType>().Compile();

      var result = projection(new ListType
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

    [TestMethod]
    public void ProjectionOfIEnumerableToListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IEnumerableType, OtherListType>().Compile();

      var result = projection(new IEnumerableType
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

    [TestMethod]
    public void ProjectionOfIListToIListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IListType, OtherIListType>().Compile();

      var result = projection(new IListType
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

    [TestMethod]
    public void ProjectionOfArrayToIListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ArrayType, OtherIListType>().Compile();

      var result = projection(new ArrayType
      {
        Foos = new NestedEnumerableSourceType[]
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }

    [TestMethod]
    public void ProjectionOfIListToArrayWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IListType, OtherArrayType>().Compile();

      var result = projection(new IListType
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

    [TestMethod]
    public void ProjectionOfIListToIEnumerableWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IListType, OtherIEnumerableType>().Compile();

      var result = projection(new IListType
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

    [TestMethod]
    public void ProjectionOfIEnumerableToIListWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IEnumerableType, OtherIListType>().Compile();

      var result = projection(new IEnumerableType
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

    [TestMethod]
    public void ProjectionOfICollectionToICollectionWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ICollectionType, OtherICollectionType>().Compile();

      var result = projection(new ICollectionType
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

    [TestMethod]
    public void ProjectionOfArrayToICollectionWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ArrayType, OtherICollectionType>().Compile();

      var result = projection(new ArrayType
      {
        Foos = new NestedEnumerableSourceType[]
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.AreEqual("Test", result.Foos.Single().Name);
    }

    [TestMethod]
    public void ProjectionOfICollectionToArrayWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ICollectionType, OtherArrayType>().Compile();

      var result = projection(new ICollectionType
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

    [TestMethod]
    public void ProjectionOfICollectionToIEnumerableWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<ICollectionType, OtherIEnumerableType>().Compile();

      var result = projection(new ICollectionType
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

    [TestMethod]
    public void ProjectionOfIEnumerableToICollectionWorks()
    {
      var mapper = new MemberMapper();

      var projection = mapper.Project<IEnumerableType, OtherICollectionType>().Compile();

      var result = projection(new IEnumerableType
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

    [TestMethod]
    public void NoCollectionMappingOptionIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Projection.MapCollectionMembers = false;

      var projection = mapper.Project<IListType, OtherListType>().Compile();

      var result = projection(new IListType
      {
        ID = 10,
        Foos = new List<NestedEnumerableSourceType>
          {
            new NestedEnumerableSourceType
            {
              Name = "Test"
            }
          }
      });

      Assert.IsNull(result.Foos);
    }
  }
}
