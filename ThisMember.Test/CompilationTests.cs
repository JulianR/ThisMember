using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{

  public class GenericSourceClass<T>
  {
    public T ID { get; set; }
  }

  public class GenericDestinationClass<T>
  {
    public T ID { get; set; }
  }

  public class NonGenericSourceClass
  {
    public GenericSourceClass<int> ID { get; set; }
  }

  public class NonGenericDestinationClass
  {
    public GenericDestinationClass<int> ID { get; set; }
  }

  [TestClass]
  public class CompilationTests
  {
    [TestMethod]
    public void GenericTypeMappingDoesNotThrow()
    {
      var source = new GenericSourceClass<int>();

      var mapper = new MemberMapper();

      var result = mapper.Map<GenericSourceClass<int>, GenericDestinationClass<int>>(source);
    }

    [TestMethod]
    public void NonGenericTypeWithGenericMemberMappingDoesNotThrow()
    {
      var source = new NonGenericSourceClass();

      var mapper = new MemberMapper();

      var result = mapper.Map<NonGenericSourceClass, NonGenericDestinationClass>(source);
    }

  }
}
