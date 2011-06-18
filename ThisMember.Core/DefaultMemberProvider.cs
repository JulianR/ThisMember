using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;

namespace ThisMember.Core
{
  internal class DefaultMemberProvider
  {
    private IMemberMapper mapper;
    private Dictionary<string, PropertyOrFieldInfo> sourceProperties;
    private Type sourceType;
    private Type destinationType;

    public DefaultMemberProvider(Type sourceType, Type destinationType, IMemberMapper mapper)
    {
      this.mapper = mapper;
      this.sourceType = sourceType;
      this.destinationType = destinationType;
    }

    public Dictionary<string, PropertyOrFieldInfo> GetSourceMembers()
    {
      var sourceProperties = (from p in sourceType.GetProperties()
                              where p.CanRead && !p.GetIndexParameters().Any()
                              select (PropertyOrFieldInfo)p)
                              .Union(from f in sourceType.GetFields()
                                     where !f.IsStatic
                                     select (PropertyOrFieldInfo)f)
                                     .ToDictionary(k => k.Name, mapper.Options.Conventions.IgnoreCaseWhenFindingMatch ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
      return sourceProperties;
    }

    public IEnumerable<PropertyOrFieldInfo> GetDestinationMembers()
    {
      var destinationProperties = (from p in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   where p.CanWrite && !p.GetIndexParameters().Any()
                                   select (PropertyOrFieldInfo)p)
                                   .Union(from f in destinationType.GetFields()
                                          where !f.IsStatic
                                          select (PropertyOrFieldInfo)f);
      return destinationProperties;
    }

    public PropertyOrFieldInfo GetMatchingSourceMember(PropertyOrFieldInfo destinationProperty)
    {
      if (sourceProperties == null)
      {
        sourceProperties = GetSourceMembers();
      }

      PropertyOrFieldInfo sourceProperty;

      sourceProperties.TryGetValue(destinationProperty.Name, out sourceProperty);

      return sourceProperty;

    }

    public bool IsMemberIgnored(Type sourceType, PropertyOrFieldInfo destinationProperty)
    {
      if (mapper.Options.Conventions.IgnoreMemberAttributeShouldBeRespected)
      {
        var ignoreAttribute = destinationProperty.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).SingleOrDefault() as IgnoreMemberAttribute;

        if (ignoreAttribute != null)
        {
          var ignore = true;

          if (!string.IsNullOrEmpty(ignoreAttribute.Profile))
          {
            ignore &= ignoreAttribute.Profile == mapper.Profile;
          }

          if (ignoreAttribute.WhenSourceTypeIs != null)
          {
            ignore &= ignoreAttribute.WhenSourceTypeIs == sourceType;
          }

          if (ignore) return true;
        }
      }
      return false;

    }
  }
}
