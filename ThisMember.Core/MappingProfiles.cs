using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class MapCollection
  {
    private Dictionary<string, IMemberMapper> mappers = new Dictionary<string, IMemberMapper>();

    public IMapRepository MapRepository { get; set; }

    public MapperOptions Options { get; set; }

    public MapCollection()
    {
      Options = new MapperOptions();
    }

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
