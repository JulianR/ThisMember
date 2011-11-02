using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class NullableValueTypesTests
  {

    public class NonNullableSource
    {
      public int ID { get; set; }
    }

    public class NullableDestination
    {
      public int? ID { get; set; }
    }

    public class NullableSource
    {
      public int? ID { get; set; }
    }

    public class NonNullableDestination
    {
      public int ID { get; set; }
    }

    [TestMethod]
    public void NonNullableGetsMappedToNullable()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NonNullableSource, NullableDestination>(new NonNullableSource { ID = 10 });

      Assert.AreEqual(10, result.ID.Value);

    }

    [TestMethod]
    public void NullableWithNonNullValueGetsMappedToNonNullable()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSource, NonNullableDestination>(new NullableSource { ID = 10 });

      Assert.AreEqual(10, result.ID);

    }

    [TestMethod]
    public void NullableWithNullValueGetsMappedToNonNullable()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSource, NonNullableDestination>(new NullableSource { ID = null });

      Assert.AreEqual(default(int), result.ID);

    }

    [TestMethod]
    public void NullableToNullableWorks()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSource, NullableSource>(new NullableSource { ID = 10 });

      Assert.AreEqual(10, result.ID);

    }

    public class NonNullableSourceNonPrimitiveVT
    {
      public decimal ID { get; set; }
    }

    public class NullableDestinationNonPrimitiveVT
    {
      public decimal? ID { get; set; }
    }

    public class NullableSourceNonPrimitiveVT
    {
      public decimal? ID { get; set; }
    }

    public class NonNullableDestinationNonPrimitiveVT
    {
      public decimal ID { get; set; }
    }

    [TestMethod]
    public void NonNullableGetsMappedToNonNullableNonPrimitiveVT()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NonNullableSourceNonPrimitiveVT, NonNullableSourceNonPrimitiveVT>(new NonNullableSourceNonPrimitiveVT { ID = 10 });

      Assert.AreEqual(10, result.ID);

    }


    [TestMethod]
    public void NonNullableGetsMappedToNullableNonPrimitiveVT()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NonNullableSourceNonPrimitiveVT, NullableDestinationNonPrimitiveVT>(new NonNullableSourceNonPrimitiveVT { ID = 10 });

      Assert.AreEqual(10, result.ID.Value);

    }

    [TestMethod]
    public void NullableWithNonNullValueGetsMappedToNonNullableNonPrimitiveVT()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSourceNonPrimitiveVT, NonNullableDestinationNonPrimitiveVT>(new NullableSourceNonPrimitiveVT { ID = 10 });

      Assert.AreEqual(10, result.ID);

    }

    [TestMethod]
    public void NullableWithNullValueGetsMappedToNonNullableNonPrimitiveVT()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSourceNonPrimitiveVT, NonNullableDestinationNonPrimitiveVT>(new NullableSourceNonPrimitiveVT { ID = null });

      Assert.AreEqual(default(int), result.ID);

    }

    [TestMethod]
    public void NullableToNullableWorksNonPrimitiveVT()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullableSourceNonPrimitiveVT, NullableSourceNonPrimitiveVT>(new NullableSourceNonPrimitiveVT { ID = 10 });

      Assert.AreEqual(10, result.ID);

    }

    public enum FooEnum
    {
      A,
      B
    }

    class EnumSourceType
    {
      public FooEnum Foo { get; set; }
    }

    class EnumDestinationType
    {
      public FooEnum? Foo { get; set; }
    }


    [TestMethod]
    public void NonNullableToNullableWithProjectionsWorksAsExpected()
    {
      var mapper = new MemberMapper();

      var result = mapper.Project<EnumSourceType, EnumDestinationType>().Compile()(new EnumSourceType { Foo = FooEnum.B });

      Assert.AreEqual(FooEnum.B, result.Foo);

    }

    [TestMethod]
    public void NullableToNonNullableWithProjectionsWorksAsExpectedWithSourceNull()
    {
      var mapper = new MemberMapper();

      var result = mapper.Project<EnumDestinationType, EnumSourceType>().Compile()(new EnumDestinationType { Foo = null });

      Assert.AreEqual(default(FooEnum), result.Foo);

    }

    [TestMethod]
    public void NullableToNonNullableWithProjectionsWorksAsExpectedWithSourceNotNull()
    {
      var mapper = new MemberMapper();

      var result = mapper.Project<EnumDestinationType, EnumSourceType>().Compile()(new EnumDestinationType { Foo = FooEnum.B });

      Assert.AreEqual(FooEnum.B, result.Foo);

    }

    class NullabeTimeSpanSource
    {
      public TimeSpan? Foo { get; set; }
    }

    class NullabeTimeSpanDestination
    {
      public TimeSpan? Foo { get; set; }
    }

    class NonNullabeTimeSpanSource
    {
      public TimeSpan Foo { get; set; }
    }

    class NonNullabeTimeSpanDestination
    {
      public TimeSpan Foo { get; set; }
    }

    [TestMethod]
    public void NullableTimeSpanGetsMappedToNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullabeTimeSpanSource, NullabeTimeSpanDestination>(new NullabeTimeSpanSource
        {
          Foo = new TimeSpan(888)
        });

      Assert.AreEqual(888, result.Foo.Value.Ticks);

    }

    [TestMethod]
    public void NonNullableTimeSpanGetsMappedToNonNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NonNullabeTimeSpanSource, NonNullabeTimeSpanDestination>(new NonNullabeTimeSpanSource
      {
        Foo = new TimeSpan(888)
      });

      Assert.AreEqual(888, result.Foo.Ticks);
    }


    [TestMethod]
    public void NonNullableTimeSpanGetsMappedToNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NonNullabeTimeSpanSource, NullabeTimeSpanDestination>(new NonNullabeTimeSpanSource
      {
        Foo = new TimeSpan(888)
      });

      Assert.AreEqual(888, result.Foo.Value.Ticks);
    }

    [TestMethod]
    public void NullableTimeSpanGetsMappedToNonNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullabeTimeSpanSource, NonNullabeTimeSpanDestination>(new NullabeTimeSpanSource
      {
        Foo = new TimeSpan(888)
      });

      Assert.AreEqual(888, result.Foo.Ticks);
    }

    [TestMethod]
    public void NullableTimeSpanWithNullValueGetsMappedToNonNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullabeTimeSpanSource, NonNullabeTimeSpanDestination>(new NullabeTimeSpanSource
      {
        Foo = null
      });

      Assert.AreEqual(new TimeSpan().Ticks, result.Foo.Ticks);
    }

    [TestMethod]
    public void NullableTimeSpanWithNullValueGetsMappedToNullableTimeSpan()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<NullabeTimeSpanSource, NullabeTimeSpanDestination>(new NullabeTimeSpanSource
      {
        Foo = null
      });

      Assert.IsNull(result.Foo);
    }
  }
}