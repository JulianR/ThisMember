using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using ThisMember.Core.Options;

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

          mappers.Add(profile, mapper);
        }

        return mapper;
      }
      set
      {
        mappers[profile] = value;

        if (value.MapRepository == null)
        {
          value.MapRepository = this.MapRepository;
        }

      }
    }

  }
}
