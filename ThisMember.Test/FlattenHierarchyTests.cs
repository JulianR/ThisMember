using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class FlattenHierarchyTests
  {
    class CompanyContainer
    {
      public Company Company { get; set; }
    }

    class Company
    {
      public int ID { get; set; }
      public string Name
      {
        set
        {

        }
      }
    }

    class CompanyDto
    {
      public int CompanyID { get; set; }
      public string CompanyName { get; set; }
    }

    class MethodContainer
    {
      public MethodSource Method { get; set; }
    }

    class MethodSource
    {
      public int ID()
      {
        return 1;
      }
    }

    class MethodDto
    {
      public int MethodID { get; set; }
    }

    [TestMethod]
    public void FlattenHierarchyWorks()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new CompanyContainer { Company = new Company { ID = 10 } }, new CompanyDto());

      Assert.AreEqual(10, result.CompanyID);
    }

    [TestMethod]
    public void FlattenHierarchyWorksWithNullValues()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new CompanyContainer { Company = null }, new CompanyDto());

      Assert.AreEqual(0, result.CompanyID);
    }

    [TestMethod]
    public void FlattenHierarchyWorksWithComplexType()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new CompanyContainer { Company = new Company { ID = 10 } }, new CompanyDto());

      Assert.AreEqual(10, result.CompanyID);
    }

    [TestMethod]
    public void FlattenHierarchyOnlyUsesAvailableGetters()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new CompanyContainer { Company = new Company { ID = 10, Name = "Test" } }, new CompanyDto());

      Assert.AreEqual(10, result.CompanyID);
      Assert.AreEqual(null, result.CompanyName);
    }

    [TestMethod]
    public void FlattenHierarchyIgnoresMethods()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new MethodContainer { Method = new MethodSource() }, new MethodDto());

      Assert.AreEqual(0, result.MethodID);
    }

    class Layer0
    {
      public Layer1 One { get; set; }
    }

    class Layer1
    {
      public Layer2 Two { get; set; }
    }

    class Layer2
    {
      public string Value { get; set; }
    }

    class LayerDestination
    {
      public string OneTwoValue { get; set; }
    }
    [TestMethod]
    public void FlattenHierarchyWorksForDeeperHierarchies()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new Layer0
        {
          One = new Layer1
          {
            Two = new Layer2
            {
              Value = "Test"
            }
          }
        }, new LayerDestination());

      Assert.AreEqual("Test", result.OneTwoValue);
    }

    [TestMethod]
    public void FlattenHierarchyWorksForDeeperHierarchiesAndNullValues_1()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new Layer0
      {
        One = null
      }, new LayerDestination());

      Assert.IsNull(result.OneTwoValue);
    }

    [TestMethod]
    public void FlattenHierarchyWorksForDeeperHierarchiesAndNullValues_2()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new Layer0
      {
        One = new Layer1
        {
          Two = null
        }
      }, new LayerDestination());

      Assert.IsNull(result.OneTwoValue);
    }

    class IncompatibleLayer0
    {
      public IncompatibleLayer1 One { get; set; }
    }

    class IncompatibleLayer1
    {
      public Guid Two { get; set; }
    }

    class IncompatibleLayerDestination
    {
      public string OneTwo { get; set; }
    }

    [TestMethod]
    public void FlattenHierarchyIsntUsedForIncompatibleTypes()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new IncompatibleLayer0
      {
        One = new IncompatibleLayer1
        {
          Two = Guid.NewGuid()
        }
      }, new IncompatibleLayerDestination());

      Assert.IsNull(result.OneTwo);
    }

    class User
    {
      public string FirstName { get; set; }
      public string HasBeenDeleted { get; set; }
    }

    class UserDto
    {
    }

    class Test
    {
      public User User { get; set; }
      public User CreatedByUser { get; set; }
    }

    class TestDto
    {
      public string UserFirstName { get; set; }
      public int UserFirstNameLength { get; set; }
      public string CreatedByUserHasBeenDeleted { get; set; }
      public int CreatedByUserHasBeenDeletedLength { get; set; }
      public string CreatedbyUserHasBeenDeletedPartial { get; set; }
      public string CreatedbyUserHasBeenDeletedPartialMatch { get; set; }
    }

    [TestMethod]
    public void UserDtoTest()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map(new[] 
      { 
        new Test
        {
          User = new User
          {
            FirstName = "test"
          }
        }
      }, new List<TestDto>());

      Assert.AreEqual("test", result[0].UserFirstName);
      Assert.AreEqual(4, result[0].UserFirstNameLength);
    }

    [TestMethod]
    public void FlattenHierarchyWorksForArbitraryCamelCasingDepth()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map
      (
        new[] 
        { 
          new Test
          {
            CreatedByUser = new User { HasBeenDeleted = "True" }
          }
        }, new List<TestDto>()
      );

      Assert.AreEqual("True", result[0].CreatedByUserHasBeenDeleted);
      Assert.AreEqual(4, result[0].CreatedByUserHasBeenDeletedLength);
    }

    [TestMethod]
    public void PartialMatchesAreNotMapped()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map
      (
        new[] 
        { 
          new Test
          {
            CreatedByUser = new User { HasBeenDeleted = "True" }
          }
        }, new List<TestDto>()
      );

      Assert.IsNull(result[0].CreatedbyUserHasBeenDeletedPartial);
      Assert.IsNull(result[0].CreatedbyUserHasBeenDeletedPartialMatch);
    }


    public class User1
    {
      public string FullName { get; set; }
    }

    public class UserDto1
    {
      public string FullName { get; set; }
    }

    public class InitialCycleCount
    {
      public User1 PreCountedBy { get; set; }
    }

    public class InitialCycleCountDto
    {
      public string PreCountedByFullName { get; set; }
    }

    [TestMethod]
    public void RealWorldExample()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map
      (
          new InitialCycleCount
          {
            PreCountedBy = new User1 { FullName = "Julian Rooze" }
          }, new InitialCycleCountDto()
      );

      Assert.AreEqual("Julian Rooze", result.PreCountedByFullName);
      Assert.AreEqual("Julian Rooze", result.PreCountedByFullName);
    }

    public class NonNullableSource
    {
      public class NonNullableNested
      {
        public int ID { get; set; }
      }

      public NonNullableNested Test { get; set; }
    }

    public class NullableDestination
    {
      public int? TestID { get; set; }
    }

    [TestMethod]
    public void Assigning_non_nullable_source_to_nullable_destination_should_not_throw()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map
      (
        new NonNullableSource { Test = new NonNullableSource.NonNullableNested { ID = 1 } },
        new NullableDestination()
      );

      Assert.AreEqual(1, result.TestID);
    }

    public class IncompatibleConversionSource
    {
      public IncompatibleConversionSourceNested Test { get; set; }
      public class IncompatibleConversionSourceNested
      {
        public string ID { get; set; }
      }
    }

    public class IncompatibleConversionDestination
    {
      public int TestID { get; set; }
    }

    [TestMethod]
    public void When_a_hierarchy_match_is_found_but_a_conversion_is_not_possible_no_mapping_should_be_attempted()
    {
      var mapper = new MemberMapper();

      var result = mapper.Map
      (
        new IncompatibleConversionSource { Test = new IncompatibleConversionSource.IncompatibleConversionSourceNested { ID = "12" } },
        new IncompatibleConversionDestination()
      );

      Assert.AreEqual(0, result.TestID);
    }
  }
}
