using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  public class MappingProfiles
  {
    private Dictionary<string, IMemberMapper> mappers = new Dictionary<string, IMemberMapper>();

    public IMemberMapper this[string profile]
    {
      get
      {
        IMemberMapper mapper;

        if (!mappers.TryGetValue(profile, out mapper))
        {
          mapper = new MemberMapper { Profile = profile };

          mappers.Add(profile, mapper);
        }

        return mapper;
      }
      set
      {
        mappers[profile] = value;
      }
    }

  }
}
