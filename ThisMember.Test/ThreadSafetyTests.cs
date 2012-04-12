using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class ThreadSafetyTests
  {
    class Source
    {
      public int Foo { get; set; }
    }

    class Destination
    {
      public int Foo { get; set; }
    }

    [TestMethod]
    public void CreateMapIsThreadSafe()
    {
      var mapper = new MemberMapper();

      var thread1 = new Thread(() =>
      {
        while (true)
        {
          var map = mapper.CreateMap<Source, Destination>();

          var result = map.MappingFunction(new Source { Foo = 1 }, new Destination());

          Assert.AreEqual(1, result.Foo);

          mapper.ClearMapCache();
        }
      });

      var thread2 = new Thread(() =>
      {
        while (true)
        {
          var map = mapper.CreateMap<Source, Destination>();

          var result = map.MappingFunction(new Source { Foo = 1 }, new Destination());

          Assert.AreEqual(1, result.Foo);

          mapper.ClearMapCache();
        }
      });

      thread1.Start();
      thread2.Start();

      Thread.Sleep(2000);

      thread1.Abort();
      thread2.Abort();
    }
  }
}
