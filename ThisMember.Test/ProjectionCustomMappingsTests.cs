
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ProjectionCustomMappingsTests
  {

    class SourceType
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }
    }

    class DestinationType
    {
      public string FullName { get; set; }
    }

    [TestMethod]
    public void CustomMappingsWorkWithProjections()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>(src => new DestinationType
      {
        FullName = src.FirstName + " " + src.LastName
      });

      var projection = mapper.Project<SourceType, DestinationType>().Compile();

      var result = projection(new SourceType { FirstName = "First", LastName = "Last" });

      Assert.AreEqual("First Last", result.FullName);

    }


    [TestMethod]
    public void ProjectionsAffectMaps()
    {
      var mapper = new MemberMapper();

      mapper.CreateProjection<SourceType, DestinationType>(src => new DestinationType
      {
        FullName = src.FirstName + " " + src.LastName
      });

      var projection = mapper.Project<SourceType, DestinationType>().Compile();

      var source = new SourceType { FirstName = "First", LastName = "Last" };

      var result = projection(source);

      Assert.AreEqual("First Last", result.FullName);

      result = mapper.Map<SourceType, DestinationType>(source);

      Assert.AreEqual("First Last", result.FullName);



    }


    class Customer
    {
      public IList<Address> Addresses { get; set; }
    }

    class Address
    {
      public Country Country { get; set; }
    }

    class Country
    {
      public string Name { get; set; }
    }

    class CustomerDto
    {
      public IList<AddressDto> Addresses { get; set; }
    }

    class AddressDto
    {
      public string CountryName { get; set; }
    }

    [TestMethod]
    public void NestedCollectionWithCustomMappingWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Address, AddressDto>(src => new AddressDto
      {
        CountryName = src.Country.Name
      });

      var projection = mapper.Project<Customer, CustomerDto>();

      var method = projection.Compile();

      var result = method(new Customer
      {
        Addresses = new List<Address>
        {
          new Address
          {
            Country = new Country
            {
              Name = "X"
            }
          }
        }
      });

      Assert.AreEqual("X", result.Addresses.Single().CountryName);

    }

    class CustomerSingleAddress
    {
      public Address Address { get; set; }
    }

    class CustomerSingleAddressDto
    {
      public AddressDto Address { get; set; }
    }

    [TestMethod]
    public void NestedPropertyWithCustomMappingWorks()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<Address, AddressDto>(src => new AddressDto
      {
        CountryName = src.Country.Name
      });

      var projection = mapper.Project<CustomerSingleAddress, CustomerSingleAddressDto>();

      var method = projection.Compile();

      var result = method(new CustomerSingleAddress
      {
        Address =
          new Address
          {
            Country = new Country
            {
              Name = "X"
            }
          }
      });

      Assert.AreEqual("X", result.Address.CountryName);

    }
  }
}
