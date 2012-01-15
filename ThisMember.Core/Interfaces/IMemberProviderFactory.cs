using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Core.Interfaces
{
  public interface IMemberProviderFactory
  {
    IMemberProvider GetMemberProvider(Type sourceType, Type destinationType, IMemberMapper mapper);
  }
}
