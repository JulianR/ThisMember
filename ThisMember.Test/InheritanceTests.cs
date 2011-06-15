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

      var result = mapper.Map<SecondSourceInherited, SecondDestinationInherited>(new SecondSourceInherited { ID = 10, Foo = "test" });

      Assert.AreEqual(10, result.Test);
      Assert.AreEqual("test", result.Bar);

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
  }
}
