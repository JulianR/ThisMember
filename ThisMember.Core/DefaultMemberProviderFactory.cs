using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  internal class DefaultMemberProviderFactory : IMemberProviderFactory
  {
    public IMemberProvider GetMemberProvider(Type sourceType, Type destinationType, IMemberMapper mapper)
    {
      return new DefaultMemberProvider(sourceType, destinationType, mapper);
    }
  }
}
