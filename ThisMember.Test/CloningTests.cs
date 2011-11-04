using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class CloningTests
  {
    public class SourceType
    {
      public int Value { get; set; }
      public int[] Values { get; set; }
      public string Name { get; set; }
    }

    [TestMethod]
    public void MappingSameTypesMakesClone()
    {
      var mapper = new MemberMapper();

      var source = new SourceType { Name = "test", Value = 1, Values = new[] { 1 } };

      var result = mapper.Map<SourceType, SourceType>(source);

      Assert.AreEqual(1, result.Value);
      Assert.IsFalse(object.ReferenceEquals(result.Values, source.Values));
      Assert.IsTrue(result.Values.SequenceEqual(source.Values));
      Assert.IsTrue(object.ReferenceEquals(result.Name, source.Name));

    }

    [TestMethod]
    public void NoCloneIsMadeWithOptionTurnedOff()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource = false;

      var source = new SourceType { Name = "test", Value = 1, Values = new[] { 1 } };

      var result = mapper.Map<SourceType, SourceType>(source);

      Assert.AreEqual(1, result.Value);
      Assert.IsTrue(object.ReferenceEquals(result.Values, source.Values));
      Assert.IsTrue(result.Values.SequenceEqual(source.Values));
      Assert.IsTrue(object.ReferenceEquals(result.Name, source.Name));

    }

    [TestMethod]
    public void DeepCloneMethodWorks()
    {
      var mapper = new MemberMapper();

      var source = new SourceType { Name = "test", Value = 1, Values = new[] { 1 } };

      var result = mapper.DeepClone(source);

      Assert.AreEqual(1, result.Value);
      Assert.IsFalse(object.ReferenceEquals(result.Values, source.Values));
      Assert.IsTrue(result.Values.SequenceEqual(source.Values));
      Assert.IsTrue(object.ReferenceEquals(result.Name, source.Name));

    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DeepCloneThrowsIfOptionIsTurnedOff()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource = false;

      var source = new SourceType { Name = "test", Value = 1, Values = new[] { 1 } };

      var result = mapper.DeepClone(source);

    }

    public class NestedSourceType
    {
      public string Foo { get; set; }
      public string[] Foos { get; set; }
    }

    public class ParentSourceType
    {
      public int Value { get; set; }
      public int[] Values { get; set; }
      public string Name { get; set; }
      public NestedSourceType Bar { get; set; }
      public NestedSourceType[] Bars { get; set; }
    }

    [TestMethod]
    public void MappingSameComplexTypesMakesClone()
    {
      var mapper = new MemberMapper();

      var source = new ParentSourceType
      {
        Name = "test",
        Value = 1,
        Values = new[] { 1 },
        Bar = new NestedSourceType
        {
          Foo = "test",
          Foos = new[] 
          {
            "test",
            "test1"
          }
        },
        Bars = new[]
        {
          new NestedSourceType
          {
            Foo = "foo",
            Foos = new[] 
            {
              "foo",
              "foo"
            }
          }
        }
      };

      var result = mapper.Map<ParentSourceType, ParentSourceType>(source);

      Assert.AreEqual(1, result.Value);
      Assert.IsFalse(object.ReferenceEquals(result.Values, source.Values));
      Assert.IsTrue(result.Values.SequenceEqual(source.Values));
      Assert.IsTrue(object.ReferenceEquals(result.Name, source.Name));
      Assert.IsFalse(object.ReferenceEquals(result.Bar, source.Bar));
      Assert.IsFalse(object.ReferenceEquals(result.Bar.Foos, source.Bar.Foos));
      Assert.IsTrue(object.ReferenceEquals(result.Bar.Foo, source.Bar.Foo));
      Assert.IsTrue(result.Bar.Foos.SequenceEqual(source.Bar.Foos));
      Assert.IsFalse(object.ReferenceEquals(result.Bars.Single(), source.Bars.Single()));
    }

    [TestMethod]
    public void NoCloneIsMadeForComplexMapIfOptionIsTurnedOff()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource = false;

      var source = new ParentSourceType
      {
        Name = "test",
        Value = 1,
        Values = new[] { 1 },
        Bar = new NestedSourceType
        {
          Foo = "test",
          Foos = new[] 
          {
            "test",
            "test1"
          }
        },
        Bars = new[]
        {
          new NestedSourceType
          {
            Foo = "foo",
            Foos = new[] 
            {
              "foo",
              "foo"
            }
          }
        }
      };

      var result = mapper.Map<ParentSourceType, ParentSourceType>(source);

      Assert.AreEqual(1, result.Value);
      Assert.IsTrue(object.ReferenceEquals(result.Values, source.Values));
      Assert.IsTrue(result.Values.SequenceEqual(source.Values));
      Assert.IsTrue(object.ReferenceEquals(result.Name, source.Name));
      Assert.IsTrue(object.ReferenceEquals(result.Bar, source.Bar));
      Assert.IsTrue(object.ReferenceEquals(result.Bar.Foos, source.Bar.Foos));
      Assert.IsTrue(object.ReferenceEquals(result.Bar.Foo, source.Bar.Foo));
      Assert.IsTrue(result.Bar.Foos.SequenceEqual(source.Bar.Foos));
      Assert.IsTrue(object.ReferenceEquals(result.Bars.Single(), source.Bars.Single()));
    }

    class NestedA
    {
      public string Bar { get; set; }
      public NestedB Nested { get; set; }
    }

    class NestedB
    {
      public string Bar { get; set; }
      public NestedC[] Nested { get; set; }
    }

    class NestedC
    {
      public string Bar { get; set; }
      public NestedD Nested { get; set; }
    }

    class NestedD
    {
      public string Bar { get; set; }
    }

    [TestMethod]
    public void DefaultNestingDepthWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var nested = new NestedA
      {
        Bar = "A",
        Nested = new NestedB
        {
          Bar = "B",
          Nested = new[]
          {
            new NestedC
            {
              Bar = "C",
              Nested = new NestedD
              {
                Bar = "D"
              }
            }
          }
        }
      };

      var clone = mapper.DeepClone(nested);

      Assert.IsNotNull(clone.Nested);
      Assert.IsNotNull(clone.Nested.Nested);
      Assert.IsNull(clone.Nested.Nested.Single().Nested);
    }

    [TestMethod]
    public void ChangedNestingDepthWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var nested = new NestedA
      {
        Bar = "A",
        Nested = new NestedB
        {
          Bar = "B",
          Nested = new[]
          {
            new NestedC
            {
              Bar = "C",
              Nested = new NestedD
              {
                Bar = "D"
              }
            }
          }
        }
      };
      mapper.Options.Cloning.MaxCloneDepth = 1;
      var clone = mapper.DeepClone(nested);

      Assert.IsNotNull(clone.Nested);
      Assert.IsNull(clone.Nested.Nested);
    }

    [TestMethod]
    public void UnlimitedNestingDepthWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var nested = new NestedA
      {
        Bar = "A",
        Nested = new NestedB
        {
          Bar = "B",
          Nested = new[]
          {
            new NestedC
            {
              Bar = "C",
              Nested = new NestedD
              {
                Bar = "D"
              }
            }
          }
        }
      };

      mapper.Options.Cloning.MaxCloneDepth = null;
      var clone = mapper.DeepClone(nested);

      Assert.IsNotNull(clone.Nested);
      Assert.IsNotNull(clone.Nested.Nested);
      Assert.IsNotNull(clone.Nested.Nested.Single().Nested);
    }
  }
}
