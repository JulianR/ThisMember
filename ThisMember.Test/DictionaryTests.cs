using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using System.Collections.Concurrent;

namespace ThisMember.Test
{
  [TestClass]
  public class DictionaryTests
  {

    public class DictionaryClass
    {
      public Dictionary<int, string> Foo { get; set; }
    }

    public class IDictionaryClass
    {
      public IDictionary<int, string> Foo { get; set; }
    }

    public class ConcurrentDictionaryClass
    {
      public ConcurrentDictionary<int, string> Foo { get; set; }
    }

    public class ListClass
    {
      public List<string> Foo { get; set; }
    }

    public class IListClass
    {
      public IList<string> Foo { get; set; }
    }

    public class ArrayClass
    {
      public string[] Foo { get; set; }
    }

    public class ICollectionClass
    {
      public ICollection<string> Foo { get; set; }
    }

    [TestMethod]
    public void DictionaryToIDictionaryIsReferenceEqual()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() };
      var result = mapper.Map(source, new IDictionaryClass());

      Assert.IsTrue(object.ReferenceEquals(source.Foo, result.Foo));
    }

    [TestMethod]
    public void MappingIDictionaryToDictionaryWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new DictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingIDictionaryToConcurrentDictionaryWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ConcurrentDictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingDictionaryToConcurrentDictionaryWorks()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ConcurrentDictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingDictionaryToListWorks()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo[0]);
      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingDictionaryToIListWorks()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new IListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo[0]);
      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingDictionaryToArrayWorks()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ArrayClass() { Foo = new [] { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void MappingDictionaryToICollectionWorks()
    {
      var mapper = new MemberMapper();
      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ICollectionClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo.ElementAt(0));
      Assert.AreEqual("test", result.Foo.ElementAt(1));
    }

    [TestMethod]
    public void MappingIDictionaryToListWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo[0]);
      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingIDictionaryToIListWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new IListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo[0]);
      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void MappingIDictionaryToArrayWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ArrayClass() { Foo = new [] { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void MappingIDictionaryToICollectionWorks()
    {
      var mapper = new MemberMapper();
      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ICollectionClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test1", result.Foo.ElementAt(0));
      Assert.AreEqual("test", result.Foo.ElementAt(1));
    }

    // preserve contents off

    [TestMethod]
    public void DontPreserveContents_DictionaryToIDictionaryIsReferenceEqual()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() };
      var result = mapper.Map(source, new IDictionaryClass());

      Assert.IsTrue(object.ReferenceEquals(source.Foo, result.Foo));
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToDictionaryWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new DictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToConcurrentDictionaryWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ConcurrentDictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingDictionaryToConcurrentDictionaryWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ConcurrentDictionaryClass());

      Assert.AreEqual("test", result.Foo[1]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingDictionaryToListWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingDictionaryToIListWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new IListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingDictionaryToArrayWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ArrayClass() { Foo = new [] { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingDictionaryToICollectionWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new DictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ICollectionClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test", result.Foo.First());
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToListWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToIListWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new IListClass() { Foo = new List<string> { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToArrayWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ArrayClass() { Foo = new [] { "test1" } });

      Assert.AreEqual("test", result.Foo[0]);
    }

    [TestMethod]
    public void DontPreserveContents_MappingIDictionaryToICollectionWorks()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.PreserveDestinationListContents = false;

      var source = new IDictionaryClass { Foo = new Dictionary<int, string>() { { 1, "test" } } };
      var result = mapper.Map(source, new ICollectionClass());

      Assert.AreEqual("test", result.Foo.First());
    }

    public class DictionaryClassComplex1
    {
      public Dictionary<int, DictionaryValue1> Foo { get; set; }
    }

    public class DictionaryValue1
    {
      public string Foo { get; set; }
    }

    public class DictionaryClassComplex2
    {
      public Dictionary<int, DictionaryValue2> Foo { get; set; }
    }

    public class DictionaryValue2
    {
      public string Foo { get; set; }
    }

    //[TestMethod]
    public void MappingDictionaryWithMappableValuesWorks()
    {
      var mapper = new MemberMapper();

      var source = new DictionaryClassComplex1 { Foo = new Dictionary<int, DictionaryValue1>() 
      { 
        { 
          1, new DictionaryValue1 { Foo = "test" } } 
        } 
      };

      var result = mapper.Map(source, new DictionaryClassComplex2());

      Assert.AreEqual("test", result.Foo.First().Value.Foo);

    }
  }
}
