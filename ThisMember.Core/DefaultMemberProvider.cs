using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  internal class DefaultMemberProvider : IMemberProvider
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

    private bool GetMemberOnType(Type type, IList<string> members, int index, IList<PropertyOrFieldInfo> memberStack)
    {
      var name = members[index];

      var member = type.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).FirstOrDefault();

      if (member == null)
      {
        // We couldn't find a property with that name, so in case that there's another property in the list after this one
        // try appending that and see if that results in a valid member.
        // Example: User.FirstName won't match to a property UserFirstName here, because it will have split it to 'User', 'First' and 'Name'.
        // This extra check will make sure it also tries to find a property 'First' + 'Name' on the source type before giving up.
        // TODO: Expand this to allow a property to consist of more than two 'camelcased' parts.
        if (index + 1 < members.Count)
        {
          member = type.GetMember(name + members[index + 1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).FirstOrDefault();
          index++;
        }

        if (member == null)
        {
          return false;
        }
      }

      if (!PropertyOrFieldInfo.IsPropertyOrField(member))
      {
        return false;
      }

      if (member.MemberType == MemberTypes.Property)
      {
        var property = (PropertyInfo)member;

        if (!property.CanRead) return false;
      }

      memberStack.Add(member);

      if (index < members.Count - 1)
      {
        return GetMemberOnType(((PropertyOrFieldInfo)member).PropertyOrFieldType, members, index + 1, memberStack);
      }

      return true;
    }

    public ProposedHierarchicalMapping ProposeHierarchicalMapping(PropertyOrFieldInfo destinationMember)
    {

      var split = CamelCaseHelper.SplitOnCamelCase(destinationMember.Name);

      if (split.Count <= 1)
      {
        return null;
      }

      var sourceMembers = SourceMembers;

      var memberStack = new List<PropertyOrFieldInfo>();

      var applies = GetMemberOnType(sourceType, split, 0, memberStack);

      if (applies)
      {
        return new ProposedHierarchicalMapping(memberStack);
      }

      return null;
    }


    private Dictionary<string, PropertyOrFieldInfo> SourceMembers
    {
      get
      {
        if (this.sourceProperties == null)
        {
          this.sourceProperties = (from p in sourceType.GetProperties()
                                   where p.CanRead && !p.GetIndexParameters().Any()
                                   select (PropertyOrFieldInfo)p)
                                  .Union(from f in sourceType.GetFields()
                                         where !f.IsStatic
                                         select (PropertyOrFieldInfo)f)
                                         .ToDictionary(k => k.Name, mapper.Options.Conventions.IgnoreCaseWhenFindingMatch ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        }
        return this.sourceProperties;
      }
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
      PropertyOrFieldInfo sourceProperty;

      SourceMembers.TryGetValue(destinationProperty.Name, out sourceProperty);

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
