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

    public override bool Equals(object obj)
    {
      var other = obj as MemberExpressionTuple;

      return other != null && other.Member.Equals(this.Member);
    }

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

      CustomMapping mapping;

      MemberInitExpression memberInit;

      if (newType != null)
      {
        mapping = GetCustomMappingFromNewExpression(destinationType, newType);
      }
      else if ((memberInit = lambda.Body as MemberInitExpression) != null)
      {
        mapping = GetCustomMappingFromMemberInitExpression(destinationType, memberInit);
      }
      else
      {
        throw new ArgumentException("Only MemberInit and NewExpression are allowed to specify custom mappings");
      }

      mapping.Parameter = lambda.Parameters.First();

      return mapping;
    }

    private class ParameterVisitor : ExpressionVisitor
    {
      private ParameterExpression _newParam;
      private ParameterExpression _oldParam;

      public ParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
      {
        _oldParam = oldParam;
        _newParam = newParam;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {

        if (_oldParam == node)
        {
          return _newParam;
        }

        return base.VisitParameter(node);
      }
    }

    public void CombineWithOtherCustomMappings(IEnumerable<CustomMapping> mappings)
    {
      CombineWithOtherCustomMappings(this, mappings);
    }

    public void CombineWithOtherCustomMappings(CustomMapping root, IEnumerable<CustomMapping> mappings)
    {
      foreach (var otherMapping in mappings)
      {
        foreach (var m in otherMapping.Members)
        {
          if (!root.Members.Contains(m))
          {
            var visitor = new ParameterVisitor(otherMapping.Parameter, root.Parameter);

            var member = new MemberExpressionTuple
            {
              Expression = visitor.Visit(m.Expression),
              Member = m.Member
            };

            root.Members.Add(member);
          }
        }
      }

      //foreach(var mapping in root.CustomMappings)
      //{
      //  CombineWithOtherCustomMappings(mapping.
      //}

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
    private static CustomMapping GetCustomMappingFromMemberInitExpression(Type destinationType, MemberInitExpression expression)
    {
      var newMapping = new CustomMapping();

      newMapping.DestinationType = destinationType;

      foreach (MemberAssignment assignment in expression.Bindings)
      {
        var member = assignment.Member;
        var argument = assignment.Expression;

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
