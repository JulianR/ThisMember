using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{

  public enum MemberOptionState
  {
    Default,
    Ignored
  }

  public class MemberOption
  {

    public MemberOptionState State { get; private set; }

    public void IgnoreMember()
    {
      State = MemberOptionState.Ignored;
    }

  }
}
