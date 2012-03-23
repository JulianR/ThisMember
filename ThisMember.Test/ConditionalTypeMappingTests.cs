using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Interfaces;
using ThisMember.Core.Exceptions;

namespace ThisMember.Test
{

  [TestClass]
  public class ConditionalTypeMappingTests
  {
    public interface ISource
    {
      bool Valid { get; set; }
    }

    public class Source : ISource
    {
      public string Foo { get; set; }
      public bool Valid { get; set; }
    }

    public class Destination
    {
      public string Foo { get; set; }
    }

    public class SourceInherited : Source
    {
      public int Bar { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMet()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test"
      };

      mapper.Map<Source, Destination>(source);
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMet()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test",
        Valid = true
      };

      var result = mapper.Map<Source, Destination>(source);

      Assert.AreEqual("test", result.Foo);
    }

    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMetForSubtype()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceInherited
      {
        Foo = "test",
        Bar = 10
      };

      mapper.Map<SourceInherited, Destination>(source);
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMetForSubtype()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceInherited
      {
        Foo = "test",
        Bar = 10,
        Valid = true
      };

      var result = mapper.Map<SourceInherited, Destination>(source);
      Assert.AreEqual("test", result.Foo);
    }

    public class SourceNested
    {
      public Source Nested { get; set; }
    }

    public class DestinationNested
    {
      public Destination Nested { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMetForNestedType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceNested
      {
        Nested = new Source
        {
          Foo = "test"
        }
      };

      mapper.Map<SourceNested, DestinationNested>(source);
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMetForNestedType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceNested
      {
        Nested = new Source
        {
          Foo = "test",
          Valid = true
        }
      };

      var result = mapper.Map<SourceNested, DestinationNested>(source);

      Assert.AreEqual("test", result.Nested.Foo);
    }

    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMetForCollectionOfType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test"
      };

      mapper.Map<IEnumerable<Source>, List<Destination>>(new[] { source });
    }

    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMetForCollectionOfNestedType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceNested
      {
        Nested = new Source
        {
          Foo = "test"
        }
      };

      mapper.Map<IEnumerable<SourceNested>, List<DestinationNested>>(new[] { source });
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMetForCollectionOfType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test",
        Valid = true
      };

      var result = mapper.Map<IEnumerable<Source>, List<Destination>>(new[] { source });

      Assert.AreEqual("test", result.Single().Foo);
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMetForCollectionOfNestedType()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<Source>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new SourceNested
      {
        Nested = new Source
        {
          Foo = "test",
          Valid = true
        }
      };

      var result = mapper.Map<IEnumerable<SourceNested>, List<DestinationNested>>(new[] { source });

      Assert.AreEqual("test", result.Single().Nested.Foo);
    }


    [TestMethod]
    [ExpectedException(typeof(MappingTerminatedException))]
    public void MappingThrowsWhenConditionIsNotMetForInterface()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<ISource>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test"
      };

      mapper.Map<Source, Destination>(source);
    }

    [TestMethod]
    public void MappingDoesNotThrowWhenConditionIsMetForInterface()
    {
      IMemberMapper mapper = new MemberMapper();

      mapper.ForSourceType<ISource>().ThrowIf(src => !src.Valid, "Source not valid!");

      var source = new Source
      {
        Foo = "test",
        Valid = true
      };

      var result = mapper.Map<Source, Destination>(source);

      Assert.AreEqual("test", result.Foo);
    }

  }
}
