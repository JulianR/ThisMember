using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Projectables
{
  internal static class ProjectableMethods
  {
    public static IQueryable<TResult> GetProjection<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return query.Select(projection);
    }

    public static TResult First<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).First();
    }

    public static TResult Single<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).Single();
    }

    public static T First<T>(IQueryable<T> query)
    {
      return query.First();
    }

    public static T Single<T>(IQueryable<T> query)
    {
      return query.Single();
    }

    public static TResult FirstOrDefault<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).FirstOrDefault();
    }

    public static TResult SingleOrDefault<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).SingleOrDefault();
    }

    public static T FirstOrDefault<T>(IQueryable<T> query)
    {
      return query.FirstOrDefault();
    }

    public static T SingleOrDefault<T>(IQueryable<T> query)
    {
      return query.SingleOrDefault();
    }

    public static List<TResult> ToList<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).ToList();
    }

    public static List<T> ToList<T>(IQueryable<T> query)
    {
      return query.ToList();
    }

    public static TResult[] ToArray<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection)
    {
      return GetProjection(query, projection).ToArray();
    }

    public static T[] ToArray<T>(IQueryable<T> query)
    {
      return query.ToArray();
    }

    public static int Count<T>(IQueryable<T> query)
    {
      return query.Count();
    }
  }

  public abstract class QueryableProjectableBase<T>
  {
    protected readonly IQueryable<T> query;

    protected QueryableProjectableBase(IQueryable<T> query)
    {
      this.query = query;
    }
  }

  public class SingularQueryableProjectable<T> : QueryableProjectableBase<T>, ISingularProjectable<T>
  {

    public SingularQueryableProjectable(IQueryable<T> query)
      : base(query)
    { }

    public TResult First<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.First(query, projection);
    }


    public TResult Single<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.Single(query, projection);
    }


    public T First()
    {
      return ProjectableMethods.First(query);
    }

    public T Single()
    {
      return ProjectableMethods.Single(query);
    }
  }

  public class OptionalQueryableProjectable<T> : QueryableProjectableBase<T>, IOptionalProjectable<T>
  {

    public OptionalQueryableProjectable(IQueryable<T> query)
      : base(query)
    { }

    public TResult FirstOrDefault<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.FirstOrDefault(query, projection);
    }

    public TResult SingleOrDefault<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.SingleOrDefault(query, projection);
    }

    public T FirstOrDefault()
    {
      return ProjectableMethods.FirstOrDefault(query);
    }

    public T SingleOrDefault()
    {
      return ProjectableMethods.SingleOrDefault(query);
    }
  }

  public class CollectionQueryableProjectable<T> : QueryableProjectableBase<T>, ICollectionProjectable<T>
  {

    public CollectionQueryableProjectable(IQueryable<T> query)
      : base(query)
    { }

    public List<TResult> ToList<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.ToList(query, projection);
    }

    public List<T> ToList()
    {
      return ProjectableMethods.ToList(query);
    }

    public TResult[] ToArray<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.ToArray(query, projection);
    }

    public T[] ToArray()
    {
      return ProjectableMethods.ToArray(query);
    }

    public int Count()
    {
      return ProjectableMethods.Count(query);
    }

  }

  public class QueryableProjectable<T> : QueryableProjectableBase<T>, IProjectable<T>
  {
    public QueryableProjectable(IQueryable<T> query)
      : base(query)
    { }

    public TResult First<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.First(query, projection);
    }


    public TResult Single<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.Single(query, projection);
    }


    public T First()
    {
      return ProjectableMethods.First(query);
    }

    public T Single()
    {
      return ProjectableMethods.Single(query);
    }

    public TResult FirstOrDefault<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.FirstOrDefault(query, projection);
    }

    public TResult SingleOrDefault<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.SingleOrDefault(query, projection);
    }

    public T FirstOrDefault()
    {
      return ProjectableMethods.FirstOrDefault(query);
    }

    public T SingleOrDefault()
    {
      return ProjectableMethods.SingleOrDefault(query);
    }

    public List<TResult> ToList<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.ToList(query, projection);
    }

    public List<T> ToList()
    {
      return ProjectableMethods.ToList(query);
    }

    public TResult[] ToArray<TResult>(Expression<Func<T, TResult>> projection)
    {
      return ProjectableMethods.ToArray(query, projection);
    }

    public T[] ToArray()
    {
      return ProjectableMethods.ToArray(query);
    }

    public int Count()
    {
      return ProjectableMethods.Count(query);
    }
  }

}
