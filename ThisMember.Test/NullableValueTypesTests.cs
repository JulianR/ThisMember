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

  }
}
