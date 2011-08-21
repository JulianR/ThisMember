using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class RecursiveMapTests
  {

    public class SourceType
    {
      public int ID { get; set; }
      public IList<SourceType> Children { get; set; }
    }

    public class DestinationType
    {
      public int ID { get; set; }
      public IList<DestinationType> Children { get; set; }
    }

    [TestMethod]
    public void RecursiveRelationshipsAreMappedCorrectly()
    {
      var map = new MemberMapper();

      var source = new SourceType
      {
        ID = 10,
        Children = new List<SourceType>
        {
          new SourceType
          {
            ID = 11,
          },
          new SourceType
          {
            ID = 12,
            Children = new List<SourceType>
            {
              new SourceType 
              {
                ID = 13
              }
            }
          }

        }
      };

      var result = map.Map<SourceType, DestinationType>(source);

    }

    class ClassA
    {
      public ClassB FooB { get; set; }
    }

    class ClassB
    {
      public ClassC FooC { get; set; }
    }

    class ClassC
    {
      public ClassA FooA { get; set; }
    }

    [TestMethod]
    public void TypeMappingsAreNotCachedWhenRecursionIsDetected()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.MaxCloneDepth = null;

      mapper.CreateMap<ClassA, ClassA>();

      mapper.CreateMap<ClassB, ClassB>();

      var result = mapper.Map<ClassB, ClassB>(new ClassB
      {
        FooC = new ClassC
        {
          FooA = new ClassA
          {

          }
        }
      });

      // The recursion in the map of ClassA should not affect the map of ClassB if ClassB has no recursion
      Assert.IsNotNull(result);
      Assert.IsNotNull(result.FooC);
      Assert.IsNotNull(result.FooC.FooA);

      var resultOther = mapper.Map<ClassA, ClassA>(new ClassA
      {
        FooB = new ClassB
        {
          FooC = new ClassC
          {
            FooA = new ClassA
            {

            }
          }
        }
      });

      // ClassA should still map the FooA property as null, otherwise it would mean a stackoverflow
      Assert.IsNotNull(resultOther);
      Assert.IsNotNull(resultOther.FooB);
      Assert.IsNotNull(resultOther.FooB.FooC);
      Assert.IsNull(resultOther.FooB.FooC.FooA);
    }

    [TestMethod]
    public void TypeMappingsAreNotCachedWhenRecursionDepthIsLimited()
    {
      var mapper = new MemberMapper();

      mapper.Options.Conventions.MaxCloneDepth = 2;

      mapper.CreateMap<ClassA, ClassA>();

      mapper.CreateMap<ClassB, ClassB>();

      var result = mapper.Map<ClassB, ClassB>(new ClassB
      {
        FooC = new ClassC
        {
          FooA = new ClassA
          {

          }
        }
      });

      Assert.IsNotNull(result);
      Assert.IsNotNull(result.FooC);
      Assert.IsNotNull(result.FooC.FooA);

      var resultOther = mapper.Map<ClassA, ClassA>(new ClassA
      {
        FooB = new ClassB
        {
          FooC = new ClassC
          {
            FooA = new ClassA
            {

            }
          }
        }
      });

      Assert.IsNotNull(resultOther);
      Assert.IsNotNull(resultOther.FooB);
      Assert.IsNotNull(resultOther.FooB.FooC);
      Assert.IsNull(resultOther.FooB.FooC.FooA);
    }

  }
}
