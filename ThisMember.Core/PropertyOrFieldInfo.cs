using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ThisMember.Core.Interfaces
{
  public class PropertyOrFieldInfo
  {
    private MemberInfo _member;

    public PropertyOrFieldInfo(MemberInfo member)
    {

      if (member == null) throw new ArgumentNullException("member");

      if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
      {
        throw new ArgumentException("member");
      }

      _member = member;
    }

    public Type PropertyOrFieldType
    {
      get
      {
        return _member.MemberType == MemberTypes.Property ? ((PropertyInfo)_member).PropertyType : ((FieldInfo)_member).FieldType;
      }
    }

    public string Name
    {
      get
      {
        return _member.Name;
      }
    }

    public override bool Equals(object obj)
    {
      var other = obj as PropertyOrFieldInfo;

      return !object.ReferenceEquals(other, null) && other._member.Equals(_member);
    }

    public static bool operator ==(PropertyOrFieldInfo left, PropertyOrFieldInfo right)
    {

      if (object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) return false;

      if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) return true;

      return left.Equals(right);
    }

    public static bool operator !=(PropertyOrFieldInfo left, PropertyOrFieldInfo right)
    {

      if (object.ReferenceEquals(left, null) && !object.ReferenceEquals(right, null)) return true;

      if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null)) return false;

      return !left.Equals(right);
    }

    public override int GetHashCode()
    {
      return _member.GetHashCode();
    }

    public static implicit operator PropertyOrFieldInfo(MemberInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

    public static implicit operator MemberInfo(PropertyOrFieldInfo member)
    {
      return member._member;
    }

    public static implicit operator PropertyOrFieldInfo(PropertyInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

    public static implicit operator PropertyOrFieldInfo(FieldInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

  }
}
