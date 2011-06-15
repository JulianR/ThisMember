using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ThisMember.Core.Interfaces
{
  public class PropertyOrFieldInfo
  {
    private MemberInfo member;

    public PropertyOrFieldInfo(MemberInfo member)
    {

      if (member == null) throw new ArgumentNullException("member");

      if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field)
      {
        throw new ArgumentException("member");
      }

      this.member = member;
    }

    public Type PropertyOrFieldType
    {
      get
      {
        return member.MemberType == MemberTypes.Property ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
      }
    }

    public string Name
    {
      get
      {
        return member.Name;
      }
    }

    public Type DeclaringType
    {
      get
      {
        return member.DeclaringType;
      }
    }

    public object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
      return member.GetCustomAttributes(attributeType, inherit);
    }

    public override bool Equals(object obj)
    {
      var other = obj as PropertyOrFieldInfo;

      if (!object.ReferenceEquals(other, null))
      {
        var equals = other.member.Equals(member);

        if (equals) return true;

        if (other.member.MetadataToken == this.member.MetadataToken) return true;

        if (other.member.DeclaringType.IsInterface && other.member.DeclaringType.IsAssignableFrom(this.member.DeclaringType) && other.member.Name == this.member.Name)
        {
          return true;
        }

        if (this.member.DeclaringType.IsInterface && this.member.DeclaringType.IsAssignableFrom(other.member.DeclaringType) && this.member.Name == other.member.Name)
        {
          return true;
        }
      }

      return false;
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
      return member.GetHashCode();
    }

    public static implicit operator PropertyOrFieldInfo(MemberInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

    public static implicit operator MemberInfo(PropertyOrFieldInfo member)
    {
      return member.member;
    }

    public static implicit operator PropertyOrFieldInfo(PropertyInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

    public static implicit operator PropertyOrFieldInfo(FieldInfo member)
    {
      return new PropertyOrFieldInfo(member);
    }

    public override string ToString()
    {
      return this.member.ToString();
    }

  }
}
