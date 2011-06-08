using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using ThisMember.Core.Exceptions;

namespace ThisMember.Core
{
  public class DefaultMappingStrategy : IMappingStrategy
  {
    private readonly Dictionary<TypePair, ProposedTypeMapping> mappingCache = new Dictionary<TypePair, ProposedTypeMapping>();

    private readonly Dictionary<TypePair, CustomMapping> customMappingCache = new Dictionary<TypePair, CustomMapping>();


    private readonly byte[] syncRoot = new byte[0];

    private readonly IMemberMapper mapper;

    public DefaultMappingStrategy(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    private Stack<TypePair> _typeStack = new Stack<TypePair>();

    private ProposedTypeMapping GetTypeMapping(TypePair pair, MappingOptions options = null, CustomMapping customMapping = null)
    {
      if (!_typeStack.Contains(pair))
      {
        _typeStack.Push(pair);
      }
      else if (mapper.Options.Safety.IfRecursiveRelationshipIsDetected == RecursivePropertyOptions.ThrowIfRecursionIsDetected)
      {
        throw new RecursiveRelationshipException(pair);
      }
      else
      {
        return null;
      }


      var typeMapping = new ProposedTypeMapping();

      typeMapping.SourceMember = null;
      typeMapping.DestinationMember = null;

      Type destinationType, sourceType;

      if (typeof(IEnumerable).IsAssignableFrom(pair.DestinationType))
      {
        destinationType = CollectionTypeHelper.GetTypeInsideEnumerable(pair.DestinationType);
      }
      else
      {
        destinationType = pair.DestinationType;
      }

      if (typeof(IEnumerable).IsAssignableFrom(pair.SourceType))
      {
        sourceType = CollectionTypeHelper.GetTypeInsideEnumerable(pair.SourceType);
      }
      else
      {
        sourceType = pair.SourceType;
      }



      var destinationProperties = (from p in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   where p.CanWrite && !p.GetIndexParameters().Any()
                                   select (PropertyOrFieldInfo)p)
                                   .Union(from f in destinationType.GetFields()
                                          where !f.IsStatic
                                          select (PropertyOrFieldInfo)f);


      var sourceProperties = (from p in sourceType.GetProperties()
                              where p.CanRead && !p.GetIndexParameters().Any()
                              select (PropertyOrFieldInfo)p)
                              .Union(from f in sourceType.GetFields()
                                     where !f.IsStatic
                                     select (PropertyOrFieldInfo)f)
                              .ToDictionary(k => k.Name);

      foreach (var destinationProperty in destinationProperties)
      {
        if (mapper.Options.Conventions.IgnoreMemberAttributeShouldBeRespected)
        {
          var ignoreAttribute = destinationProperty.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).SingleOrDefault() as IgnoreMemberAttribute;

          if (ignoreAttribute != null)
          {
            var ignore = true;

            if (!string.IsNullOrEmpty(ignoreAttribute.Profile))
            {
              ignore &= ignoreAttribute.Profile == mapper.Profile;
            }

            if (ignoreAttribute.WhenSourceTypeIs != null)
            {
              ignore &= ignoreAttribute.WhenSourceTypeIs == sourceType;
            }

            if (ignore) continue;
          }
        }

        PropertyOrFieldInfo sourceProperty;

        Expression customExpression = null;

        if (customMapping != null)
        {
          customExpression = customMapping.GetExpressionForMember(destinationProperty);
        }


        if (!sourceProperties.TryGetValue(destinationProperty.Name, out sourceProperty)
          && customExpression == null
          && mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember
          && !mapper.Options.Conventions.AutomaticallyFlattenHierarchies)
        {
          typeMapping.IncompatibleMappings.Add(destinationProperty);
        }
        else if (mapper.Options.Conventions.AutomaticallyFlattenHierarchies)
        {
          throw new NotImplementedException("Sorry, this hasn't been implemented yet");
        }

        Type nullableType = null;

        if (sourceProperty != null && sourceProperty.PropertyOrFieldType.IsNullableValueType())
        {
          nullableType = sourceProperty.PropertyOrFieldType.GetGenericArguments().Single();
        }

        var canUseSimpleTypeMapping = sourceProperty != null;

        if (canUseSimpleTypeMapping)
        {
          canUseSimpleTypeMapping &= (destinationProperty.PropertyOrFieldType.IsAssignableFrom(sourceProperty.PropertyOrFieldType))
            || (sourceProperty.PropertyOrFieldType.IsNullableValueType() && destinationProperty.PropertyOrFieldType.IsAssignableFrom(nullableType));


          if (pair.SourceType == pair.DestinationType && mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
          {
            canUseSimpleTypeMapping &= sourceProperty.PropertyOrFieldType.IsValueType || sourceProperty.PropertyOrFieldType == typeof(string);
          }
        }


        if (canUseSimpleTypeMapping)
        {

          if (options != null)
          {
            var option = new MemberOption();

            options(sourceProperty, destinationProperty, option);

            switch (option.State)
            {
              case MemberOptionState.Ignored:
                continue;
            }

          }

          typeMapping.ProposedMappings.Add
          (
            new ProposedMemberMapping
            {
              SourceMember = sourceProperty,
              DestinationMember = destinationProperty
            }
          );
        }
        else if (sourceProperty != null)
        {

          if (typeof(IEnumerable).IsAssignableFrom(sourceProperty.PropertyOrFieldType)
            && typeof(IEnumerable).IsAssignableFrom(destinationProperty.PropertyOrFieldType))
          {

            var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(sourceProperty.PropertyOrFieldType);
            var typeOfDestinationEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(destinationProperty.PropertyOrFieldType);

            if (typeOfDestinationEnumerable == typeOfSourceEnumerable)
            {

              typeMapping.ProposedTypeMappings.Add(
                new ProposedTypeMapping
              {
                DestinationMember = destinationProperty,
                SourceMember = sourceProperty,
                ProposedMappings = new List<ProposedMemberMapping>()
              });

            }
            else
            {
              var complexPair = new TypePair(typeOfSourceEnumerable, typeOfDestinationEnumerable);

              var complexTypeMapping = GetComplexTypeMapping(complexPair, options, customMapping);

              if (complexTypeMapping != null)
              {
                complexTypeMapping = complexTypeMapping.Clone();

                complexTypeMapping.DestinationMember = destinationProperty;
                complexTypeMapping.SourceMember = sourceProperty;

                CustomMapping customMappingForType;

                customMappingCache.TryGetValue(complexPair, out customMappingForType);

                complexTypeMapping.CustomMapping = customMappingForType;

                typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
              }
            }
          }
          else
          {
            var complexPair = new TypePair(sourceProperty.PropertyOrFieldType, destinationProperty.PropertyOrFieldType);

            var complexTypeMapping = GetComplexTypeMapping(complexPair, options, customMapping);

            if (complexTypeMapping != null)
            {

              complexTypeMapping = complexTypeMapping.Clone();

              complexTypeMapping.DestinationMember = destinationProperty;
              complexTypeMapping.SourceMember = sourceProperty;

              CustomMapping customMappingForType;

              customMappingCache.TryGetValue(complexPair, out customMappingForType);

              complexTypeMapping.CustomMapping = customMappingForType;

              typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
            }
          }
        }
        else if (customExpression != null)
        {
          typeMapping.ProposedMappings.Add
          (
            new ProposedMemberMapping
            {
              SourceMember = null,
              DestinationMember = destinationProperty
            }
          );
        }
      }

      lock (syncRoot)
      {
        mappingCache[pair] = typeMapping;
      }

      _typeStack.Pop();

      return typeMapping;
    }

    private ProposedTypeMapping GetComplexTypeMapping(TypePair complexPair, MappingOptions options, CustomMapping customMapping, bool skipCache = false)
    {
      ProposedTypeMapping complexTypeMapping;
      lock (syncRoot)
      {
        if (skipCache || !mappingCache.TryGetValue(complexPair, out complexTypeMapping))
        {
          ProposedMap proposedMap;
          if (mapper.MapRepository != null && mapper.MapRepository.TryGetMap(mapper, options, complexPair, out proposedMap))
          {
            complexTypeMapping = proposedMap.ProposedTypeMapping;
          }
          else
          {
            complexTypeMapping = GetTypeMapping(complexPair, options, customMapping);
          }
        }
      }
      return complexTypeMapping;
    }

    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMappingExpression = null)
    {
      var map = new ProposedMap<TSource, TDestination>(this.mapper);

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      map.MapGenerator = mapper.MapGenerator;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      CustomMapping customMapping = null;

      if (customMappingExpression != null)
      {
        customMapping = CustomMapping.GetCustomMapping(typeof(TDestination), customMappingExpression);
        customMappingCache[pair] = customMapping;
      }

      //var mapping = GetTypeMapping(pair, options, customMapping);

      var mapping = GetComplexTypeMapping(pair, options, customMapping, true);


      if (mapping.CustomMapping == null)
      {
        mapping.CustomMapping = customMapping;
      }

      map.ProposedTypeMapping = mapping;

      return map;
    }

    public ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null, LambdaExpression customMappingExpression = null)
    {

      var map = new ProposedMap(this.mapper);

      map.MapGenerator = mapper.MapGenerator;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      CustomMapping customMapping = null;

      if (customMappingExpression != null)
      {
        customMapping = CustomMapping.GetCustomMapping(pair.DestinationType, customMappingExpression);
        customMappingCache[pair] = customMapping;
      }

      //var mapping  = GetTypeMapping(pair, options, customMapping);

      var mapping = GetComplexTypeMapping(pair, options, customMapping, true);

      if (mapping.CustomMapping == null)
      {
        mapping.CustomMapping = customMapping;
      }

      map.ProposedTypeMapping = mapping;

      return map;

    }

    public void ClearMapCache()
    {
      this.mappingCache.Clear();
    }
  }
}
