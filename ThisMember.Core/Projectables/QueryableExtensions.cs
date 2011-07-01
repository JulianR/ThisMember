using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Projectables;

namespace System.Linq
{
  public static class QueryableExtensions
  {
    public static IProjectable<T> AsProjectable<T>(this IQueryable<T> query)
    {
      return new QueryableProjectable<T>(query);
    }

    public static ISingularProjectable<T> AsSingularProjectable<T>(this IQueryable<T> query)
    {
      return new SingularQueryableProjectable<T>(query);
    }

    public static IOptionalProjectable<T> AsOptionalProjectable<T>(this IQueryable<T> query)
    {
      return new OptionalQueryableProjectable<T>(query);
    }

    public static IOptionalProjectable<T> AsCollectionProjectable<T>(this IQueryable<T> query)
    {
      return new OptionalQueryableProjectable<T>(query);
    }
  }
}
