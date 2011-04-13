using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Exceptions;
using System.Globalization;

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

    private class Foo
    {
      public string Bar { get; set; }
    }

    private class Address
    {
      public string Street { get; set; }
      public string HouseNumber { get; set; }
      public string ZipCode { get; set; }
      public string City { get; set; }
      public Foo Foo { get; set; }
    }

    private class EmailAddress
    {
      public string Address { get; set; }
    }

    private class Customer
    {
      public int CustomerID { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public Address Address { get; set; }
      public EmailAddress EmailAddress { get; set; }

    }

    private class SimpleCustomerDto
    {
      public string FullName { get; set; }
      public string AddressLine { get; set; }
      public string EmailAddress { get; set; }
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

    [TestMethod]
    public void NavigationPropertiesDoNotThrowWhenNull()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>(customMapping: src =>
      new
      {
        AddressLine = src.Address.Foo.Bar
      }).FinalizeMap();

      var customer = new Customer
      {
      };

      var dto = mapper.Map<Customer, SimpleCustomerDto>(customer);
    }

    [TestMethod]
    public void NavigationPropertiesDoNotThrowWhenNull_2()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Customer, SimpleCustomerDto>(customMapping: src =>
      new
      {
        AddressLine = src.Address.Foo.Bar
      }).FinalizeMap();

      var customer = new Customer
      {
        Address = new Address()
      };

      var dto = mapper.Map<Customer, SimpleCustomerDto>(customer);
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void NavigationPropertiesThrowsWhenNullWithSafetyDisabled()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.PerformNullChecksOnCustomMappings = false;

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>(customMapping: src =>
      new
      {
        AddressLine = src.Address.Foo.Bar
      }).FinalizeMap();

      var customer = new Customer
      {
      };

      var dto = mapper.Map<Customer, SimpleCustomerDto>(customer);
    }

    public class StringSource
    {
      public string Date { get; set; }
    }

    public class DateTimeDestination
    {
      public DateTime Date { get; set; }
    }

    [TestMethod]
    public void StringIsParsedToDateTime()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.DateTime.ParseStringsToDateTime = true;
      mapper.Options.Conventions.DateTime.ParseCulture = new CultureInfo("en-US");

      var result = mapper.Map<StringSource, DateTimeDestination>(new StringSource { Date = "12-31-2001" });

      Assert.AreEqual(new DateTime(2001, 12, 31), result.Date);

    }

    [TestMethod]
    public void StringIsParsedToDateTimeWithDifferentCulture()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.DateTime.ParseStringsToDateTime = true;
      mapper.Options.Conventions.DateTime.ParseCulture = new CultureInfo("nl-NL");

      var result = mapper.Map<StringSource, DateTimeDestination>(new StringSource { Date = "31-12-2001" });

      Assert.AreEqual(new DateTime(2001, 12, 31), result.Date);
    }

    [TestMethod]
    [ExpectedException(typeof(CodeGenerationException))]
    public void ExceptionIsThrownWithMappingToDateTimeTurnedOff()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.DateTime.ParseStringsToDateTime = false;

      var result = mapper.Map<StringSource, DateTimeDestination>(new StringSource { Date = "31-12-2001" });
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void ExceptionIsThrownWithInvalidFormat()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.DateTime.ParseStringsToDateTime = true;

      var result = mapper.Map<StringSource, DateTimeDestination>(new StringSource { Date = "3a1-31-2001abc" });
    }
  }
}
