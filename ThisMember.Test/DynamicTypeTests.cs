using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class DynamicTypeTests
  {
    class StaticType
    {
      public string Test { get; set; }
    }

    [TestMethod]
    public void ExpandoObjectAsSourceWorks()
    {
      //dynamic expando = new ExpandoObject();

      //expando.Name = "Test";

      //var mapper = new MemberMapper();

      //var result = mapper.MapFromDynamic(expando, new StaticType());

      //Assert.AreEqual("Test", result.Test);

    }
  }
}
