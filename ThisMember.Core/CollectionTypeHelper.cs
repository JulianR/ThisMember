using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ThisMember.Core.Interfaces;
using System.Collections;

namespace ThisMember.Core
{
  internal static class CollectionTypeHelper
  {

    public static bool IsEnumerable(ProposedTypeMapping mapping)
    {
      return typeof(IEnumerable).IsAssignableFrom(mapping.SourceMember.PropertyOrFieldType)
            && typeof(IEnumerable).IsAssignableFrom(mapping.DestinationMember.PropertyOrFieldType);
    }

    public static Type GetTypeInsideEnumerable(Type type)
    {
      var getEnumeratorMethod = type.GetMethod("GetEnumerator", Type.EmptyTypes);

      if (getEnumeratorMethod == null)
      {
        getEnumeratorMethod = (from i in type.GetInterfaces()
                               from m in i.GetMethods()
                               where m.Name == "GetEnumerator"
                               orderby m.ReturnType.IsGenericType descending
                               select m).FirstOrDefault();
                               
      }

      if (getEnumeratorMethod == null) return null;

      if (getEnumeratorMethod.ReturnType.IsGenericType)
      {
        return getEnumeratorMethod.ReturnType.GetGenericArguments().First();
      }
      else if (type.IsArray)
      {
        return type.GetElementType();
      }

      return typeof(object);

    }
  }
}
