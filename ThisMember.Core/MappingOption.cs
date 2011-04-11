using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{

  public enum MappingOptionState
  {
    Default,
    Ignored
  }

  public class MappingOption
  {

    public MappingOptionState State { get; private set; }

    public void IgnoreMember()
    {
      State = MappingOptionState.Ignored;
    }

  }
}
