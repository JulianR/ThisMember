﻿using System;
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

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>(customMapping: src =>
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

    private class ReuseSourceType
    {
      public ReuseNestedSourceType Foo { get; set; }
      public string Name { get; set; }
    }

    private class ReuseNestedSourceType
    {
      public int ID { get; set; }
      public string Bar { get; set; }
    }

    private class ReuseDestinationType
    {
      public ReuseNestedDestinationType Foo { get; set; }
      public string Name { get; set; }
    }

    private class ReuseNestedDestinationType
    {
      [IgnoreMember]
      public int ID { get; set; }

      public string Bar { get; set; }
    }

    [TestMethod]
    public void ReuseNonNullMembersIsRespected()
    {
      var mapper = new MemberMapper();
      mapper.Options.Conventions.ReuseNonNullComplexMembersOnDestination = true;

      var source = new ReuseSourceType
      {
        Name = "test",
        Foo = new ReuseNestedSourceType
        {
          ID = 1,
          Bar = "Foo"
        }
      };

      var destination = new ReuseDestinationType
      {
        Foo = new ReuseNestedDestinationType
        {
          ID = 15
        }
      };

      var result = mapper.Map<ReuseSourceType, ReuseDestinationType>(source, destination);

      Assert.AreEqual(15, result.Foo.ID);
      Assert.AreEqual("Foo", result.Foo.Bar);

    }

    class IncompatibleSource
    {
      public int ID { get; set; }
    }

    class IncompatibleDestination
    {
      public int Test { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(IncompatibleMappingException))]
    public void ThrowsWhenTypesAreIncompatible()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<IncompatibleSource, IncompatibleDestination>();

    }

    [TestMethod]
    public void DoesNotThrowWhenTypesAreIncompatibleButPropertyGetsIgnored()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMapProposal<IncompatibleSource, IncompatibleDestination>()
        .ForMember(s => s.Test).Ignore().FinalizeMap();

    }

    class IncompatibleNestedSource
    {
      public int ID { get; set; }
    }

    class IncompatibleSourceWithNested
    {
      public IncompatibleNestedSource Foo { get; set; }
    }

    class IncompatibleNestedDestination
    {
      public int Test { get; set; }
    }

    class IncompatibleDestinationWithNested
    {
      public IncompatibleNestedDestination Foo { get; set; }
    }

    [TestMethod]
    [ExpectedException(typeof(IncompatibleMappingException))]
    public void ThrowsWhenTypesAreIncompatibleOnNestedMember()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMap<IncompatibleSourceWithNested, IncompatibleDestinationWithNested>();

    }

    [TestMethod]
    public void DoesNotThrowWhenTypesAreIncompatibleButPropertyGetsIgnoredOnNestedMember()
    {
      var mapper = new MemberMapper();

      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;

      mapper.CreateMapProposal<IncompatibleSourceWithNested, IncompatibleDestinationWithNested>()
        .ForMember(s => s.Foo.Test).Ignore().FinalizeMap();

    }

    class Product
    {
      public int ProductId { get; set; }
    }

    class DestProduct
    {
      public int ProductID { get; set; }
    }

    [TestMethod]
    public void PropertyNameCasingIsIgnored()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.IgnoreCaseWhenFindingMatch = true;

      var result = mapper.Map<Product, DestProduct>(new Product { ProductId = 10 });

      Assert.AreEqual(10, result.ProductID);

    }

    [TestMethod]
    public void PropertyNameCasingIsNotIgnored()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.IgnoreCaseWhenFindingMatch = false;

      var result = mapper.Map<Product, DestProduct>(new Product { ProductId = 10 });

      Assert.AreNotEqual(10, result.ProductID);

    }

    class SourceObjectComplexType
    {
      public string Name { get; set; }
    }

    class SourceObjectWithDefaultMembers
    {
      public int? ID { get; set; }
      public string Name { get; set; }
      public DateTime? Date { get; set; }
      public SourceObjectComplexType Complex { get; set; }
      public bool? Bool { get; set; }

      public int NonNullableID { get; set;}
      public DateTime NonNullableDate { get; set; }
      public bool NonNullableBool { get; set; }
    }

    class DestObjectComplexType
    {
      public string Name { get; set; }
    }

    class DestObjectWithDefaultMembers
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public DateTime Date { get; set; }
      public DestObjectComplexType Complex { get; set; }
      public bool Bool { get; set; }

      public int NonNullableID { get; set; }
      public DateTime NonNullableDate { get; set; }
      public bool NonNullableBool { get; set; }
    }

    [TestMethod]
    public void DefaultSourceMemberIsIgnored()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.IgnoreMembersWithNullValueOnSource = true;

      var destination = new DestObjectWithDefaultMembers
      {
        ID = 10,
        Name = "Name",
        Date = new DateTime(2000, 1, 1),
        Bool = true,
        Complex = new DestObjectComplexType
        {
          Name = "Name"
        }
      };

      mapper.Map(new SourceObjectWithDefaultMembers(), destination);

      Assert.AreEqual(10, destination.ID);
      Assert.AreEqual("Name", destination.Name);
      Assert.AreEqual(new DateTime(2000, 1, 1), destination.Date);
      Assert.AreEqual("Name", destination.Complex.Name);
      Assert.AreEqual(true, destination.Bool);

      Assert.AreEqual(default(int), destination.NonNullableID);
      Assert.AreEqual(default(DateTime), destination.NonNullableDate);
      Assert.AreEqual(default(bool), destination.NonNullableBool);

      mapper.Map(new SourceObjectWithDefaultMembers { Complex = new SourceObjectComplexType { Name = "Foo" } },
        destination);

      Assert.AreEqual("Foo", destination.Complex.Name);

    }
  }
}
