using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Misc
{
  public class MapperDataAccessor
  {
    private MemberMapper memberMapper;

    private Dictionary<Type, LambdaExpression> constructorCache = new Dictionary<Type, LambdaExpression>();
    private Dictionary<Type, SourceTypeData> sourceTypeCache = new Dictionary<Type, SourceTypeData>();

    public MapperDataAccessor(MemberMapper memberMapper)
    {
      this.memberMapper = memberMapper;
    }

    internal LambdaExpression GetConstructor(Type t)
    {
      LambdaExpression e;
      constructorCache.TryGetValue(t, out e);
      return e;
    }

    internal void AddSourceTypeData(SourceTypeData data)
    {
      lock (sourceTypeCache)
      {
        SourceTypeData current;
        if (sourceTypeCache.TryGetValue(data.Type, out current))
        {
          current.Message = data.Message ?? current.Message;
          current.ThrowIfCondition = data.ThrowIfCondition ?? current.ThrowIfCondition;
        }
        else
        {
          sourceTypeCache[data.Type] = data;
        }
      }
    }

    internal SourceTypeData TryGetSourceTypeData(Type t)
    {
      SourceTypeData data;

      if (!sourceTypeCache.TryGetValue(t, out data))
      {
        var item = sourceTypeCache.FirstOrDefault(s => s.Key.IsAssignableFrom(t));

        data = item.Value;
      }

      return data;
    }

    internal void AddCustomConstructor(Type type, LambdaExpression ctor)
    {
      lock (constructorCache)
      {
        constructorCache[type] = ctor;
      }
    }
  }

  internal class SourceTypeData
  {
    public Type Type { get; set; }
    public string Message { get;set;}
    public LambdaExpression ThrowIfCondition { get;set;}
  }
}
