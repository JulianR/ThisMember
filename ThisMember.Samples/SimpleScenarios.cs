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

    public static void MapCreation()
    {
      // There's a variety of ways to create a map. First, we need an IMemberMapper, which unless you write your own, is MemberMapper:
      var mapper = new MemberMapper();

      // The first way is implicit. If there's no map defined yet, calling Map will create it and ThisMember will do its best to come up
      // with a map for the entire object graph.
      var customer = mapper.Map<CustomerDto, Customer>(new CustomerDto());

      // The second way is explicit, which allows you to modify the map
      mapper.CreateMap<CustomerDto, Customer>(source => new Customer
      {
        FirstName = source.FirstName.ToLower()
      });

      // The third way is more explicit, allowing you to modify the map in several steps until you 'finalize' it.
      mapper.CreateMapProposal<CustomerDto, Customer>(source => new Customer
      {
        LastName = source.FirstName
      })
      .ForMember(c => c.ID)
      .OnlyIf(c => c.ID > 0)
      .FinalizeMap();
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

      // Update the existing Customer, just set a new FirstName for him
      dto = new CustomerDto
      {
        FirstName = "NewName"
      };

      // For that we have to set an option that null-values from the source are ignored, so that LastName does not get set to null
      mapper.Options.Conventions.IgnoreMembersWithNullValueOnSource = true;

      // Setting an option that affects map generation has no effect on maps that have already been generated. Normally there'd be little
      // need to set this option on the fly, you would just have a seperate mapper for doing these mappings. But for this sample, it works fine.
      mapper.ClearMapCache(); // We could also have called mapper.CreateMap<CustomerDto, Customer>(), which always recreates the map.

      // Only FirstName will have changed now on customer, because the null (or null-equivalent in case of nullable value types) values were ignored.
      mapper.Map(dto, customer);
    }

    public class User
    {
      public string Username { get; set; }
      [IgnoreMember]
      public string Password { get; set; }
    }

    public class UserDto
    {
      public string Username { get; set; }
      public string Password { get; set; }
    }

    public static void IgnoringMembers()
    {
      // Ignoring members can be done in a variety of ways. In the class above, we've placed the [IgnoreMember] attribute on Password,
      // indicating it should never be mapped.
      var mapper = new MemberMapper();
      // You can set this option to ignore the IgnoreMember attribute:
      mapper.Options.Conventions.IgnoreMemberAttributeShouldBeRespected = false;

      // But we won't
      mapper.Options.Conventions.IgnoreMemberAttributeShouldBeRespected = true;

      var existingUser = new User
      {
        Username = "User",
        Password = "1234"
      };

      mapper.Map<UserDto, User>(new UserDto { Username = "Name", Password = "Secret" }, existingUser);

      // The password of existingUser will be unchanged

      mapper.Options.Conventions.IgnoreMemberAttributeShouldBeRespected = false;

      // Now we'll ignore through the fluent configuration interface
      mapper.CreateMapProposal<UserDto, User>()
        .ForMember(u => u.Password).Ignore()
        .FinalizeMap(); // Maps can be modified until you call this method.

      // Or you could use it like this:

      var map = mapper.CreateMapProposal<UserDto, User>();

      var userCanSetPassword = false;

      if (!userCanSetPassword)
      {
        map.ForMember(u => u.Password).Ignore();
      }

      map.FinalizeMap();

      // The third way:
      mapper.CreateMap<UserDto, User>(options: (source, destination, option) =>
      {
        // You can make this as complicated as you want
        if (destination.Name == "Password")
        {
          option.IgnoreMember();
        }

        // For example, check for the presence of an attribute 
        // that determines if a user has rights to map this property
        var attr = destination
          .PropertyOrFieldType
          .GetCustomAttributes(typeof(MappingRequiresPermissionAttribute), false)
          .FirstOrDefault() as MappingRequiresPermissionAttribute;

        if (attr != null && !attr.HasPermission)
        {
          option.IgnoreMember();
        }

      });

    }

    class MappingRequiresPermissionAttribute : Attribute
    {
      public bool HasPermission
      {
        get
        {
          return false;
        }
      }
    }

  }
}
