using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core.Fluent;
using ThisMember.Core.Options;

namespace ThisMember.Core.Misc
{
  public class MapperDataAccessor
  {
    private MemberMapper memberMapper;

    private Dictionary<Type, LambdaExpression> constructorCache;
    private Dictionary<Type, SourceTypeData> sourceTypeCache;
    private Dictionary<VariableCacheKey, VariableDefinition> variableCache;
    private Dictionary<OptionsCacheKey, MapperOptions> mapperOptionsCache;

    private byte[] lockObj = new byte[0];

    private Dictionary<Type, LambdaExpression> ConstructorCache
    {
      get
      {
        if (constructorCache == null)
        {
          lock (lockObj)
          {
            if (constructorCache == null)
            {
              constructorCache = new Dictionary<Type, LambdaExpression>();
            }
          }
        }
        return constructorCache;
      }
    }

    private Dictionary<Type, SourceTypeData> SourceTypeCache
    {
      get
      {
        if (sourceTypeCache == null)
        {
          lock (lockObj)
          {
            if (sourceTypeCache == null)
            {
              sourceTypeCache = new Dictionary<Type, SourceTypeData>();
            }
          }
        }
        return sourceTypeCache;
      }
    }

    private Dictionary<VariableCacheKey, VariableDefinition> VariableCache
    {
      get
      {
        if (variableCache == null)
        {
          lock (lockObj)
          {
            if (variableCache == null)
            {
              variableCache = new Dictionary<VariableCacheKey, VariableDefinition>();
            }
          }
        }
        return variableCache;
      }
    }

    private Dictionary<OptionsCacheKey, MapperOptions> MapperOptionsCache
    {
      get
      {
        if (mapperOptionsCache == null)
        {
          lock (lockObj)
          {
            if (mapperOptionsCache == null)
            {
              mapperOptionsCache = new Dictionary<OptionsCacheKey, MapperOptions>();
            }
          }
        }
        return mapperOptionsCache;
      }
    }

    public MapperDataAccessor(MemberMapper memberMapper)
    {
      this.memberMapper = memberMapper;
    }

    internal LambdaExpression GetConstructor(Type t)
    {
      LambdaExpression e;
      ConstructorCache.TryGetValue(t, out e);
      return e;
    }

    private class OptionsCacheKey
    {
      public Type Type;
      public bool IsSource;

      public override bool Equals(object obj)
      {
        var other = obj as OptionsCacheKey;
        return other != null && this.Type.IsAssignableFrom(other.Type) && other.IsSource == this.IsSource;
      }

      public override int GetHashCode()
      {
        return (Type.GetHashCode() << 5) ^ IsSource.GetHashCode();
      }
    }

    private class VariableCacheKey
    {
      public Type Type;
      public string Name;

      public override bool Equals(object obj)
      {
        var other = obj as VariableCacheKey;
        return other != null && other.Type == this.Type && other.Name == this.Name;
      }

      public override int GetHashCode()
      {
        return (Type.GetHashCode() << 5) ^ Name.GetHashCode();
      }
    }

    internal void AddMapperOptions(Type t, MapperOptions options, bool isSource)
    {
      var key = new OptionsCacheKey
      {
        Type = t,
        IsSource = isSource
      };

      lock (MapperOptionsCache)
      {
        MapperOptionsCache[key] = options;
      }
    }

    internal MapperOptions TryGetMapperOptions(Type t, bool isSource)
    {
      if (mapperOptionsCache == null)
      {
        return null;
      }

      var key = new OptionsCacheKey
      {
        Type = t,
        IsSource = isSource
      };

      MapperOptions options;

      MapperOptionsCache.TryGetValue(key, out options);

      return options;
    }

    internal void AddSourceTypeData(SourceTypeData data)
    {
      lock (SourceTypeCache)
      {
        SourceTypeData current;
        if (SourceTypeCache.TryGetValue(data.Type, out current))
        {
          current.Message = data.Message ?? current.Message;
          current.ThrowIfCondition = data.ThrowIfCondition ?? current.ThrowIfCondition;
        }
        else
        {
          SourceTypeCache[data.Type] = data;
        }
      }
    }

    internal SourceTypeData TryGetSourceTypeData(Type t)
    {
      SourceTypeData data;

      if (!SourceTypeCache.TryGetValue(t, out data))
      {
        var item = SourceTypeCache.FirstOrDefault(s => s.Key.IsAssignableFrom(t));

        data = item.Value;
      }

      return data;
    }

    internal void AddCustomConstructor(Type type, LambdaExpression ctor)
    {
      lock (ConstructorCache)
      {
        ConstructorCache[type] = ctor;
      }
    }

    internal void AddVariableDefinition<T>(Type type, string name, Fluent.VariableDefinition<T> variable)
    {
      var key = new VariableCacheKey
      {
        Name = name,
        Type = type
      };

      lock (VariableCache)
      {
        VariableCache[key] = variable;
      }
    }

    internal IEnumerable<VariableDefinition> GetAllVariablesForType(Type t)
    {
      if (variableCache == null)
      {
        return Enumerable.Empty<VariableDefinition>();
      }
      return null;

      //return (from v in variableCache
      //        where v.Key.Type
    }

    internal VariableDefinition TryGetVariableDefinition(Type type, string name)
    {
      var key = new VariableCacheKey
      {
        Name = name,
        Type = type
      };

      VariableDefinition variable;

      VariableCache.TryGetValue(key, out variable);

      return variable;
    }
  }

  internal class SourceTypeData
  {
    public Type Type { get; set; }
    public string Message { get; set; }
    public LambdaExpression ThrowIfCondition { get; set; }
  }
}
