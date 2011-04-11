using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace ThisMember.Core.Interfaces
{

  public class MemberExpressionTuple
  {
    public PropertyOrFieldInfo Member { get; set; }

    public Expression Expression { get; set; }
  }

  public class CustomMapping
  {

    public IList<MemberExpressionTuple> Members { get; set; }

    public IList<CustomMapping> CustomMappings { get; set; }

    public Type DestinationType { get; set; }

    public ParameterExpression Parameter { get; set; }

    public CustomMapping()
    {
      CustomMappings = new List<CustomMapping>();
      Members = new List<MemberExpressionTuple>();
    }

    public static CustomMapping GetCustomMapping(Type destinationType, Expression expression)
    {
      var lambda = expression as LambdaExpression;

      if (lambda == null) throw new ArgumentException("Only LambdaExpression is allowed here");

      var newType = lambda.Body as NewExpression;

      if (newType == null) throw new ArgumentException("Only NewExpression is allowed to specify a custom mapping");

      var mapping = GetCustomMappingFromNewExpression(destinationType, newType);

      mapping.Parameter = lambda.Parameters.First();

      return mapping;
    }

    public Expression GetExpressionForMember(PropertyOrFieldInfo member)
    {
      foreach (var m in this.Members)
      {
        if (member.Equals(m.Member))
        {
          return m.Expression;
        }
      }

      Expression expression = null;

      foreach (var cm in this.CustomMappings)
      {
        expression = cm.GetExpressionForMember(member);
      }

      return expression;
    }

    private static CustomMapping GetCustomMappingFromNewExpression(Type destinationType, NewExpression expression)
    {
      var newMapping = new CustomMapping();

      newMapping.DestinationType = destinationType;

      for (var i = 0; i < expression.Arguments.Count; i++)
      {
        var member = expression.Members[i];
        var argument = expression.Arguments[i];

        var memberOnDestination = destinationType.GetMember(member.Name).FirstOrDefault();

        if (memberOnDestination == null)
        {
          throw new ArgumentException(string.Format("Member {0} does not exist on type {1}", member.Name, destinationType.Name));
        }

        var memberExpression = new MemberExpressionTuple();

        memberExpression.Member = memberOnDestination;

        if (argument is NewExpression)
        {
          newMapping.CustomMappings.Add(GetCustomMappingFromNewExpression(memberExpression.Member.PropertyOrFieldType, (NewExpression)argument));
        }
        else
        {
          memberExpression.Expression = argument;
        }

        newMapping.Members.Add(memberExpression);

      }
      return newMapping;
    }

  }



}
