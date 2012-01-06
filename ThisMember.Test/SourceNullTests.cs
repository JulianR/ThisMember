using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Options;

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

    [TestMethod]
    public void DynamicInvokeWithSourceNullReturnsNull()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.ReturnNullWhenSourceIsNull;

      SourceType type = null;

      var result = mapper.Map<DestinationType>(type);

      Assert.IsNull(result);

    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void DynamicInvokeWithSourceNullThrows()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.AllowNullReferenceExceptionWhenSourceIsNull;

      SourceType type = null;

      var result = mapper.Map<DestinationType>(type);

    }

    [TestMethod]
    public void DynamicInvokeWithSourceNullReturnsDestination()
    {
      var mapper = new MemberMapper();

      mapper.Options.Safety.IfSourceIsNull = SourceObjectNullOptions.ReturnDestinationObject;

      SourceType type = null;

      var result = mapper.Map<DestinationType>(type);

      Assert.IsNotNull(result);
    }
  }
}
