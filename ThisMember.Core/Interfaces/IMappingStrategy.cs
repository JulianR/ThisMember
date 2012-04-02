using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;
using System.Linq.Expressions;
using ThisMember.Core.Options;

namespace ThisMember.Core.Interfaces
{

  public delegate void MemberOptions(MappingContext context, MemberOption option);

  public interface IMappingStrategy
  {
    ProposedMap CreateMapProposal(TypePair pair, MemberOptions options = null, LambdaExpression customMapping = null, params Type[] parameters);

    ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MemberOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(MemberOptions options = null, Expression<Func<TSource, TParam, object>> customMapping = null);

    void ClearMapCache();

    IMemberProviderFactory MemberProviderFactory { get; set; }
  }
}
