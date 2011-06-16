using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using System.Linq.Expressions;
using System.Globalization;

namespace ThisMember.Test
{

  [TestClass]
  public class CompilationTests
  {
    private int Foo()
    {
      return 1;
    }

    public int Bar()
    {
      return 1;
    }

    [TestMethod]
    public void ExpressionNotUsingExternalOperationsIsNotDetected()
    {
      Expression<Func<int, int, int>> expr = (a, b) => a + b;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsFalse(processor.NonPublicMembersAccessed);
    }

    public class PublicSource
    {
      public int Foo { get; set; }
    }

    public class PublicDest
    {
      public int Bar { get; set; }
    }

    [TestMethod]
    public void ExpressionUsingPublicTypesIsNotDetected()
    {
      Expression<Func<PublicSource, PublicDest, int>> expr = (a, b) => a.Foo + b.Bar;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsFalse(processor.NonPublicMembersAccessed);
    }

    internal class InternalSource
    {
      public int Foo { get; set; }
    }

    internal class InternalDest
    {
      public int Bar { get; set; }
    }

    [TestMethod]
    public void ExpressionUsingInternalTypesIsDetected()
    {
      Expression<Func<InternalSource, InternalDest, int>> expr = (a, b) => a.Foo + b.Bar;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    private class PrivateSource
    {
      public int Foo { get; set; }
    }

    private class PrivateDest
    {
      public int Bar { get; set; }
    }

    [TestMethod]
    public void ExpressionUsingPrivateTypesIsDetected()
    {
      Expression<Func<PrivateSource, PrivateDest, int>> expr = (a, b) => a.Foo + b.Bar;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    public class GenericSource<T>
    {
      public T Foo { get; set; }
    }

    public class GenericDest<T>
    {
      public T Bar { get; set; }
    }

    [TestMethod]
    public void ExpressionUsingGenericTypesIsDetected()
    {
      Expression<Func<GenericSource<int>, GenericDest<int>, int>> expr = (a, b) => a.Foo + b.Bar;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    [TestMethod]
    public void AccessingPrivateMethodIsDetected()
    {
      Expression<Func<int, int, int>> expr = (a, b) => Foo();

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    [TestMethod]
    public void AccessingPublicMethodIsNotDetected()
    {
      Expression<Func<int, int, int>> expr = (a, b) => Bar();

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsFalse(processor.NonPublicMembersAccessed);
    }

    [TestMethod]
    public void AccessingCapturedVariableIsDetected()
    {
      var i = 1;
      Expression<Func<int, int, int>> expr = (a, b) => a + b + i;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    [TestMethod]
    public void AccessingNonTrivialCapturedVariableIsDetected()
    {
      var info = new CultureInfo("en-US");

      Expression<Func<int, int, int>> expr = (a, b) => a + b + info.EnglishName.Length;

      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }




  }

  [TestClass]
  public class MoreCompilationTests
  {
    
    public class NestedClass
    {
      public int Bar()
      {
        return 1;
      }

      public Expression GenerateExpression()
      {
        Expression<Func<int, int, int>> expr = (a, b) => Bar();
        return expr;
      }
    }

    private class PrivateNestedClass
    {
      public int Bar()
      {
        return 1;
      }

      public Expression GenerateExpression()
      {
        Expression<Func<int, int, int>> expr = (a, b) => Bar();
        return expr;
      }
    }

    [TestMethod]
    public void AccessingMethodFromNestedPublicTypeIsDetected()
    {

      var expr = new NestedClass().GenerateExpression();
      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }

    [TestMethod]
    public void AccessingMethodFromNonPublicTypeIsDetected()
    {

      var expr = new PrivateNestedClass().GenerateExpression();
      var processor = new MapProposalProcessor(new MemberMapper());

      processor.Process(expr);

      Assert.IsTrue(processor.NonPublicMembersAccessed);
    }
  }
}
