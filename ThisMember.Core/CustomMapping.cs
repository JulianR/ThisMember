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

    public override int GetHashCode()
    {
      return Member.GetHashCode();
    }

  }

  public class CustomMapping
  {
    public IList<MemberExpressionTuple> Members { get; set; }

    public IList<CustomMapping> CustomMappings { get; set; }

    public Type DestinationType { get; set; }

    public ParameterExpression SourceParameter { get; set; }

    public IList<IndexedParameterExpression> ArgumentParameters { get; set; }

    public CustomMapping()
    {
      CustomMappings = new List<CustomMapping>();
      Members = new List<MemberExpressionTuple>();
      ArgumentParameters = new List<IndexedParameterExpression>();
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
        throw new ArgumentException(string.Format("Only new {0} { .. } and new { .. } are allowed as a custom mapping", destinationType.Name));
      }

      int index = 0;
      foreach (var param in lambda.Parameters)
      {
        if (mapping.SourceParameter == null)
        {
          mapping.SourceParameter = param;
        }
        else
        {
          mapping.ArgumentParameters.Add(new IndexedParameterExpression { Index = index, Parameter = param });
        }
        index++;
      }

      return mapping;
    }

    public class ParameterVisitor : ExpressionVisitor
    {
      private IList<ParameterExpression> _newParams;
      private IList<ParameterExpression> _oldParams;

      public ParameterVisitor(IList<ParameterExpression> oldParams, IList<ParameterExpression> newParams)
      {
        _oldParams = oldParams;
        _newParams = newParams;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        for (var i = 0; i < _oldParams.Count; i++)
        {
          if (_oldParams[i] == node)
          {
            return _newParams[i];
          }
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
            var oldParams = otherMapping.ArgumentParameters.Select(p => p.Parameter).ToList();
            var newParams = root.ArgumentParameters.Select(p => p.Parameter).ToList();

            oldParams.Add(otherMapping.SourceParameter);
            newParams.Add(root.SourceParameter);

            var visitor = new ParameterVisitor(oldParams, newParams);

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
