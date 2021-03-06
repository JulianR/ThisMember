﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using ThisMember.Core.Exceptions;
using ThisMember.Core.Fluent;
using ThisMember.Core.Options;
using System.Collections.Concurrent;

namespace ThisMember.Core
{
  /// <summary>
  /// A map proposed by a IMappingStrategy, that can be adjusted still, for example by adding 
  /// custom constructors and conditions.
  /// </summary>
  public class ProposedMap
  {
    private readonly Type sourceType, destinationType;

    public Type SourceType
    {
      get
      {
        return sourceType;
      }
    }

    public Type DestinationType
    {
      get
      {
        return destinationType;
      }
    }

    public IList<Type> ParameterTypes { get; private set; }

    protected readonly IMemberMapper mapper;

    protected readonly MapperOptions options;

    public ProposedMap(Type sourceType, Type destinationType, IMemberMapper mapper, MapperOptions options)
    {
      this.mapper = mapper;
      this.ParameterTypes = new List<Type>();
      this.options = options;
      this.sourceType = sourceType;
      this.destinationType = destinationType;
    }

    protected ConcurrentDictionary<Type, LambdaExpression> constructorCache = new ConcurrentDictionary<Type, LambdaExpression>();

    private static MemberMap CreateMemberMapInstance(Type source, Type destination, Delegate mappingFunction)
    {
      var type = typeof(MemberMap<,>).MakeGenericType(source, destination);

      return (MemberMap)Activator.CreateInstance(type,  mappingFunction);
    }

    private static Projection CreateProjectionInstance(Type source, Type destination, LambdaExpression expression)
    {
      var type = typeof(Projection<,>).MakeGenericType(source, destination);

      return (Projection)Activator.CreateInstance(type, expression);
    }

    public virtual MemberMap FinalizeMap()
    {
      EnsureNoInvalidMappings();
  
      var generator = this.mapper.MapGeneratorFactory.GetGenerator(this.mapper, this, this.options);

      var mappingFunction = generator.GenerateMappingFunction();

      var map = CreateMemberMapInstance(this.SourceType, this.DestinationType, mappingFunction);
      
      map.DebugInformation = generator.DebugInformation;

      mapper.RegisterMap(map);

      return map;
    }

    public virtual Projection FinalizeProjection()
    {
      EnsureNoInvalidMappings();

      var generator = this.mapper.ProjectionGeneratorFactory.GetGenerator(this.mapper);

      var expression = generator.GetProjection(this);

      var projection = CreateProjectionInstance(this.SourceType, this.DestinationType, expression);

      mapper.RegisterProjection(projection);

      return projection;
    }

    public void ValidateMapping()
    {
      EnsureNoInvalidMappings();
    }

    protected void EnsureNoInvalidMappings()
    {
      var invalidPropertyMappings = new List<PropertyOrFieldInfo>();

      EnsureNoInvalidMappings(invalidPropertyMappings, this.ProposedTypeMapping);

      if (invalidPropertyMappings.Any())
      {
        var sb = new StringBuilder();

        sb.AppendLine("The following properties could not be mapped: ");

        foreach (var property in invalidPropertyMappings)
        {
          if (!property.CanWrite)
          {
            sb.AppendLine(property.DeclaringType.Name + "." + property.Name + " (private setter), ");
          }
          else
          {
            sb.AppendLine(property.DeclaringType.Name + "." + property.Name + ", ");
          }
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

    /// <summary>
    /// Configures how a certain type should be instantiated.
    /// </summary>
    /// <param name="constructor">The expression that describes the type construction. 
    /// Should be a lambda returning the type T.</param>
    public ProposedMap WithConstructorFor<T>(LambdaExpression constructor)
    {
      constructorCache[typeof(T)] = constructor;
      return this;
    }

    /// <summary>
    /// Configures how a certain type should be instantiated.
    /// </summary>
    /// <param name="constructor">The expression that describes the type construction. 
    /// Should be a lambda returning the type.</param>
    public ProposedMap WithConstructorFor(Type type, LambdaExpression constructor)
    {
      constructorCache[type] = constructor;
      return this;
    }

    /// <summary>
    /// Returns a custom constructor, if any, for a certain type.
    /// </summary>
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

    public ProposedMap(IMemberMapper mapper, MapperOptions options)
      : base(typeof(TSource), typeof(TDestination), mapper, options)
    {
    }

    public override MemberMap FinalizeMap()
    {
      EnsureNoInvalidMappings();

      var generator = this.mapper.MapGeneratorFactory.GetGenerator(this.mapper, this, this.options);

      var mappingFunction = (Func<TSource, TDestination, TDestination>)generator.GenerateMappingFunction();

      var map = new MemberMap<TSource, TDestination>(mappingFunction);

      map.DebugInformation = generator.DebugInformation;

      mapper.RegisterMap(map);

      return map;
    }

    public override Projection FinalizeProjection()
    {
      EnsureNoInvalidMappings();

      var generator = this.mapper.ProjectionGeneratorFactory.GetGenerator(this.mapper);

      var expression = (Expression<Func<TSource, TDestination>>)generator.GetProjection(this);

      var projection = new Projection<TSource, TDestination>(expression);

      mapper.RegisterProjection(projection);

      return projection;
    }

    /// <summary>
    /// Configures how a certain type should be instantiated.
    /// </summary>
    /// <param name="constructor">The expression that describes the type construction. 
    /// Should be a lambda returning the type.</param>
    public ProposedMap<TSource, TDestination> WithConstructorFor<T>(Expression<Func<TSource, TDestination, T>> constructor)
    {
      constructorCache.AddOrUpdate(typeof(T), constructor, (k, v) => constructor);
      return this;
    }

    private IMappingProposition GetMemberMappingForMember(ProposedTypeMapping mapping, PropertyOrFieldInfo member)
    {

      if (mapping.IncompatibleMappings.Contains(member))
      {
        return new IncompatibleMapping(member, mapping);
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

    /// <summary>
    /// Gets a member from the current mapping. If this member is not being mapped yet,
    /// then you can only ignore it and add a custom mapping to it.
    /// </summary>
    /// <typeparam name="TMemberType"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
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
        mapping = new IncompatibleMapping(memberExpression.Member);
      }

      return new MappingPropositionModifier<TSource, TDestination>(this, mapping);
    }

    private class IncompatibleMapping : IMappingProposition
    {
      private PropertyOrFieldInfo member;
      private ProposedTypeMapping mapping;

      public IncompatibleMapping(PropertyOrFieldInfo member, ProposedTypeMapping mapping = null)
      {
        this.mapping = mapping;
        this.member = member;
      }

      public bool Ignored
      {
        get
        {
          throw new MemberNotFoundException(member);
        }
        set
        {
          if (mapping != null)
          {
            mapping.IncompatibleMappings.Remove(member);
          }
        }
      }

      public LambdaExpression Condition
      {
        get
        {
          throw new MemberNotFoundException(member);
        }
        set
        {
          throw new MemberNotFoundException(member);
        }
      }

      public PropertyOrFieldInfo DestinationMember
      {
        get
        {
          return member;
        }
        set
        {
          member = value;
        }
      }

      public PropertyOrFieldInfo SourceMember
      {
        get
        {
          return null;
        }
        set
        {
          throw new NotImplementedException();
        }
      }
    }

  }

  public class ProposedMap<TSource, TDestination, TParam> : ProposedMap<TSource, TDestination>
  {

    public ProposedMap(IMemberMapper mapper, MapperOptions options)
      : base(mapper, options)
    {
    }

    public override MemberMap FinalizeMap()
    {
      EnsureNoInvalidMappings();
  
      var generator = this.mapper.MapGeneratorFactory.GetGenerator(this.mapper, this, this.options);

      var mappingFunction = (Func<TSource, TDestination, TParam, TDestination>)generator.GenerateMappingFunction();

      var map = new MemberMap<TSource, TDestination, TParam>(mappingFunction);

      map.DebugInformation = generator.DebugInformation;

      mapper.RegisterMap(map);

      return map;
    }
  }
}
