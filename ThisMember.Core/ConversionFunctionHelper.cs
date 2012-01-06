using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;

namespace ThisMember.Core
{
  internal static class ConversionFunctionHelper
  {
    internal static LambdaExpression Bind(Type sourceType, PropertyOrFieldInfo member, LambdaExpression conversion, LambdaExpression customMapping = null)
    {
      var parameterToReplace = conversion.Parameters.Single();

      var newParameter = Expression.Parameter(sourceType, "p");

      var accessMember = Expression.MakeMemberAccess(newParameter, member);

      var visitor = new ParameterVisitor(parameterToReplace, accessMember);


      var newBody= visitor.Visit(conversion.Body);

      return Expression.Lambda(newBody, newParameter);

    }
    private class ParameterVisitor : ExpressionVisitor
    {
      private ParameterExpression param;
      private MemberExpression member;

      public ParameterVisitor(ParameterExpression param, MemberExpression member)
      {
        this.param = param;
        this.member = member;
      }

      protected override Expression VisitParameter(ParameterExpression node)
      {
        if (node == this.param)
        {
          return this.member;
        }

        return base.VisitParameter(node);
      }
    }
  }
}
