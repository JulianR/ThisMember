using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ThisMember.Core
{
  internal static class ConversionTypeHelper
  {
    internal static bool AreConvertible(Type source, Type destination)
    {

      var nullableSource = NullableTypeHelper.TryGetNullableType(source);
      source = nullableSource ?? source;

      var nullableDestination = NullableTypeHelper.TryGetNullableType(destination);
      destination = nullableDestination ?? destination;

      return AreExplicitlyConvertible(source, destination)
        || AreImplicitlyConvertible(source, destination)
        || CanConvertToOrFromEnum(source, destination);
    }

    private static readonly Dictionary<Type, IList<MethodInfo>> conversionMethodCache = new Dictionary<Type, IList<MethodInfo>>();

    internal static bool AreImplicitlyConvertible(Type source, Type destination)
    {
      if (legalConversions.Contains(new TypePair(source, destination)))
      {
        return true;
      }

      var methods = GetConversionMethods(source);

      var method = methods.Where(m => m.Name == "op_Implicit" && m.ReturnType == destination).SingleOrDefault();

      return method != null;
    }

    private static IList<MethodInfo> GetConversionMethods(Type source)
    {
      lock (conversionMethodCache)
      {
        IList<MethodInfo> methods;
        if (!conversionMethodCache.TryGetValue(source, out methods))
        {
          methods = source.GetMethods().Where(m => m.Name.StartsWith("op_")).ToList();

          conversionMethodCache.Add(source, methods);
        }
        return methods;
      }
    }

    internal static bool AreExplicitlyConvertible(Type source, Type destination)
    {
      if (legalConversions.Contains(new TypePair(source, destination)))
      {
        return true;
      }

      var methods = GetConversionMethods(source);

      var method = methods.Where(m => m.Name == "op_Explicit" && m.ReturnType == destination).SingleOrDefault();

      return method != null;
    }

    internal static bool CanConvertToOrFromEnum(Type source, Type destination)
    {
      return (source.IsEnum && (destination == typeof(int) || legalConversions.Contains(new TypePair(destination, typeof(int)))))
      || (destination.IsEnum && (source == typeof(int) || legalConversions.Contains(new TypePair(source, typeof(int)))));
    }

    private readonly static HashSet<TypePair> legalConversions = new HashSet<TypePair>
    (
      new[]
      {
        new TypePair(typeof(System.Boolean), typeof(System.Byte)),
        new TypePair(typeof(System.Boolean), typeof(System.Char)),
        new TypePair(typeof(System.Boolean), typeof(System.Double)),
        new TypePair(typeof(System.Boolean), typeof(System.Int16)),
        new TypePair(typeof(System.Boolean), typeof(System.Int32)),
        new TypePair(typeof(System.Boolean), typeof(System.Int64)),
        new TypePair(typeof(System.Boolean), typeof(System.SByte)),
        new TypePair(typeof(System.Boolean), typeof(System.Single)),
        new TypePair(typeof(System.Boolean), typeof(System.UInt16)),
        new TypePair(typeof(System.Boolean), typeof(System.UInt32)),
        new TypePair(typeof(System.Boolean), typeof(System.UInt64)),
        new TypePair(typeof(System.Byte), typeof(System.Char)),
        new TypePair(typeof(System.Byte), typeof(System.Double)),
        new TypePair(typeof(System.Byte), typeof(System.Int16)),
        new TypePair(typeof(System.Byte), typeof(System.Int32)),
        new TypePair(typeof(System.Byte), typeof(System.Int64)),
        new TypePair(typeof(System.Byte), typeof(System.SByte)),
        new TypePair(typeof(System.Byte), typeof(System.Single)),
        new TypePair(typeof(System.Byte), typeof(System.UInt16)),
        new TypePair(typeof(System.Byte), typeof(System.UInt32)),
        new TypePair(typeof(System.Byte), typeof(System.UInt64)),
        new TypePair(typeof(System.Byte), typeof(System.Decimal)),
        new TypePair(typeof(System.Char), typeof(System.Byte)),
        new TypePair(typeof(System.Char), typeof(System.Double)),
        new TypePair(typeof(System.Char), typeof(System.Int16)),
        new TypePair(typeof(System.Char), typeof(System.Int32)),
        new TypePair(typeof(System.Char), typeof(System.Int64)),
        new TypePair(typeof(System.Char), typeof(System.SByte)),
        new TypePair(typeof(System.Char), typeof(System.Single)),
        new TypePair(typeof(System.Char), typeof(System.UInt16)),
        new TypePair(typeof(System.Char), typeof(System.UInt32)),
        new TypePair(typeof(System.Char), typeof(System.UInt64)),
        new TypePair(typeof(System.Char), typeof(System.Decimal)),
        new TypePair(typeof(System.Double), typeof(System.Byte)),
        new TypePair(typeof(System.Double), typeof(System.Char)),
        new TypePair(typeof(System.Double), typeof(System.Int16)),
        new TypePair(typeof(System.Double), typeof(System.Int32)),
        new TypePair(typeof(System.Double), typeof(System.Int64)),
        new TypePair(typeof(System.Double), typeof(System.SByte)),
        new TypePair(typeof(System.Double), typeof(System.Single)),
        new TypePair(typeof(System.Double), typeof(System.UInt16)),
        new TypePair(typeof(System.Double), typeof(System.UInt32)),
        new TypePair(typeof(System.Double), typeof(System.UInt64)),
        new TypePair(typeof(System.Double), typeof(System.Decimal)),
        new TypePair(typeof(System.Int16), typeof(System.Byte)),
        new TypePair(typeof(System.Int16), typeof(System.Char)),
        new TypePair(typeof(System.Int16), typeof(System.Double)),
        new TypePair(typeof(System.Int16), typeof(System.Int32)),
        new TypePair(typeof(System.Int16), typeof(System.Int64)),
        new TypePair(typeof(System.Int16), typeof(System.SByte)),
        new TypePair(typeof(System.Int16), typeof(System.Single)),
        new TypePair(typeof(System.Int16), typeof(System.UInt16)),
        new TypePair(typeof(System.Int16), typeof(System.UInt32)),
        new TypePair(typeof(System.Int16), typeof(System.UInt64)),
        new TypePair(typeof(System.Int16), typeof(System.Decimal)),
        new TypePair(typeof(System.Int32), typeof(System.Byte)),
        new TypePair(typeof(System.Int32), typeof(System.Char)),
        new TypePair(typeof(System.Int32), typeof(System.Double)),
        new TypePair(typeof(System.Int32), typeof(System.Int16)),
        new TypePair(typeof(System.Int32), typeof(System.Int64)),
        new TypePair(typeof(System.Int32), typeof(System.IntPtr)),
        new TypePair(typeof(System.Int32), typeof(System.SByte)),
        new TypePair(typeof(System.Int32), typeof(System.Single)),
        new TypePair(typeof(System.Int32), typeof(System.UInt16)),
        new TypePair(typeof(System.Int32), typeof(System.UInt32)),
        new TypePair(typeof(System.Int32), typeof(System.UInt64)),
        new TypePair(typeof(System.Int32), typeof(System.Decimal)),
        new TypePair(typeof(System.Int64), typeof(System.Byte)),
        new TypePair(typeof(System.Int64), typeof(System.Char)),
        new TypePair(typeof(System.Int64), typeof(System.Double)),
        new TypePair(typeof(System.Int64), typeof(System.Int16)),
        new TypePair(typeof(System.Int64), typeof(System.Int32)),
        new TypePair(typeof(System.Int64), typeof(System.IntPtr)),
        new TypePair(typeof(System.Int64), typeof(System.SByte)),
        new TypePair(typeof(System.Int64), typeof(System.Single)),
        new TypePair(typeof(System.Int64), typeof(System.UInt16)),
        new TypePair(typeof(System.Int64), typeof(System.UInt32)),
        new TypePair(typeof(System.Int64), typeof(System.UInt64)),
        new TypePair(typeof(System.Int64), typeof(System.Decimal)),
        new TypePair(typeof(System.IntPtr), typeof(System.Int32)),
        new TypePair(typeof(System.IntPtr), typeof(System.Int64)),
        new TypePair(typeof(System.SByte), typeof(System.Byte)),
        new TypePair(typeof(System.SByte), typeof(System.Char)),
        new TypePair(typeof(System.SByte), typeof(System.Double)),
        new TypePair(typeof(System.SByte), typeof(System.Int16)),
        new TypePair(typeof(System.SByte), typeof(System.Int32)),
        new TypePair(typeof(System.SByte), typeof(System.Int64)),
        new TypePair(typeof(System.SByte), typeof(System.Single)),
        new TypePair(typeof(System.SByte), typeof(System.UInt16)),
        new TypePair(typeof(System.SByte), typeof(System.UInt32)),
        new TypePair(typeof(System.SByte), typeof(System.UInt64)),
        new TypePair(typeof(System.SByte), typeof(System.Decimal)),
        new TypePair(typeof(System.Single), typeof(System.Byte)),
        new TypePair(typeof(System.Single), typeof(System.Char)),
        new TypePair(typeof(System.Single), typeof(System.Double)),
        new TypePair(typeof(System.Single), typeof(System.Int16)),
        new TypePair(typeof(System.Single), typeof(System.Int32)),
        new TypePair(typeof(System.Single), typeof(System.Int64)),
        new TypePair(typeof(System.Single), typeof(System.SByte)),
        new TypePair(typeof(System.Single), typeof(System.UInt16)),
        new TypePair(typeof(System.Single), typeof(System.UInt32)),
        new TypePair(typeof(System.Single), typeof(System.UInt64)),
        new TypePair(typeof(System.Single), typeof(System.Decimal)),
        new TypePair(typeof(System.UInt16), typeof(System.Byte)),
        new TypePair(typeof(System.UInt16), typeof(System.Char)),
        new TypePair(typeof(System.UInt16), typeof(System.Double)),
        new TypePair(typeof(System.UInt16), typeof(System.Int16)),
        new TypePair(typeof(System.UInt16), typeof(System.Int32)),
        new TypePair(typeof(System.UInt16), typeof(System.Int64)),
        new TypePair(typeof(System.UInt16), typeof(System.SByte)),
        new TypePair(typeof(System.UInt16), typeof(System.Single)),
        new TypePair(typeof(System.UInt16), typeof(System.UInt32)),
        new TypePair(typeof(System.UInt16), typeof(System.UInt64)),
        new TypePair(typeof(System.UInt16), typeof(System.Decimal)),
        new TypePair(typeof(System.UInt32), typeof(System.Byte)),
        new TypePair(typeof(System.UInt32), typeof(System.Char)),
        new TypePair(typeof(System.UInt32), typeof(System.Double)),
        new TypePair(typeof(System.UInt32), typeof(System.Int16)),
        new TypePair(typeof(System.UInt32), typeof(System.Int32)),
        new TypePair(typeof(System.UInt32), typeof(System.Int64)),
        new TypePair(typeof(System.UInt32), typeof(System.SByte)),
        new TypePair(typeof(System.UInt32), typeof(System.Single)),
        new TypePair(typeof(System.UInt32), typeof(System.UInt16)),
        new TypePair(typeof(System.UInt32), typeof(System.UInt64)),
        new TypePair(typeof(System.UInt32), typeof(System.UIntPtr)),
        new TypePair(typeof(System.UInt32), typeof(System.Decimal)),
        new TypePair(typeof(System.UInt64), typeof(System.Byte)),
        new TypePair(typeof(System.UInt64), typeof(System.Char)),
        new TypePair(typeof(System.UInt64), typeof(System.Double)),
        new TypePair(typeof(System.UInt64), typeof(System.Int16)),
        new TypePair(typeof(System.UInt64), typeof(System.Int32)),
        new TypePair(typeof(System.UInt64), typeof(System.Int64)),
        new TypePair(typeof(System.UInt64), typeof(System.SByte)),
        new TypePair(typeof(System.UInt64), typeof(System.Single)),
        new TypePair(typeof(System.UInt64), typeof(System.UInt16)),
        new TypePair(typeof(System.UInt64), typeof(System.UInt32)),
        new TypePair(typeof(System.UInt64), typeof(System.UIntPtr)),
        new TypePair(typeof(System.UInt64), typeof(System.Decimal)),
        new TypePair(typeof(System.UIntPtr), typeof(System.UInt32)),
        new TypePair(typeof(System.UIntPtr), typeof(System.UInt64)),
        new TypePair(typeof(System.Decimal), typeof(System.Byte)),
        new TypePair(typeof(System.Decimal), typeof(System.Char)),
        new TypePair(typeof(System.Decimal), typeof(System.Double)),
        new TypePair(typeof(System.Decimal), typeof(System.Int16)),
        new TypePair(typeof(System.Decimal), typeof(System.Int32)),
        new TypePair(typeof(System.Decimal), typeof(System.Int64)),
        new TypePair(typeof(System.Decimal), typeof(System.SByte)),
        new TypePair(typeof(System.Decimal), typeof(System.Single)),
        new TypePair(typeof(System.Decimal), typeof(System.UInt16)),
        new TypePair(typeof(System.Decimal), typeof(System.UInt32)),
        new TypePair(typeof(System.Decimal), typeof(System.UInt64))
      }
    );

  }
}
