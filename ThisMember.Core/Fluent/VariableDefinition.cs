using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Fluent
{
  public class VariableDefinition
  {
    public string Name { get; private set; }
    public Type Type { get; private set; }
    public LambdaExpression Initialization { get; protected set; }

    public VariableDefinition(Type t, string name)
    {
      this.Type = t;
      this.Name = name;
    }
  }

  public class VariableDefinition<T> : VariableDefinition
  {
    public VariableDefinition(string name) : base(typeof(T), name) { }

    public void InitializedAs(Expression<Func<T>> initializer)
    {
      Initialization = initializer;
    }

  }

  public static class Variable
  {
    public static T Use<T>(string name)
    {
      return default(T);
    }
  }
}
