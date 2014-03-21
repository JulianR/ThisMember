using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using ThisMember.Extensions;

namespace ThisMember.Core
{
  internal class NullableTypeHelper
  {
    internal static Type TryGetNullableType(Type type)
    {
      Type nullableType = null;

      if (type.IsNullableValueType())
      {
        nullableType = type.GetGenericArguments().Single();
      }
      return nullableType;
    }

    internal static Type TryGetNullableType(PropertyOrFieldInfo sourceMember)
    {
      if (sourceMember == null) return null;

      return TryGetNullableType(sourceMember.PropertyOrFieldType);
    }


  }
}
