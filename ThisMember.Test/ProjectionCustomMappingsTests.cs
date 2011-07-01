
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ProjectionCustomMappingsTests
  {

    class SourceType
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }
    }

    class DestinationType
    {
      public string FullName { get; set; }
    }

    [TestMethod]
    public void CustomMappingsWorkWithProjections()
    {
      var mapper = new MemberMapper();

      mapper.CreateMap<SourceType, DestinationType>(src => new DestinationType
      {
        FullName = src.FirstName + " " + src.LastName
      });

      var projection = mapper.Project<SourceType, DestinationType>().Compile();

      var result = projection(new SourceType { FirstName = "First", LastName = "Last" });

      Assert.AreEqual("First Last", result.FullName);

    }
  }
}
