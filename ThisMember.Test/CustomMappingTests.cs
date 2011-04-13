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
      public string Address { get;set;}
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

      mapper.CreateMapProposal<Customer, SimpleCustomerDto>(customMapping: c =>
      new
      {
        FullName = c.FirstName + " " + c.LastName.ToString(),
        AddressLine = c.Address.Street + " " + c.Address.HouseNumber,
        EmailAddress = c.EmailAddress.Address
      }).FinalizeMap();

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
  }
}
