using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using ThisMember.Core.Exceptions;

namespace ThisMember.Core
{
  public class ProposedMap
  {
    public Type SourceType { get; set; }
    public Type DestinationType { get; set; }

    //public CustomMapping CustomMapping { get; set; }

    public IMapGenerator MapGenerator { get; set; }

    private readonly IMemberMapper mapper;

    public ProposedMap(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    protected Dictionary<Type, LambdaExpression> constructorCache = new Dictionary<Type, LambdaExpression>();

    public MemberMap FinalizeMap()
    {
      EnsureNoInvalidMappings();

      var map = new MemberMap();

      map.SourceType = this.SourceType;
      map.DestinationType = this.DestinationType;
      map.MappingFunction = this.MapGenerator.GenerateMappingFunction(this);

      mapper.RegisterMap(map);

      return map;
    }

    private void EnsureNoInvalidMappings()
    {
      var invalidPropertyMappings = new List<PropertyOrFieldInfo>();

      EnsureNoInvalidMappings(invalidPropertyMappings, this.ProposedTypeMapping);

      if (invalidPropertyMappings.Any())
      {
        var sb = new StringBuilder();

        sb.AppendLine("The following properties could not be mapped: ");

        foreach (var property in invalidPropertyMappings)
        {
          sb.AppendLine(property.DeclaringType.Name + "." + property.Name + ", ");
        }

        throw new IncompatibleMappingException(sb.ToString());

      }
    }

    private void EnsureNoInvalidMappings(List<PropertyOrFieldInfo> properties, ProposedTypeMapping typeMapping)
    {
      properties.AddRange(typeMapping.IncompatibleMappings);

      foreach (var mapping in typeMapping.ProposedTypeMappings)
      {
        EnsureNoInvalidMappings(properties, mapping);
      }
    }

    public ProposedMap WithConstructorFor<T>(LambdaExpression constructor)
    {
      constructorCache[typeof(T)] = constructor;
      return this;
    }

    public ProposedMap WithConstructorFor(Type type, LambdaExpression constructor)
    {
      constructorCache[type] = constructor;
      return this;
    }

    public LambdaExpression GetConstructor(Type type)
    {
      LambdaExpression e;
      constructorCache.TryGetValue(type, out e);
      return e;
    }

    public ProposedTypeMapping ProposedTypeMapping { get; set; }

  }

  public class ProposedMap<TSource, TDestination> : ProposedMap
  {

    public ProposedMap(IMemberMapper mapper)
      : base(mapper)
    {
    }

    public ProposedMap<TSource, TDestination> AddMapping<TSourceReturn, TDestinationReturn>(Expression<Func<TSource, TSourceReturn>> source, Expression<Func<TDestination, TDestinationReturn>> destination) where TDestinationReturn : TSourceReturn
    {
      return this;
    }

    public ProposedMap<TSource, TDestination> WithConstructorFor<T>(Expression<Func<TSource, TDestination, T>> constructor)
    {
      constructorCache.Add(typeof(T), constructor);
      return this;
    }

    private IMappingProposition GetMemberMappingForMember(ProposedTypeMapping mapping, PropertyOrFieldInfo member)
    {

      if (mapping.IncompatibleMappings.Contains(member))
      {
        return new IncompatibleMapping(mapping, member);
      }

      foreach (var memberMapping in mapping.ProposedMappings)
      {
        if (memberMapping.DestinationMember != null && memberMapping.DestinationMember.Equals(member))
        {
          return memberMapping;
        }
      }

      IMappingProposition result = null;

      foreach (var typeMapping in mapping.ProposedTypeMappings)
      {

        if (typeMapping.DestinationMember != null && typeMapping.DestinationMember.Equals(member))
        {
          return typeMapping;
        }

        result = GetMemberMappingForMember(typeMapping, member);
      }

      return result;
    }

    public MappingPropositionModifier<TSource, TDestination> ForMember<TMemberType>(Expression<Func<TDestination, TMemberType>> expression)
    {
      var memberExpression = expression.Body as MemberExpression;

      if (memberExpression == null)
      {
        throw new ArgumentException("Expression must be of type MemberExpression");
      }

      var mapping = GetMemberMappingForMember(this.ProposedTypeMapping, memberExpression.Member);

      if (mapping == null)
      {
        throw new MemberNotFoundException(memberExpression.Member);
      }

      return new MappingPropositionModifier<TSource, TDestination>(this, mapping);
    }

    private class IncompatibleMapping : IMappingProposition
    {
      private PropertyOrFieldInfo _member;
      private ProposedTypeMapping _mapping;

      public IncompatibleMapping(ProposedTypeMapping mapping, PropertyOrFieldInfo member)
      {
        _mapping = mapping;
        _member = member;
      }

      public bool Ignored
      {
        get
        {
          throw new MemberNotFoundException(_member);
        }
        set
        {
          _mapping.IncompatibleMappings.Remove(_member);
        }
      }

      public LambdaExpression Condition
      {
        get
        {
          throw new MemberNotFoundException(_member);
        }
        set
        {
          throw new MemberNotFoundException(_member);
        }
      }
    }

  }
}
