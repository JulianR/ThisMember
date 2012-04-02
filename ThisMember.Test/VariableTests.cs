using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Fluent;

namespace ThisMember.Test
{
  [TestClass]
  public class VariableTests
  {
    public class Source { public int Foo { get; set; } }
    public class Destination { public int Foo { get; set; } }

    [TestMethod]
    public void TestMethod1()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => 10);

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int, int>(i => Variable.Use<int>("i"));
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(10, result.Foo);
    }
  }
}
