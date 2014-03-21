using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThisMember.Extensions
{
  public static class DictionaryExtensions
  {
    public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) where TValue : class
    {
      TValue value;

      source.TryGetValue(key, out value);

      return value;
    }
  }
}
