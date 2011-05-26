using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class SourceNullTests
  {

    public class SourceType
    {
      public string Name { get; set; }
    }

    public class DestinationType
    {
      public string Name { get; set; }
    }

    [TestMethod]
    public void Option_ReturnNullWhenSourceIsNull_Works()
    {
      SourceType source = null;

      var mapper = new MemberMapper();
      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.ReturnNullWhenSourceIsNull;

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNull(result);
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void Option_AllowNullReferenceExceptionWhenSourceIsNull_Works()
    {
      SourceType source = null;

      var mapper = new MemberMapper();
      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.AllowNullReferenceExceptionWhenSourceIsNull;

      mapper.Map<SourceType, DestinationType>(source);
    }

    [TestMethod]
    public void Option_ReturnDestinationObject_Works()
    {
      SourceType source = null;

      var mapper = new MemberMapper();
      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.ReturnDestinationObject;

      var result = mapper.Map<SourceType, DestinationType>(source);

      Assert.IsNotNull(result);
    }
  }
}
