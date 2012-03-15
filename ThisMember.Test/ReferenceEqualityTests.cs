using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ReferenceEqualityTests
  {

    class WithListofString
    {
      public List<string> Foo { get; set; }
    }

    [TestMethod]
    public void WithListStringToWithListString()
    {
      var mapper = new MemberMapper();

      mapper.Options.Debug.DebugInformationEnabled = true;

      var source = new WithListofString { Foo = new List<string> { "test" } };

      var result = mapper.Map(source, new WithListofString());

      Assert.IsTrue(object.ReferenceEquals(source.Foo, result.Foo));

      var x = result;

      result.Foo = x.Foo;

      var newResult = mapper.Map(source, result);

      Assert.IsTrue(object.ReferenceEquals(result, newResult));
      Assert.IsTrue(object.ReferenceEquals(result.Foo, newResult.Foo));
      Assert.AreEqual(1, newResult.Foo.Count);

      var otherSource = new WithListofString { Foo = new List<string> { "test1" } };

      mapper.Map(otherSource, result);

      Assert.IsTrue(object.ReferenceEquals(result.Foo, newResult.Foo));
      Assert.AreEqual(2, result.Foo.Count);
    }

    class ComplexType1
    {
      public NestedType1 Foo;
      public NestedType2 Bar;
    }

    class ComplexType2
    {
      public NestedType1 Foo { get; set; }
      public NestedType3 Bar { get; set; }
    }

    class NestedType1
    {
      public string Foo { get; set; }
    }

    class NestedType2
    {
      public string Foo { get; set; }
    }

    class NestedType3
    {
      public string Foo { get; set; }
    }

    [TestMethod]
    public void NestedTypesAreReferenceEqual()
    {
      var mapper = new MemberMapper();

      var source = new ComplexType1
      {
        Foo = new NestedType1
        {
          Foo = "test"
        },
        Bar = new NestedType2
        {
          Foo = "test2"
        }
      };

      var result = mapper.Map(source, new ComplexType2());

      Assert.IsTrue(object.ReferenceEquals(result.Foo, source.Foo));
      Assert.IsFalse(object.ReferenceEquals(result.Bar, source.Bar));
      Assert.AreEqual("test", result.Foo.Foo);
      Assert.AreEqual("test2", result.Bar.Foo);
    }
  }
}
