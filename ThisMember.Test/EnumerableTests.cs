using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class EnumerableTests
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
      public IList<string> List { get; set; }
    }

    private class DestinationSimpleIListType
    {
      public IList<string> List { get; set; }
    }

    private class SourceSimpleArrayType
    {
      public string[] List { get; set; }
    }

    private class DestinationSimpleArrayType
    {
      public string[] List { get; set; }
    }

    private class SourceSimpleICollectionType
    {
      public ICollection<string> List { get; set; }
    }

    private class DestinationSimpleICollectionType
    {
      public ICollection<string> List { get; set; }
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

    class SourceMultipleIEnumerables
    {
      public IEnumerable<int> One { get; set; }
      public IEnumerable<int> Two { get; set; }
    }

    class DestMultipleLists
    {
      public List<int> One { get; set; }
      public List<int> Two { get; set; }
    }

    [TestMethod]
    public void TypeWithMultipleIEnumerableIsMappedCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceMultipleIEnumerables
      {
        One = new List<int>
        {
          1,2,3
        },
        Two = new List<int>
        {
          4,5,6
        }
      };

      var result = mapper.Map<SourceMultipleIEnumerables, DestMultipleLists>(source);

      Assert.IsTrue(result.One.SequenceEqual(source.One));
      Assert.IsTrue(result.Two.SequenceEqual(source.Two));

    }

    class SourceMultipleLists
    {
      public List<int> One { get; set; }
      public List<int> Two { get; set; }
    }

    class DestMultipleArray
    {
      public int[] One { get; set; }
      public int[] Two { get; set; }
    }

    [TestMethod]
    public void TypeWithMultipleListsIsMappedCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceMultipleLists
      {
        One = new List<int>
        {
          1,2,3
        },
        Two = new List<int>
        {
          4,5,6
        }
      };

      var result = mapper.Map<SourceMultipleLists, DestMultipleArray>(source);

      Assert.IsTrue(result.One.SequenceEqual(source.One));
      Assert.IsTrue(result.Two.SequenceEqual(source.Two));

    }

    [TestMethod]
    public void TypeWithMultipleArraysIsMappedCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new DestMultipleArray
      {
        One = new[]
        {
          1,2,3
        },
        Two = new[]
        {
          4,5,6
        }
      };

      var result = mapper.Map<DestMultipleArray, SourceMultipleLists>(source);

      Assert.IsTrue(result.One.SequenceEqual(source.One));
      Assert.IsTrue(result.Two.SequenceEqual(source.Two));

    }

    class ICollectionElement
    {
      public string Name { get; set; }
    }

    class ICollectionType
    {
      public ICollection<ICollectionElement> Elements { get; set; }
    }

    [TestMethod]
    public void ICollectionIsCorrectlyMapped()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<ICollectionType, ICollectionType>(new ICollectionType { Elements = new List<ICollectionElement> { new ICollectionElement { Name = "Test" } } });

      Assert.AreEqual("Test", result.Elements.Single().Name);

    }

    class ListDestinationTypeForICollection
    {

    }

    [TestMethod]
    public void ICollectionToListIsCorrectlyMapped()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<ICollectionType, ICollectionType>(new ICollectionType { Elements = new List<ICollectionElement> { new ICollectionElement { Name = "Test" } } });

      Assert.AreEqual("Test", result.Elements.Single().Name);
    }

    [TestMethod]
    public void ICollectionTypeIsMappedToListTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceElement[]
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var result = mapper.Map<ICollection<SourceElement>, List<DestElement>>(source);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ICollectionTypeIsMappedToArrayTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceElement[]
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var destination = new DestElement[0];

      var result = mapper.Map<ICollection<SourceElement>, DestElement[]>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ICollectionTypeIsMappedToIEnumerableTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceElement[]
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var destination = new DestElement[0];

      var result = mapper.Map<ICollection<SourceElement>, IEnumerable<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ListTypeIsMappedToICollectionTypeCorrectly()
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

      var result = mapper.Map<List<SourceElement>, ICollection<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void IListTypeIsMappedToICollectionTypeCorrectly()
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

      var result = mapper.Map<IList<SourceElement>, ICollection<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void IEnumerableTypeIsMappedToICollectionTypeCorrectly()
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

      var result = mapper.Map<IEnumerable<SourceElement>, ICollection<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ICollectionTypeIsMappedToICollectionTypeCorrectly()
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

      var result = mapper.Map<ICollection<SourceElement>, ICollection<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ArrayTypeIsMappedToICollectionTypeCorrectly()
    {
      var mapper = new MemberMapper();

      var source = new SourceElement[]
      {
        new SourceElement
        {
          Value = "X"
        }
      };

      var destination = new DestElement[0];

      var result = mapper.Map<SourceElement[], ICollection<DestElement>>(source, destination);

      Assert.AreEqual("X", result.First().Value);
    }

    [TestMethod]
    public void ExistingListsValuesArePersisted()
    {
      var mapper = new MemberMapper();

      var list = new List<int> { 1, 2, 3, 4, 5 };

      var array = new[] { 6, 7, 8, 9, 0 };

      mapper.Map<int[], List<int>>(array, list);

      Assert.IsTrue(list.Contains(9));
      Assert.AreEqual(10, list.Count);

    }

    class SourceArrayPropertyClass
    {
      public int[] Foo { get; set; }
    }

    class DestinationListPropertyClass
    {
      public List<int> Foo { get; set; }
    }

    [TestMethod]
    public void ExistingListsValuesArePersistedOnListsThatAreProperties()
    {
      var mapper = new MemberMapper();

      var list = new List<int> { 1, 2, 3, 4, 5 };

      var array = new[] { 6, 7, 8, 9, 0 };

      var source = new SourceArrayPropertyClass
      {
        Foo = array
      };

      var destination = new DestinationListPropertyClass
      {
        Foo = list
      };

      mapper.Map(source, destination);

      Assert.IsTrue(list.Contains(9));
      Assert.AreEqual(10, list.Count);

    }

    class SourceEnumerablePropertyClass
    {
      public IEnumerable<int> Foo { get; set; }
    }

    class DestinationIListPropertyClass
    {
      public IList<int> Foo { get; set; }
    }

    [TestMethod]
    public void ExistingListsValuesArePersistedOnIListsThatAreProperties()
    {
      var mapper = new MemberMapper();

      var list = new List<int> { 1, 2, 3, 4, 5 };

      var array = new[] { 6, 7, 8, 9, 0 };

      var source = new SourceEnumerablePropertyClass
      {
        Foo = array
      };

      var destination = new DestinationIListPropertyClass
      {
        Foo = list
      };

      mapper.Map(source, destination);

      Assert.IsTrue(list.Contains(9));
      Assert.AreEqual(10, list.Count);

    }

  }
}
