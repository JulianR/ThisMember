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

    public PropertyOrFieldInfo Source { get; private set; }

    public PropertyOrFieldInfo Destination { get; private set; }

    public void MapProperty(PropertyOrFieldInfo source, PropertyOrFieldInfo destination)
    {
      this.Source = source;
      this.Destination = destination;
    }

    public void IgnoreMember()
    {
      State = MemberOptionState.Ignored;
    }

  }
}
