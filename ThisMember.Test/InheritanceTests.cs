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
  }
}
