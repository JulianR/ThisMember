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

    public static IList<TResult> Page<T, TResult>(IQueryable<T> query, Expression<Func<T, TResult>> projection, int start = 0, int limit = -1)
    {
      query = query.Skip(start);

      if(limit > 0)
      {
        query = query.Take(limit);
      }

      return GetProjection(query, projection).ToList();

    }

    public static IList<T> Page<T>(IQueryable<T> query, int start = 0, int limit = -1)
    {
      query = query.Skip(start);

      if (limit > 0)
      {
        query = query.Take(limit);
      }

      return query.ToList();
    }

    public static Dictionary<TKey, TElement> ToDictionary<T, TKey, TElement>(IQueryable<T> query, Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
    {
      return query.ToDictionary(keySelector, elementSelector);
    }

    public static Dictionary<TKey, T> ToDictionary<T, TKey>(IQueryable<T> query, Func<T, TKey> keySelector)
    {
      return query.ToDictionary(keySelector);
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

    public ISingularProjectable<TResult> Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new SingularQueryableProjectable<TResult>(query.Select(projection));
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

    public IOptionalProjectable<TResult> Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new OptionalQueryableProjectable<TResult>(query.Select(projection));
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

    public IList<TResult> Page<TResult>(Expression<Func<T, TResult>> projection, int start = 0, int limit = -1)
    {
      return ProjectableMethods.Page(query, projection, start, limit);
    }

    public IList<T> Page(int start = 0, int limit = -1)
    {
      return ProjectableMethods.Page(query, start, limit);
    }

    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
    {
      return ProjectableMethods.ToDictionary<T, TKey, TElement>(query, keySelector, elementSelector);
    }

    public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector)
    {
      return ProjectableMethods.ToDictionary<T, TKey>(query, keySelector);
    }

    public ICollectionProjectable<TResult> Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new CollectionQueryableProjectable<TResult>(query.Select(projection));
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

    public IList<TResult> Page<TResult>(Expression<Func<T, TResult>> projection, int start = 0, int limit = -1)
    {
      return ProjectableMethods.Page(query, projection, start, limit);
    }

    public IList<T> Page(int start = 0, int limit = -1)
    {
      return ProjectableMethods.Page(query, start, limit);
    }

    public IProjectable<TResult> Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new QueryableProjectable<TResult>(query.Select(projection));
    }

    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
    {
      return ProjectableMethods.ToDictionary<T, TKey, TElement>(query, keySelector, elementSelector);
    }

    public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector)
    {
      return ProjectableMethods.ToDictionary<T, TKey>(query, keySelector);
    }

    ISingularProjectable<TResult> ISingularProjectable<T>.Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new SingularQueryableProjectable<TResult>(query.Select(projection)); 
    }

    IOptionalProjectable<TResult> IOptionalProjectable<T>.Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new OptionalQueryableProjectable<TResult>(query.Select(projection));
    }

    ICollectionProjectable<TResult> ICollectionProjectable<T>.Project<TResult>(Expression<Func<T, TResult>> projection)
    {
      return new CollectionQueryableProjectable<TResult>(query.Select(projection));
    }
  }
}
