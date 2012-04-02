using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using ThisMember.Core.Options;

namespace ThisMember.Core
{

  public enum MemberOptionState
  {
    Default,
    Ignored
  }

  public class MemberOption
  {

    public MemberOptionState State { get; private set; }

    public PropertyOrFieldInfo Source { get; set; }

    public PropertyOrFieldInfo Destination { get; set; }

    public LambdaExpression ConversionFunction { get; private set; }

    public MemberOption(PropertyOrFieldInfo source, PropertyOrFieldInfo destination)
    {
      this.Source = source;
      this.Destination = destination;
    }

    public void MapProperty(PropertyOrFieldInfo source, PropertyOrFieldInfo destination)
    {
      this.Source = source;
      this.Destination = destination;
    }

    public void Convert<TSource, TDestination>(Expression<Func<TSource, TDestination>> conversion)
    {
      Convert(((LambdaExpression)conversion));
    }

    public void Convert(LambdaExpression conversion)
    {
      var args = conversion.Parameters;

      if (args.Count != 1)
      {
        throw new InvalidOperationException("Conversion function must take one argument");
      }

      var arg = args[0];

      if (Destination == null)
      {
        throw new InvalidOperationException("No Destination member defined");
      }

      if (!Destination.PropertyOrFieldType.IsAssignableFrom(conversion.ReturnType))
      {
        throw new InvalidOperationException("Invalid return type for conversion function");
      }

      ConversionFunction = conversion;
    }

    public void IgnoreMember()
    {
      State = MemberOptionState.Ignored;
    }
  }
}
