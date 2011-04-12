using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Exceptions;

namespace ThisMember.Test
{
  [TestClass]
  public class OptionTests
  {

    class Source
    {
      public int ID { get; set; }
    }

    class Destination
    {
      public string ID { get; set; }
    }


    [TestMethod]
    public void ToStringIsCalledWhenTypesDoNotMatch()
    {
      var mapper = new MemberMapper();

      var src = new Source
      {
        ID = 10
      };

      var dest = mapper.Map<Source, Destination>(src);

      Assert.AreEqual("10", dest.ID);
    }

    [TestMethod]
    [ExpectedException(typeof(CodeGenerationException))]
    public void ExceptionIsThrownWhenTypesDoNotMatch()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.CallToStringWhenDestinationIsString = false;

      var src = new Source
      {
        ID = 10
      };

      var dest = mapper.Map<Source, Destination>(src);
    }

    [TestMethod]
    public void ToStringIsCalledWhenTypesDoNotMatchOnCollection()
    {
      var mapper = new MemberMapper();

      var src = new Source
      {
        ID = 10
      };

      var dest = mapper.Map<Source[], List<Destination>>(new[] { src });

      Assert.IsTrue(dest.All(d => d.ID == "10"));
    }

    [TestMethod]
    [ExpectedException(typeof(CodeGenerationException))]
    public void ExceptionIsThrownWhenTypesDoNotMatchOnCollection()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.CallToStringWhenDestinationIsString = false;

      var src = new Source
      {
        ID = 10
      };

      var dest = mapper.Map<Source[], List<Destination>>(new[] { src });
    }
  }
}
