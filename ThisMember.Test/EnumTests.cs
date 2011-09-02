using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class EnumTests
  {
    public enum Foo
    {
      None = 0,
      A = 1,
      B = 2,
      C = 3
    }

    public class EnumClass
    {
      public Foo Foo { get; set; }
    }

    public class IntClass
    {
      public int Foo { get; set; }
    }

    [TestMethod]
    public void EnumIsMappedToInt()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<EnumClass, IntClass>(new EnumClass { Foo = Foo.B });

      Assert.AreEqual(2, result.Foo);

    }

    [TestMethod]
    public void IntIsMappedToEnum()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<IntClass, EnumClass>(new IntClass { Foo = 2 });

      Assert.AreEqual(Foo.B, result.Foo);

    }
  }
}
