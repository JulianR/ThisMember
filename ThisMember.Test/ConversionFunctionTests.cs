using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;

namespace ThisMember.Test
{
  [TestClass]
  public class ConversionFunctionTests
  {

    class Source
    {
      public int Foo { get; set; }
      
    }

    class Destination
    {
      public int Foo { get; set; }
    }

    class SourceDate
    {
      public DateTime Start { get; set; }
    }

    class DestinationDate
    {
      public DateTime Start { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConversionFunctionIsValidatedOnReturnType()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, Destination>(options: (src, dest, options, depth) =>
      {
        //new Expression<Func<int, int>>();
        Expression<Func<int, string>> func = s => s + "X";
        options.Convert(func);
      });

    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConversionFunctionIsValidatedOnArgumentType()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, Destination>(options: (src, dest, options, depth) =>
      {
        Expression<Func<string, int>> func = (s => s.Length);
        options.Convert(func);
      });

    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConversionFunctionIsValidatedOnArgumentCount()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, Destination>(options: (src, dest, options, depth) =>
      {
        Expression<Func<string, string, string, int>> func = ((s, y, z) => s.Length);
        options.Convert(func);
      });

    }

    [TestMethod]
    public void ConversionFunctionWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, Destination>(options: (src, dest, options, depth) =>
      {
        options.Convert<int, int>(s => s * 2);
      });

      var result = mapper.Map(new Source { Foo = 2 }, new Destination());

      Assert.AreEqual(4, result.Foo);

    }

    [TestMethod]
    public void ConversionFunctionWithConditionWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<Source, Destination>(options: (src, dest, options, depth) =>
      {
        options.Convert<int, int>(s => s * 2);
      })
      .ForMember(s => s.Foo)
      .OnlyIf(s => s.Foo == 3)
      .FinalizeMap();

      var result = mapper.Map(new Source { Foo = 2 }, new Destination());

      Assert.AreEqual(0, result.Foo);

      result = mapper.Map(new Source { Foo = 3 }, new Destination());

      Assert.AreEqual(6, result.Foo);

    }

    [TestMethod]
    public void ConversionFunctionWithConditionAndCustomMappingWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<Source, Destination>(
      customMapping: src => new Destination
      {
        Foo = src.Foo * 3
      },
      options: (src, dest, options, depth) =>
      {
        options.Convert<int, int>(s => s * 2);
      })
      .ForMember(s => s.Foo)
      .OnlyIf(s => s.Foo == 3)
      .FinalizeMap();

      var result = mapper.Map(new Source { Foo = 2 }, new Destination());

      Assert.AreEqual(0, result.Foo);

      result = mapper.Map(new Source { Foo = 3 }, new Destination());

      Assert.AreEqual(18, result.Foo);

    }

    [TestMethod]
    public void ConversionFunctionAndCustomMappingWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Source, Destination>(
      
      customMapping: src => new Destination
      {
        Foo = src.Foo * 3
      },

      options: (src, dest, options, depth) =>
      {
        options.Convert<int, int>(s => s * 2);
      });

      var result = mapper.Map(new Source { Foo = 2 }, new Destination());

      Assert.AreEqual(12, result.Foo);

    }

    [TestMethod]
    public void ConversionToUtcTimeWorks()
    {
      var mapper = new MemberMapper();

      MappingOptions func = new MappingOptions((src, dest, options, depth) =>
      {
        if (src.DeclaringType == typeof(SourceDate))
        {
          options.Convert<DateTime, DateTime>(d => d.ToUniversalTime());
        }
        else
        {
          options.Convert<DateTime, DateTime>(d => d.ToLocalTime());
        }
      });

      mapper.CreateMap<SourceDate, DestinationDate>(options: func);
      mapper.CreateMap<DestinationDate, SourceDate>(options: func);

      var result = mapper.Map(new SourceDate { Start = DateTime.Now }, new DestinationDate());

      Assert.AreEqual(DateTimeKind.Utc, result.Start.Kind);

      var reverse = mapper.Map(new DestinationDate { Start = DateTime.Now }, new SourceDate());

      Assert.AreEqual(DateTimeKind.Local, reverse.Start.Kind);

    }

    [TestMethod]
    public void ConversionToUtcTimeWithCustomMappingWorks()
    {
      var mapper = new MemberMapper();

      MappingOptions func = new MappingOptions((src, dest, options, depth) =>
      {
        if (src.DeclaringType == typeof(SourceDate))
        {
          options.Convert<DateTime, DateTime>(d => d.ToUniversalTime());
        }
        else
        {
          options.Convert<DateTime, DateTime>(d => d.ToLocalTime());
        }
      });

      mapper.CreateMap<SourceDate, DestinationDate>(options: func, 
      customMapping: src => new DestinationDate
      {
        Start = new DateTime(2001, 12, 1, 10, 0, 0, DateTimeKind.Local)
      });

      mapper.CreateMap<DestinationDate, SourceDate>(options: func);

      var result = mapper.Map(new SourceDate { Start = DateTime.Now }, new DestinationDate());

      Assert.AreEqual(2001, result.Start.Year);
      Assert.AreEqual(DateTimeKind.Utc, result.Start.Kind);

      var reverse = mapper.Map(new DestinationDate { Start = DateTime.Now }, new SourceDate());

      Assert.AreEqual(DateTimeKind.Local, reverse.Start.Kind);

    }

    interface IDataModel
    {
    }

    class DtoBase
    {
    }

    class Customer : IDataModel
    {
      public DateTime CreationTime { get; set; }
    }

    class CustomerDto : DtoBase
    {
      public DateTime CreationTime { get; set; }
    }

    [TestMethod]
    public void ConversionIsCarriedOverToSubtypes()
    {
      var mapper = new MemberMapper();

      MappingOptions func = new MappingOptions((src, dest, options, depth) =>
      {
        if (typeof(IDataModel).IsAssignableFrom(src.DeclaringType)  && typeof(DtoBase).IsAssignableFrom(dest.DeclaringType))
        {
          options.Convert<DateTime, DateTime>(d => d.ToUniversalTime());
        }
      });

      mapper.CreateMap<Customer, CustomerDto>(options: func);

      var result = mapper.Map<Customer, CustomerDto>(new Customer
      {
        CreationTime = DateTime.Now
      });

      Assert.AreEqual(DateTimeKind.Utc, result.CreationTime.Kind);


    }

  }
}
