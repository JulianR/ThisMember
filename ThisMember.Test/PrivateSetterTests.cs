using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core.Exceptions;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class PrivateSetterTests
  {

    class SourceType
    {
      public int Foo { get; set; }
    }

    class DestinationType
    {
      public int Foo { get; private set; }
    }

    [TestMethod]
    [ExpectedException(typeof(IncompatibleMappingException))]
    public void PrivateSetterDoesNotCauseUnexpectedException()
    {
      var mapper = new MemberMapper();
      mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember = true;
      var result = mapper.Map<SourceType, DestinationType>(new SourceType { Foo = 1 });
    }
  }
}
