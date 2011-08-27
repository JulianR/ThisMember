using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ConversionTests
  {

    class IntType
    {
      public int Foo { get; set; } 
    }

    class LongType
    {
      public long Foo { get; set; }
    }

    class DoubleType
    {
      public int Foo { get; set; }
    }

    class FloatType
    {
      public long Foo { get; set; }
    }

    class DecimalType
    {
      public long Foo { get; set; }
    }

    [TestMethod]
    public void IntIsConvertedToLong()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<IntType, LongType>(new IntType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void LongIsConvertedToInt()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<LongType, IntType>(new LongType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void IntIsConvertedToDecimal()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<IntType, DecimalType>(new IntType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void DecimalIsConvertedToInt()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<DecimalType, IntType>(new DecimalType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void IntIsConvertedToDouble()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<IntType, DoubleType>(new IntType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void DoubleIsConvertedToInt()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<DoubleType, IntType>(new DoubleType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void DecimalIsConvertedToDouble()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<DecimalType, DoubleType>(new DecimalType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    [TestMethod]
    public void DoubleIsConvertedToDecimal()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<DoubleType, DecimalType>(new DoubleType
      {
        Foo = 10
      });

      Assert.AreEqual(10, result.Foo);

    }

    class ImplicitlyConvertibleFromType
    {
      public string Foo { get; set; }
      public static implicit operator ImplicitlyConvertibleToType(ImplicitlyConvertibleFromType type)
      {
        return new ImplicitlyConvertibleToType { Bar = type.Foo };
      }
    }

    class ImplicitlyConvertibleToType
    {
      public string Bar { get; set; }
    }

    class ImplicitlyConvertibleType
    {
      public ImplicitlyConvertibleFromType Foo { get; set; }
    }

    class ImplicitlyConvertibleDestinationType
    {
      public ImplicitlyConvertibleToType Foo { get; set; }
    }

    [TestMethod]
    public void UserDefinedTypeIsConvertedImplicitly()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<ImplicitlyConvertibleType, ImplicitlyConvertibleDestinationType>(new ImplicitlyConvertibleType
      {
        Foo = new ImplicitlyConvertibleFromType
        {
          Foo = "x"
        }
      });

      Assert.AreEqual("x", result.Foo.Bar);

    }


    class ExplicitlyConvertibleFromType
    {
      public string Foo { get; set; }
      public static explicit operator ExplicitlyConvertibleToType(ExplicitlyConvertibleFromType type)
      {
        return new ExplicitlyConvertibleToType { Bar = type.Foo };
      }
    }

    class ExplicitlyConvertibleToType
    {
      public string Bar { get; set; }
    }

    class ExplicitlyConvertibleType
    {
      public ExplicitlyConvertibleFromType Foo { get; set; }
    }

    class ExplicitlyConvertibleDestinationType
    {
      public ExplicitlyConvertibleToType Foo { get; set; }
    }

    [TestMethod]
    public void UserDefinedTypeIsConvertedExplicitly()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map<ExplicitlyConvertibleType, ExplicitlyConvertibleDestinationType>(new ExplicitlyConvertibleType
      {
        Foo = new ExplicitlyConvertibleFromType
        {
          Foo = "x"
        }
      });

      Assert.AreEqual("x", result.Foo.Bar);

    }
  }
}
