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
    public void CanDefineAndUseVariable()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => 10);

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * 10);
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(100, result.Foo);
    }

    public class Bar
    {
      public static int Foo = 10;
    }

    [TestMethod]
    public void VariableCanBeInitializedAsComplexExpression()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => Bar.Foo);

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * 10);
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(100, result.Foo);

      Bar.Foo = 18;

      result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(180, result.Foo);
    }

    [TestMethod]
    public void MissingVariableIsInitializedAsDefaultValue()
    {
      var mapper = new MemberMapper();

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * 10);
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(0, result.Foo);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void MissingVariableThrowsExceptionWithOptionTurnedOn()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.UseDefaultValueForMissingVariable = false;

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * 10);
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());
    }

    [TestMethod]
    public void MoreComplexVariableUseWorks()
    {
      var mapper = new MemberMapper();

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int, int>(i => Variable.Use<string>("s").Length);
      };

      var result = mapper.Map(new Source { Foo = 1 }, new Destination());

      Assert.AreEqual(0, result.Foo);
    }


    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CantDefineVariableTwice()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => 10);
      mapper.ForSourceType<Destination>().DefineVariable<int>("i");
    }

    public class SourceInherited : Source { }
    public class DestinationInherited : Destination { }

    [TestMethod]
    public void CanDefineAndUseVariableOnInheritedType()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => 10);

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * i);
      };

      var result = mapper.Map(new SourceInherited { Foo = 1 }, new DestinationInherited());

      Assert.AreEqual(10, result.Foo);
    }

    [TestMethod]
    public void CanDefineAndUseVariableOnListType()
    {
      var mapper = new MemberMapper();
      mapper.ForSourceType<Source>().DefineVariable<int>("i").InitializedAs(() => 10);

      mapper.DefaultMemberOptions = (ctx, options) =>
      {
        options.Convert<int>(i => Variable.Use<int>("i") * 10);
      };

      var result = mapper.Map(new[] { new Source { Foo = 1 } }, new List<Destination>());

      Assert.AreEqual(100, result.First().Foo);
    }
  }
}
