using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class MemberMapperEnumerableTests
  {

    private class SourceElement
    {
      public string Value { get; set; }
    }

    private class DestElement
    {
      public string Value { get; set; }
    }

    private class SourceListType
    {
      public List<SourceElement> List { get; set; }
    }

    private class DestinationListType
    {
      public List<DestElement> List { get; set; }
    }

    private class SourceIListType
    {
      public IList<SourceElement> List { get; set; }
    }

    private class DestinationIListType
    {
      public IList<DestElement> List { get; set; }
    }

    private class SourceEnumerableType
    {
      public IEnumerable<SourceElement> List { get; set; }
    }

    private class DestinationEnumerableType
    {
      public IEnumerable<DestElement> List { get; set; }
    }

    private class SourceArrayType
    {
      public SourceElement[] List { get; set; }
    }

    private class DestinationArrayType
    {
      public DestElement[] List { get; set; }
    }

    private class SourceSimpleListType
    {
      public List<string> List { get; set; }
    }

    private class DestinationSimpleListType
    {
      public List<string> List { get; set; }
    }

    private class SourceSimpleIListType
    {
      public List<string> List { get; set; }
    }

    private class DestinationSimpleIListType
    {
      public List<string> List { get; set; }
    }

    private class SourceSimpleArrayType
    {
      public string[] List { get; set; }
    }

    private class DestinationSimpleArrayType
    {
      public string[] List { get; set; }
    }

    [TestMethod]
    public void ListToListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void ListToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void IListToListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void IListToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count, result.List.Count);
      Assert.AreEqual("X", result.List[0].Value);

    }

    [TestMethod]
    public void ListToEnumerableIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void IListToEnumerableIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToEnumerableIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToArrayIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ArrayToEnumerableIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceArrayType
      {
        List = new SourceElement[]
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceArrayType, DestinationEnumerableType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void IListToArrayIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceIListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceIListType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void ListToArrayIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceListType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceListType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);

    }

    [TestMethod]
    public void EnumerableToArrayIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceEnumerableType
      {
        List = new List<SourceElement>
        {
          new SourceElement
          {
            Value = "X"
          }
        }
      };

      var result = mapper.Map<SourceEnumerableType, DestinationArrayType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single().Value);
    }

    [TestMethod]
    public void SimpleListToListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceSimpleListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleListType, DestinationSimpleListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleListToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceSimpleListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleListType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleIListToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceSimpleIListType
      {
        List = new List<string>
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleIListType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void SimpleArrayToIListIsMappedCorrectly()
    {

      var mapper = new MemberMapper();

      var source = new SourceSimpleArrayType
      {
        List = new string[]
        {
          "X"
        }
      };

      var result = mapper.Map<SourceSimpleArrayType, DestinationSimpleIListType>(source);

      Assert.AreEqual(source.List.Count(), result.List.Count());
      Assert.AreEqual("X", result.List.Single());
    }

    [TestMethod]
    public void ListTypeIsMappedToListTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceElement>
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var result = mapper.Map<List<SourceElement>, List<DestElement>>(source);

      Assert.AreEqual("X", result.First().Value);

    }

    [TestMethod]
    public void IEnumerableTypeIsMappedToListTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceElement>
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var result = mapper.Map<IEnumerable<SourceElement>, List<DestElement>>(source);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ArrayTypeIsMappedToListTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceElement[]
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var result = mapper.Map<SourceElement[], List<DestElement>>(source);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ListTypeIsMappedToArrayTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceElement>
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var destination = new DestElement[0];

      var result = mapper.Map<List<SourceElement>, DestElement[]>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void IEnumerableTypeIsMappedToArrayTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<SourceElement>
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var destination = new DestElement[0];

      var result = mapper.Map<IEnumerable<SourceElement>, DestElement[]>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void PrimitiveIEnumerableTypeIsMappedToArrayTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<int>
      {
        1,2,3,4
      };

      var destination = new int[0];

      var result = mapper.Map<IEnumerable<int>, int[]>(source, destination);

      Assert.IsTrue(result.SequenceEqual(source));
    }

    [TestMethod]
    public void PrimitiveIEnumerableTypeIsMappedToObservableCollectionTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new List<int>
      {
        1,2,3,4
      };

      //var destination = new int[0];

      //var result = mapper.Map<IEnumerable<int>, System.Collections.ObjectModel. >>(source, destination);

      //Assert.IsTrue(result.SequenceEqual(source));
    }
  }
}
