using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;
using ThisMember.Core.Misc;
using ThisMember.Core.Options;

namespace ThisMember.Core.Fluent
{
  public class SourceTypeModifier
  {
    protected IMemberMapper mapper;
    protected Type type;

    public SourceTypeModifier(Type t, IMemberMapper mapper)
    {
      this.type = t;
      this.mapper = mapper;
    }

    public void UseMapperOptions(MapperOptions options)
    {
      mapper.Data.AddMapperOptions(type, options, MappingSides.Source);
    }

    public void ThrowIf(LambdaExpression condition, string message)
    {
      if (condition == null) throw new ArgumentNullException("condition");

      if (condition.Parameters.Count != 1 || condition.Parameters.Single().Type != type)
      {
        throw new InvalidOperationException("Invalid expression parameters");
      }

      if (condition.ReturnType != typeof(bool))
      {
        throw new InvalidOperationException("Invalid return type, must be bool");
      }

      var data = new TypeModifierData
      {
        Type = type,
        Message = message,
        ThrowIfCondition = condition
      };

      mapper.Data.AddTypeModifierData(data, MappingSides.Source);
    }
  }

  public class SourceTypeModifier<TSource> : SourceTypeModifier
  {

    public SourceTypeModifier(IMemberMapper mapper)
      : base(typeof(TSource), mapper)
    {
    }

    public VariableDefinition<T> DefineVariable<T>(string name)
    {
      var variable = new VariableDefinition<T>(name);

      mapper.Data.AddVariableDefinition(typeof(TSource), name, variable, MappingSides.Source);

      return variable;
    }

    public void ThrowIf(Expression<Func<TSource, bool>> condition, string message)
    {
      if (condition == null) throw new ArgumentNullException("condition");

      ThrowIf((LambdaExpression)condition, message);
    }
  }
}
