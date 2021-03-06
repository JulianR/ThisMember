﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Extensions
{
  public static class TypeExtensions
  {
    public static bool IsNullableValueType(this Type type)
    {
      return (type.IsGenericType && type.
        GetGenericTypeDefinition().Equals
        (typeof(Nullable<>)));
    }
  }
}
