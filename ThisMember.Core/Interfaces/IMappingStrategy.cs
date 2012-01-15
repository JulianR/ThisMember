using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;
using System.Linq.Expressions;

namespace ThisMember.Core.Interfaces
{

  public delegate void MappingOptions(PropertyOrFieldInfo source, PropertyOrFieldInfo destination, MemberOption option, int depth);

  public interface IMappingStrategy
  {
    ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null, LambdaExpression customMapping = null, params Type[] parameters);

    ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null);

    ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(MappingOptions options = null, Expression<Func<TSource, TParam, object>> customMapping = null);

    void ClearMapCache();

    IMemberProviderFactory MemberProviderFactory { get; set; }
  }
}
