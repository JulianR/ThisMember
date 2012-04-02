using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core
{
  public class DebugInformation
  {
    public LambdaExpression MappingExpression { get; set; }
  }

  public class DebugVisualizer : ExpressionVisitor
  {
    private SerializationStringBuilder sb = new SerializationStringBuilder();

    protected override Expression VisitBlock(BlockExpression node)
    {
      sb.AppendLine("{");
      sb.IncreaseIdent();
      var ex = base.VisitBlock(node);
      sb.DecreaseIndent();
      sb.AppendLine("}");
      return ex;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      sb.Append("Binary");
      return node;
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
      sb.Append("Catch");
      return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      sb.Append("Constant");
      return node;
    }

    protected override Expression VisitDefault(DefaultExpression node)
    {
      sb.Append("Default");
      return node;
    }

    protected override ElementInit VisitElementInit(ElementInit node)
    {
      sb.Append("ElementInit");
      return node;
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
      sb.Append("Index");
      return node;
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
      sb.Append("Invocation");
      return node;
    }

    protected override Expression VisitLabel(LabelExpression node)
    {
      sb.Append("Label");
      return node;
    }

    protected override LabelTarget VisitLabelTarget(LabelTarget node)
    {
      sb.Append("LabelTarget");
      return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {

      sb.Append(node.Name);

      return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Object == null)
      {
        sb.Append(node.Method.DeclaringType.Name + "." + node.Method.Name + "(");

        foreach (var arg in node.Arguments)
        {
          Visit(arg);
        }

        sb.Append(")");
      }
      else
      {
        Visit(node.Object);
        sb.Append("." + node.Method.Name + "(");

        foreach (var arg in node.Arguments)
        {
          Visit(arg);
        }

        sb.Append(")");
      }

      sb.Append(node.Method.Name + "()");

      return node;
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
      sb.Append("if( ");

      Visit(node.Test);

      sb.Append(" )");

      return base.VisitConditional(node);
    }

    public override string ToString()
    {
      return sb.ToString();
    }




    private class SerializationStringBuilder
    {
      private readonly StringBuilder builder;

      public SerializationStringBuilder()
      {
        builder = new StringBuilder(200);
      }


      private int _currentIndent;

      public void IncreaseIdent()
      {
        _currentIndent += 2;
      }

      public void DecreaseIndent()
      {
        _currentIndent -= 2;
      }

      public StringBuilder Append(string s)
      {
        return builder.Append(s);
      }

      public StringBuilder AppendFormat(string s)
      {
        return builder.AppendFormat(s);
      }

      public StringBuilder AppendLine()
      {
        builder.Append(new string(' ', _currentIndent));
        return builder.AppendLine();
      }

      public StringBuilder AppendLine(string s)
      {
        builder.Append(new string(' ', _currentIndent));
        return builder.AppendLine(s);
      }

      public StringBuilder AppendLineFormat(string str, params object[] args)
      {
        builder.Append(new string(' ', _currentIndent));
        return builder.AppendFormat(str, args).AppendLine();
      }

      public override string ToString()
      {
        return builder.ToString();
      }
    }
  }
}
