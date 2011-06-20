using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;

namespace ThisMember.Samples
{
  public class SimpleScenarios
  {

    public class Order
    {
      public int ID { get;set;}
      public decimal Amount { get;set;}
    }

    public class Customer
    {
      public int ID { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public IList<Order> Orders { get; set; }
    }

    public class OrderDto
    {
      public int ID { get;set;}
      public decimal Amount { get;set;}
    }

    public class CustomerDto
    {
      public int ID { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public IList<OrderDto> Orders { get; set; }
    }

    public static void DirectMapping()
    {
      var customer = new Customer
      {
        ID = 1,
        FirstName = "First",
        LastName = "Last",
        Orders = new List<Order>
        {
          new Order
          {
            ID = 1,
            Amount = 10,
          },
          new Order
          {
            ID = 2,
            Amount = 20
          }
        }
      };

      var mapper = new MemberMapper();

      // Explicit source and destination type, just pass a source type instance
      var dto = mapper.Map<Customer, CustomerDto>(customer);

      // Just specify what ThisMember should map to, pass in any type that you think can be mapped
      dto = mapper.Map<CustomerDto>(customer);

      dto = new CustomerDto
      {
        FirstName = "NewName"
      };

      mapper.Map(dto, customer);

      Console.WriteLine(customer.LastName);

    }

  }
}
