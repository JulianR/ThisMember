using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using ThisMember.Core.Options;
using System.Threading;

namespace ThisMember.Core
{
  /// <summary>
  /// A simple helper class that allows you to easily manage multiple mappers that each have their purpose.
  /// For example, use this class if you have multiple mappers for update and create scenarios.
  /// </summary>
  public class MapCollection
  {
    private Dictionary<string, IMemberMapper> mappers = new Dictionary<string, IMemberMapper>();

    public IMapRepository MapRepository { get; set; }

    public MapperOptions Options { get; set; }

    private byte[] lockObj = new byte[0];

    public MapCollection()
    {
      Options = new MapperOptions();
    }

    /// <summary>
    /// Returns a mapper for a certain profile, and creates it if the mapper does not exist yet.
    /// </summary>
    /// <param name="profile">The profile for which you want to have a mapper.</param>
    /// <returns></returns>
    public IMemberMapper this[string profile]
    {
      get
      {
        IMemberMapper mapper;

        if (!mappers.TryGetValue(profile, out mapper))
        {
          mapper = new MemberMapper { Profile = profile };

          mapper.MapRepository = this.MapRepository;

          mapper.Options = this.Options;

          lock (mappers)
          {
            if (!mappers.ContainsKey(profile))
            {
              CreateMapper(profile, mapper);
            }
          }
        }

        return mapper;
      }
      set
      {
        CreateMapper(profile, value);

      }
    }

    private void CreateMapper(string profile, IMemberMapper mapper)
    {
      lock (lockObj)
      {
        var newMappers = new Dictionary<string, IMemberMapper>(mappers);

        newMappers[profile] = mapper;

        if (mapper.MapRepository == null)
        {
          mapper.MapRepository = this.MapRepository;
        }

        Interlocked.CompareExchange(ref mappers, newMappers, mappers);
      }
    }
  }
}
