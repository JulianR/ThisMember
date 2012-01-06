using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class CustomMappingTests
  {

    private class Address
    {
      public string Street { get; set; }
      public string HouseNumber { get; set; }
      public string ZipCode { get; set; }
      public string City { get; set; }
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
    public void CustomMappingIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Customer, SimpleCustomerDto>(customMapping: c => new
      {
        FullName = c.FirstName + " " + c.LastName.ToString(),
        AddressLine = c.Address.Street + " " + c.Address.HouseNumber,
        EmailAddress = c.EmailAddress.Address
      });

      var customer = new Customer
      {
        FirstName = "Test",
        LastName = "Test",
        EmailAddress = new EmailAddress
        {
          Address = "test@test.com"
        },
        CustomerID = 1,
        Address = new Address
        {
          Street = "test",
          HouseNumber = "10",
          ZipCode = "1111AB",
          City = "test"
        }
      };

      var dto = mapper.Map<Customer, SimpleCustomerDto>(customer);

    }

    [TestMethod]
    public void CustomMappingAddedLaterIsRespected()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>()
        .ForMember(c => c.AddressLine).MapAs(c => c.Address.Street + " " + c.Address.HouseNumber)
        .FinalizeMap();

      var source = new Customer
      {
        Address = new Address
        {
          Street = "Street",
          HouseNumber = "12"
        }
      };

      var result = mapper.Map<Customer, SimpleCustomerDto>(source);

      Assert.AreEqual("Street 12", result.AddressLine);

    }

    [TestMethod]
    public void NonMatchingMembesAreMappedWhenCustomMappingIsPresent()
    {
      var mapper = new MemberMapper();

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>(customMapping: src =>
      new
      {
        AddressLine = src.Address.City
      }).FinalizeMap();

      var customer = new Customer
      {
        Address = new Address
        {
          City = "test"
        }
      };

      var dto = mapper.Map<Customer, SimpleCustomerDto>(customer);

      Assert.AreEqual("test", dto.AddressLine);

    }

    public class NestedSourceType
    {
      public string Test { get; set; }
      public int ID { get; set; }
    }

    public class SourceType
    {
      public NestedSourceType Nested { get; set; }
    }

    public class NestedDestinationType
    {
      public string Testing { get; set; }
    }

    public class DestinationType
    {
      public NestedDestinationType Nested { get; set; }
    }

    [TestMethod]
    public void CustomMappingsAreReused()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<NestedSourceType, NestedDestinationType>(customMapping: src => new NestedDestinationType
      {
        Testing = src.Test + " " + src.ID
      });

      mapper.CreateMap<SourceType, DestinationType>();

      var source = new SourceType { Nested = new NestedSourceType { Test = "test", ID = 10 } };

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreEqual("test 10", result.Nested.Testing);

    }

    class ComplexSourceTypeNested
    {
      public string Name { get; set; }
    }

    class ComplexSourceType
    {
      public ComplexSourceTypeNested Foo { get;set;}
    }

    class SimpleDestinationType
    {
      public string Foo { get; set; }
    }

    [TestMethod]
    public void ComplexMappingIsUsedWhenDestinationPropertyIsString()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<ComplexSourceType, SimpleDestinationType>(customMapping: src => new SimpleDestinationType
      {
         Foo = src.Foo.Name
      }); 

      var result = mapper.Map<ComplexSourceType, SimpleDestinationType>(new ComplexSourceType { Foo = new ComplexSourceTypeNested { Name = "Test" } });

      Assert.AreEqual("Test", result.Foo); 

    }

    class SimpleSourceType
    {
      public string Foo { get; set; }
    }

    [TestMethod]
    public void ComplexMappingTakesPrecedenceOverNormalMapping()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SimpleSourceType, SimpleDestinationType>(customMapping: src => new SimpleDestinationType
      {
        Foo = "Foo"
      });

      var result = mapper.Map<SimpleSourceType, SimpleDestinationType>(new SimpleSourceType { Foo = "Bar" });

      Assert.AreEqual("Foo", result.Foo);

    }

    [TestMethod]
    public void CustomMappingWithManualNullCheckIsHandledCorrectly()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceNullCheck, DestinationNullCheck>(s => new DestinationNullCheck
      {
        Oof = s.Foo != null ? s.Foo.Bar : string.Empty
      });

      var result = mapper.Map(new SourceNullCheck { Foo = new SourceNullCheckNested { Bar = "test" } }, new DestinationNullCheck());

      Assert.AreEqual("test", result.Oof);

      result = mapper.Map(new SourceNullCheck { Foo = null }, new DestinationNullCheck());

      Assert.AreEqual("", result.Oof);
    }

    [TestMethod]
    public void CustomMappingIsRespectedByCollectionMapping()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SimpleSourceType, SimpleDestinationType>(customMapping: src => new SimpleDestinationType
      {
        Foo = "Foo"
      });

      var result = mapper.Map(new List<SimpleSourceType>
      {
        new SimpleSourceType
        {
          Foo = "abc",
        },
        new SimpleSourceType
        {
          Foo = "def",
        }
      }, new List<SimpleDestinationType>());

      Assert.IsTrue(result.All(r => r.Foo == "Foo"));

    }

    class SourceNullCheck
    {
      public SourceNullCheckNested Foo { get; set; }
    }

    class SourceNullCheckNested
    {
      public string Bar { get; set; }
    }

    class DestinationNullCheck
    {
      public string Oof { get; set; }
    }

   
    class SourceConstructor
    {
      public string Foo { get; set; }
    }

    class DestinationConstructorNested
    {
      public string Foobar { get; set; }
      public string Test { get; set; }
    }

    class DestinationConstructor
    {
      public DestinationConstructorNested Nested { get; set; }
    }

    [TestMethod]
    public void NewExpressionCanBeUsedAsMapping()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceConstructor, DestinationConstructor>(src => new DestinationConstructor
      {
        Nested = new DestinationConstructorNested
        {
          Foobar = src.Foo + "x",
          Test = src.Foo + src.Foo
        }
      });

      var result = mapper.Map(new SourceConstructor
        {
          Foo = "12"
        }, new DestinationConstructor());

      Assert.AreEqual("12x", result.Nested.Foobar);
      Assert.AreEqual("1212", result.Nested.Test);

    }

  }
}
