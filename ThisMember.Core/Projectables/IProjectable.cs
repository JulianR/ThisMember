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
  }

  public interface IProjectable<TSource> : ISingularProjectable<TSource>, IOptionalProjectable<TSource>, ICollectionProjectable<TSource>
  {

  }
}
