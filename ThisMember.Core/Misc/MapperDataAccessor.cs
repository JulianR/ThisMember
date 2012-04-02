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
    private Dictionary<TypeDataCacheKey, TypeModifierData> typeModifierCache;
    private Dictionary<VariableCacheKey, VariableDefinition> variableCache;
    private Dictionary<OptionsCacheKey, MapperOptions> mapperOptionsCache;

    private byte[] lockObj = new byte[0];

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

    internal void AddMapperOptions(Type t, MapperOptions options, MappingSides side)
    {
      var key = new OptionsCacheKey
      {
        Type = t,
        Side = side
      };

      lock (MapperOptionsCache)
      {
        MapperOptionsCache[key] = options;
      }
    }

    internal MapperOptions TryGetMapperOptions(Type t, MappingSides side)
    {
      if (mapperOptionsCache == null)
      {
        return null;
      }

      var key = new OptionsCacheKey
      {
        Type = t,
        Side = side
      };

      MapperOptions options;

      MapperOptionsCache.TryGetValue(key, out options);

      return options;
    }

    internal void AddTypeModifierData(TypeModifierData data, MappingSides side)
    {
      lock (TypeModifierCache)
      {
        TypeModifierData current;

        var key = new TypeDataCacheKey
        {
          Type = data.Type,
          Side = side
        };

        if (TypeModifierCache.TryGetValue(key, out current))
        {
          current.Message = data.Message ?? current.Message;
          current.ThrowIfCondition = data.ThrowIfCondition ?? current.ThrowIfCondition;
        }
        else
        {
          TypeModifierCache[key] = data;
        }
      }
    }

    internal TypeModifierData TryGetTypeModifierData(Type t, MappingSides side)
    {
      TypeModifierData data;

      var key = new TypeDataCacheKey
      {
        Type = t,
        Side = side
      };

      if (!TypeModifierCache.TryGetValue(key, out data))
      {
        var item = TypeModifierCache.FirstOrDefault(s => s.Key.Type.IsAssignableFrom(t));

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

    internal void AddVariableDefinition<T>(Type type, string name, Fluent.VariableDefinition<T> variable, MappingSides side)
    {
      var key = new VariableCacheKey
      {
        Name = name,
        Type = type,
        Side = side
      };

      lock (VariableCache)
      {
        var existing = VariableCache.Keys.FirstOrDefault(k => k.Name == name);

        if (existing != null)
        {
          throw new InvalidOperationException(string.Format("Variable {0} already defined on type {1}", name, existing.Type));
        }

        VariableCache[key] = variable;
      }
    }

    internal IEnumerable<VariableDefinition> GetAllVariablesForType(Type t, MappingSides side)
    {
      if (variableCache == null)
      {
        return Enumerable.Empty<VariableDefinition>();
      }

      return VariableCache.Where(v => v.Key.Type == t && v.Key.Side == side).Select(v => v.Value);
    }

    internal VariableDefinition TryGetVariableDefinition(Type type, string name, MappingSides side)
    {
      var key = new VariableCacheKey
      {
        Name = name,
        Type = type,
        Side = side
      };

      VariableDefinition variable;

      VariableCache.TryGetValue(key, out variable);

      return variable;
    }

    #region Caches

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

    private Dictionary<TypeDataCacheKey, TypeModifierData> TypeModifierCache
    {
      get
      {
        if (typeModifierCache == null)
        {
          lock (lockObj)
          {
            if (typeModifierCache == null)
            {
              typeModifierCache = new Dictionary<TypeDataCacheKey, TypeModifierData>();
            }
          }
        }
        return typeModifierCache;
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
    #endregion Caches

    #region Keys
    private class OptionsCacheKey
    {
      public Type Type;
      public MappingSides Side;

      public override bool Equals(object obj)
      {
        var other = obj as OptionsCacheKey;
        return other != null && this.Type.IsAssignableFrom(other.Type) && other.Side == this.Side;
      }

      public override int GetHashCode()
      {
        return (Type.GetHashCode() << 5) ^ Side.GetHashCode();
      }
    }

    private class VariableCacheKey
    {
      public Type Type;
      public string Name;
      public MappingSides Side;

      public override bool Equals(object obj)
      {
        var other = obj as VariableCacheKey;
        return other != null && other.Type == this.Type && other.Name == this.Name
          && other.Side == this.Side;
      }

      public override int GetHashCode()
      {
        return (Type.GetHashCode() << 5) ^ Name.GetHashCode() ^ Side.GetHashCode();
      }
    }

    private class TypeDataCacheKey
    {
      public Type Type;
      public MappingSides Side;

      public override bool Equals(object obj)
      {
        var other = obj as TypeDataCacheKey;
        return other != null && this.Type.IsAssignableFrom(other.Type) && other.Side == this.Side;
      }

      public override int GetHashCode()
      {
        return (Type.GetHashCode() << 5) ^ Side.GetHashCode();
      }
    }

    #endregion Keys
  }

  internal enum MappingSides
  {
    Source,
    Destination
  }

  internal class TypeModifierData
  {
    public MappingSides Side { get; set; }
    public Type Type { get; set; }
    public string Message { get; set; }
    public LambdaExpression ThrowIfCondition { get; set; }
  }
}
