using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class InheritanceTests
  {

    class SourceBase
    {
      public int ID { get; set; }
    }

    class SourceInherited : SourceBase
    {
    }

    class DestinationBase
    {
      public int Test { get; set; }
    }

    class DestinationInherited : DestinationBase
    {
    }

    [TestMethod]
    public void BaseClassMappingIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<SourceBase, DestinationBase>(customMapping: src => new DestinationBase
      {
        Test = src.ID
      });

      var result = mapper.Map<SourceInherited, DestinationInherited>(new SourceInherited { ID = 10 });

      Assert.AreEqual(10, result.Test);

    }

    class SecondSourceInherited : SourceInherited
    {
      public string Foo { get; set; }
    }

    class SecondDestinationInherited : DestinationInherited
    {
      public string Bar { get; set; }
    }

    class ThirdSourceInherited : SecondSourceInherited
    {
      public string AB { get; set; }
    }

    class ThirdDestinationInherited : SecondDestinationInherited
    {
      public string CD { get; set; }
    }

    [TestMethod]
    public void BaseClassMappingAndInheritedClassMappingIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<SourceBase, DestinationBase>(customMapping: src => new DestinationBase
      {
        Test = src.ID
      });

      mapper.CreateMap<SecondSourceInherited, SecondDestinationInherited>(customMapping: src => new SecondDestinationInherited
      {
        Bar = src.Foo
      });

      var result = mapper.Map<SecondSourceInherited, SecondDestinationInherited>(new SecondSourceInherited 
      { 
        ID = 10, 
        Foo = "test"
      });

      Assert.AreEqual(10, result.Test);
      Assert.AreEqual("test", result.Bar);
    }

    [TestMethod]
    public void CustomMappingsInEntireHierarchyAreRespected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<SourceBase, DestinationBase>(customMapping: src => new DestinationBase
      {
        Test = src.ID
      });

      mapper.CreateMap<SecondSourceInherited, SecondDestinationInherited>(customMapping: src => new SecondDestinationInherited
      {
        Bar = src.Foo
      });

      mapper.CreateMap<ThirdSourceInherited, ThirdDestinationInherited>(customMapping: src => new ThirdDestinationInherited
      {
        CD = src.AB + "test"
      });

      var result = mapper.Map<ThirdSourceInherited, ThirdDestinationInherited>(new ThirdSourceInherited
      {
        ID = 10,
        Foo = "test",
        AB = "test"
      });

      Assert.AreEqual(10, result.Test);
      Assert.AreEqual("test", result.Bar);
      Assert.AreEqual("testtest", result.CD);

    }

    interface Interface
    {
      string Foo { get; }
    }

    class Implementation : Interface
    {
      public string Foo { get; set; }
    }

    class ImplementationSource
    {
      public string Bar { get; set; }
    }


    [TestMethod]
    public void MappingDefinedOnInterfaceIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<ImplementationSource, Interface>(customMapping: src => new
        {
          Foo = src.Bar
        });


      var result = mapper.Map<ImplementationSource, Implementation>(new ImplementationSource { Bar = "test" });

      Assert.AreEqual("test", result.Foo);

    }

    public interface ISource
    {
      int Foo { get; set; }
    }

    public class Source : ISource
    {
      public int Foo { get; set; }
    }

    public interface IDest
    {
      int Bar { get; set; }
    }

    public class Dest : IDest
    {
      public int Bar { get; set; }
    }

    [TestMethod]
    public void MappingDefinedOnSourceBaseTypeOnlyIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<ISource, Dest>(src => new Dest
      {
        Bar = src.Foo * 10
      });

      var result = mapper.Map<Source, Dest>(new Source { Foo = 10 });

      Assert.AreEqual(100, result.Bar);

    }

    [TestMethod]
    public void MappingDefinedOnSourceBaseTypeOnlyIsRespectedForIDestFromISource()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<ISource, IDest>(src => new Dest
      {
        Bar = src.Foo * 10
      });

      var result = mapper.Map<Source, Dest>(new Source { Foo = 10 });

      Assert.AreEqual(100, result.Bar);

    }

    [TestMethod]
    public void MappingDefinedOnSourceBaseTypeOnlyIsRespectedForIDestFromSource()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, IDest>(src => new Dest
      {
        Bar = src.Foo * 10
      });

      var result = mapper.Map<Source, Dest>(new Source { Foo = 10 });

      Assert.AreEqual(100, result.Bar);

    }
  }
}
