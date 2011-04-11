using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;
using System.Diagnostics;

namespace ThisMember.Benchmarks
{
  public static class Comparisons
  {

    public class Order
    {
      public decimal Amount { get; set; }
    }

    public class Customer
    {
      public int CustomerID { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public IList<Order> Orders { get; set; }
    }

    public class CustomerDto
    {
      public int CustomerID { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string FullName { get; set; }
      public int OrderCount { get; set; }
      public decimal OrderAmount { get; set; }
    }

    public static CustomerDto Dto;

    public static void Benchmark()
    {
      var mapper = new MemberMapper();

      var map = mapper.CreateAndFinalizeMap<Customer, CustomerDto>(customMapping: src => new
      {
        FullName = src.FirstName + " " + src.LastName,
        OrderCount = src.Orders.Count,
        OrderAmount = src.Orders.Sum(o => o.Amount)
      });

      var mappingFunc = map.MappingFunction;

      var customer = new Customer
      {
        CustomerID = 1,
        FirstName = "John",
        LastName = "Doe",
        Orders = new List<Order>
        {
          new Order
          {
            Amount = 100m
          },
          new Order
          {
            Amount = 15m
          },
          new Order
          {
            Amount = 150m
          }
        }
      };

      var sw = Stopwatch.StartNew();

      for (var i = 0; i < 1000000; i++)
      {
        Dto = new CustomerDto();

        Dto.FirstName = customer.FirstName;
        Dto.LastName = customer.LastName;
        Dto.FullName = customer.FirstName + " " + customer.LastName;
        Dto.OrderCount = customer.Orders.Count;
        Dto.OrderAmount = customer.Orders.Sum(o => o.Amount);
      }

      sw.Stop();

      Console.WriteLine("Manual: " + sw.Elapsed);

      sw.Restart();

      for (var i = 0; i < 1000000; i++)
      {
        Dto = mapper.Map<Customer, CustomerDto>(customer);
      }

      sw.Stop();

      Console.WriteLine("ThisMember slow: " + sw.Elapsed);

      sw.Restart();

      for (var i = 0; i < 1000000; i++)
      {
        Dto = mappingFunc(customer, new CustomerDto());
      }

      sw.Stop();

      Console.WriteLine("ThisMember fast: " + sw.Elapsed);

    }
  }
}
