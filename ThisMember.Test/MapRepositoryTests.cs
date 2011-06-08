using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using ThisMember.Core.Interfaces;

namespace ThisMember.Test
{
  [TestClass]
  public class MapRepositoryTests
  {
    [TestMethod]
    public void RepositoryIsUsedWhenSupplied()
    {
      var mapper = new MemberMapper();

      var repo = new MapRepository();

      mapper.MapRepository = repo;

      var result = mapper.Map<SourceType, DestinationType>(new SourceType { ID = "1" });

      Assert.AreEqual(1, result.Test);

    }

    class SourceType
    {
      public string ID { get; set; }
    }

    class DestinationType
    {
      public int Test { get; set; }
    }

    private class MapRepository : IMapRepository
    {

      private Dictionary<TypePair, Func<IMemberMapper, MappingOptions, ProposedMap>> cache = new Dictionary<TypePair, Func<IMemberMapper, MappingOptions, ProposedMap>>();

      public MapRepository()
      {
        CreateMap<SourceType, DestinationType>((mapper, options) =>
          {
            return mapper.CreateMapProposal<SourceType, DestinationType>(customMapping: src => new DestinationType
            {
              Test = int.Parse(src.ID)
            });
          });
      }

      private void CreateMap<TSource, TDestination>(Func<IMemberMapper, MappingOptions, ProposedMap> action)
      {
        cache.Add(new TypePair(typeof(TSource), typeof(TDestination)), action);
      }

      public bool TryGetMap(IMemberMapper mapper, MappingOptions options, TypePair pair, out ProposedMap map)
      {
        Func<IMemberMapper, MappingOptions, ProposedMap> action;
        if (cache.TryGetValue(pair, out action))
        {
          map = action(mapper, options);
          return true;
        }

        map = null;

        return false;
      }


      public bool TryGetMap<TSource, TDestination>(IMemberMapper mapper, MappingOptions options, out ProposedMap<TSource, TDestination> map)
      {
        Func<IMemberMapper, MappingOptions, ProposedMap> action;
        if (cache.TryGetValue(new TypePair(typeof(TSource), typeof(TDestination)), out action))
        {
          map = (ProposedMap<TSource, TDestination>)action(mapper, options);
          return true;
        }

        map = null;

        return false;
      }
    }


  }




}
