using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Options;

namespace ThisMember.Test
{
  [TestClass]
  public class MapperOptionsTests
  {
    public class Source { }
    public class Destination { }

    [TestMethod]
    public void SourceMapperOptionsInContextAreRespected()
    {
      var mapper = new MemberMapper();

      var options = new MapperOptions();
      options.Debug.DebugInformationEnabled = true;

      mapper.ForSourceType<Source>().UseMapperOptions(options);

      var map = mapper.CreateMap<Source, Destination>();

      Assert.IsNotNull(map.DebugInformation.MappingExpression);

      mapper.ClearMapCache();

      options.Debug.DebugInformationEnabled = false;

      map = mapper.CreateMap<Source, Destination>();

      Assert.IsNull(map.DebugInformation);


    }

    [TestMethod]
    public void DestinationMapperOptionsInContextAreRespected()
    {
      var mapper = new MemberMapper();

      var options = new MapperOptions();
      options.Debug.DebugInformationEnabled = true;

      mapper.ForDestinationType<Destination>().UseMapperOptions(options);

      var map = mapper.CreateMap<Source, Destination>();

      Assert.IsNotNull(map.DebugInformation.MappingExpression);

      mapper.ClearMapCache();

      options.Debug.DebugInformationEnabled = false;

      map = mapper.CreateMap<Source, Destination>();

      Assert.IsNull(map.DebugInformation);
    }

    [TestMethod]
    public void DestinationMapperOptionsTakePriorityOverSourceMapperOption()
    {
      var mapper = new MemberMapper();

      var options = new MapperOptions();
      options.Debug.DebugInformationEnabled = true;

      var options1 = new MapperOptions();
      options1.Debug.DebugInformationEnabled = false;

      mapper.ForDestinationType<Destination>().UseMapperOptions(options);
      mapper.ForSourceType<Source>().UseMapperOptions(options1);

      var map = mapper.CreateMap<Source, Destination>();

      Assert.IsNotNull(map.DebugInformation.MappingExpression);
    }
  }
}
