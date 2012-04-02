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
    public IList<MemberExpressionTuple> Members { get; internal set; }

    public IList<CustomMapping> CustomMappings { get; internal set; }

    public Type DestinationType { get; internal set; }

    internal ParameterExpression SourceParameter { get; set; }

    internal IList<IndexedParameterExpression> ArgumentParameters { get; set; }

    private class ConversionFunctionKey
    {
      public ConversionFunctionKey(PropertyOrFieldInfo source, PropertyOrFieldInfo dest)
      {
        if (source != null)
        {
          this.sourceAsString = source.DeclaringType.FullName + "." + source.Name;
        }
        else
        {
          this.sourceAsString = "";
        }
        if (dest != null)
        {
          this.destinationAsString = dest.DeclaringType.FullName + "." + dest.Name;
        }
        else
        {
          this.destinationAsString = "";
        }
      }
      private readonly string sourceAsString;
      private readonly string destinationAsString;

      public override bool Equals(object obj)
      {
        var other = obj as ConversionFunctionKey;

        return other != null && other.sourceAsString == this.sourceAsString && other.destinationAsString == this.destinationAsString;
      }

      public override int GetHashCode()
      {
        return (this.sourceAsString.GetHashCode() << 17) ^ this.destinationAsString.GetHashCode();
      }
    }

    private Dictionary<ConversionFunctionKey, LambdaExpression> conversionFunctions;

    public CustomMapping()
    {
      CustomMappings = new List<CustomMapping>();
      Members = new List<MemberExpressionTuple>();
      ArgumentParameters = new List<IndexedParameterExpression>();
      this.conversionFunctions = new Dictionary<ConversionFunctionKey, LambdaExpression>();
    }

    public void AddConversionFunction(PropertyOrFieldInfo source, PropertyOrFieldInfo destination, LambdaExpression conversion)
    {
      lock (conversionFunctions)
      {
        this.conversionFunctions[new ConversionFunctionKey(source, destination)] = conversion;
      }
    }

    public LambdaExpression GetConversionFunction(PropertyOrFieldInfo source, PropertyOrFieldInfo destination)
    {
      LambdaExpression conversion;

      this.conversionFunctions.TryGetValue(new ConversionFunctionKey(source, destination), out conversion);

      return conversion;
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
      private IList<ParameterExpression> newParams;
      private IList<ParameterExpression> oldParams;

      public ParameterVisitor(IList<ParameterExpression> oldParams, IList<ParameterExpression> newParams)
      {
        this.oldParams = oldParams;
        this.newParams = newParams;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        for (var i = 0; i < oldParams.Count; i++)
        {
          if (oldParams[i] == node)
          {
            return newParams[i];
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
      lock (root)
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
      }

    }

    private bool HasCustomMappingForMember(PropertyOrFieldInfo member)
    {
      var match = this.Members.FirstOrDefault(m => m.Equals(member));

      return match != null;
    }

    private CustomMapping GetCustomMappingForMember(PropertyOrFieldInfo member)
    {

      if (!this.HasCustomMappingForMember(member))
      {
        foreach (var cm in this.CustomMappings)
        {
          var mapping = cm.GetCustomMappingForMember(member);

          if (mapping == null)
          {
            return mapping;
          }
        }
        return null;
      }
      else
      {
        return this;
      }


    }

    public void AddExpressionForMember(PropertyOrFieldInfo member, Expression expression)
    {
      var cm = GetCustomMappingForMember(member);

      if (cm != null)
      {
        lock (cm)
        {
          cm.Members.Add(new MemberExpressionTuple
          {
            Member = member,
            Expression = expression
          });
        }
      }
      else
      {

      }
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

        var argAsNewExpression = argument as NewExpression;

        if (argAsNewExpression != null && argAsNewExpression.Members != null)
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
          var cm = GetCustomMappingFromNewExpression(memberExpression.Member.PropertyOrFieldType, (NewExpression)argument);

          if (cm != null)
          {
            newMapping.CustomMappings.Add(cm);
          }
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
