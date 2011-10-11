using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ThisMember.Core.Projectables
{
  /// <summary>
  /// Represents a single, non-optional result from a projectable sequence.
  /// </summary>
  /// <typeparam name="TSource"></typeparam>
  public interface ISingularProjectable<TSource>
  {
    TResult First<TResult>(Expression<Func<TSource, TResult>> projection);

    TResult Single<TResult>(Expression<Func<TSource, TResult>> projection);

    TSource First();

    TSource Single();

    ISingularProjectable<TResult> Project<TResult>(Expression<Func<TSource, TResult>> projection);
  }

  /// <summary>
  /// Represents a single, optional result from a projectable sequence.
  /// </summary>
  /// <typeparam name="TSource"></typeparam>
  public interface IOptionalProjectable<TSource>
  {
    TResult FirstOrDefault<TResult>(Expression<Func<TSource, TResult>> projection);

    TResult SingleOrDefault<TResult>(Expression<Func<TSource, TResult>> projection);

    TSource FirstOrDefault();

    TSource SingleOrDefault();

    IOptionalProjectable<TResult> Project<TResult>(Expression<Func<TSource, TResult>> projection);
  }

  /// <summary>
  /// Represents a collection of results from a projectable sequence.
  /// </summary>
  /// <typeparam name="TSource"></typeparam>
  public interface ICollectionProjectable<TSource>
  {
    List<TResult> ToList<TResult>(Expression<Func<TSource, TResult>> projection);

    List<TSource> ToList();

    TResult[] ToArray<TResult>(Expression<Func<TSource, TResult>> projection);

    TSource[] ToArray();

    int Count();

    IList<TResult> Page<TResult>(Expression<Func<TSource, TResult>> projection, int start = 0, int limit = -1);

    IList<TSource> Page(int start = 0, int limit = -1);

    ICollectionProjectable<TResult> Project<TResult>(Expression<Func<TSource, TResult>> projection);

    Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector);

    Dictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector);
  }

  public interface IProjectable<TSource> : ISingularProjectable<TSource>, IOptionalProjectable<TSource>, ICollectionProjectable<TSource>
  {
  }
}
