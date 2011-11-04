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
  internal class DefaultMappingStrategy : IMappingStrategy
  {
    private readonly Dictionary<TypePair, ProposedTypeMapping> mappingCache = new Dictionary<TypePair, ProposedTypeMapping>();

    private readonly Dictionary<TypePair, CustomMapping> customMappingCache = new Dictionary<TypePair, CustomMapping>();

    private readonly IMemberMapper mapper;

    public DefaultMappingStrategy(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMappingExpression = null)
    {
      var processor = new StrategyProcessor(mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal<TSource, TDestination>(options, customMappingExpression);
    }

    public ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(MappingOptions options = null, Expression<Func<TSource, TParam, object>> customMappingExpression = null)
    {
      var processor = new StrategyProcessor(mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal<TSource, TDestination, TParam>(options, customMappingExpression);
    }

    public ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null, LambdaExpression customMappingExpression = null, params Type[] parameters)
    {
      var processor = new StrategyProcessor(mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal(pair, options, customMappingExpression, parameters);
    }

    public void ClearMapCache()
    {
      this.mappingCache.Clear();
      this.customMappingCache.Clear();
    }

    private class StrategyProcessor
    {
      private readonly IMemberMapper mapper;

      private readonly Dictionary<TypePair, ProposedTypeMapping> mappingCache;

      private readonly Dictionary<TypePair, CustomMapping> customMappingCache;

      public StrategyProcessor(IMemberMapper mapper, Dictionary<TypePair, ProposedTypeMapping> mappingCache, Dictionary<TypePair, CustomMapping> customMappingCache)
      {
        this.mapper = mapper;
        this.mappingCache = mappingCache;
        this.customMappingCache = customMappingCache;
      }

      private Stack<TypePair> _typeStack = new Stack<TypePair>();

      private ProposedTypeMapping GetTypeMapping(int currentDepth, TypePair pair, MappingOptions options = null, CustomMapping customMapping = null)
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

        var memberProvider = new DefaultMemberProvider(sourceType, destinationType, mapper);

        foreach (var mapping in GetTypeMembers(memberProvider, options, currentDepth))
        {
          var destinationMember = mapping.Destination;

          if (memberProvider.IsMemberIgnored(sourceType, destinationMember))
          {
            continue;
          }

          Expression customExpression = null;

          if (customMapping != null)
          {
            customExpression = customMapping.GetExpressionForMember(destinationMember);
          }

          var sourceMember = mapping.Source;

          if (HasNoSourceMember(customExpression, sourceMember) || !destinationMember.CanWrite)
          {
            typeMapping.IncompatibleMappings.Add(destinationMember);
          }
          else if (mapper.Options.Conventions.AutomaticallyFlattenHierarchies)
          {
            throw new NotImplementedException("Sorry, this hasn't been implemented yet");
          }

          var nullableType = TryGetNullableType(sourceMember);

          var canUseSimpleTypeMapping = CanUseDirectAssignment(pair, destinationMember, sourceMember, nullableType);

          if (canUseSimpleTypeMapping || customExpression != null)
          {
            typeMapping.ProposedMappings.Add
            (
              new ProposedMemberMapping
              {
                SourceMember = sourceMember,
                DestinationMember = destinationMember
              }
            );
          }
          else if (sourceMember != null)
          {

            if (AreMembersIEnumerable(destinationMember, sourceMember))
            {
              GenerateEnumerableMapping(currentDepth, options, customMapping, typeMapping, destinationMember, sourceMember);
            }
            else
            {
              GenerateComplexTypeMapping(currentDepth, options, customMapping, typeMapping, destinationMember, sourceMember);
            }
          }
          else if (customExpression != null)
          {
            typeMapping.ProposedMappings.Add
            (
              new ProposedMemberMapping
              {
                SourceMember = null,
                DestinationMember = destinationMember
              }
            );
          }
        }

        if (!typeMapping.DoNotCache)
        {
          lock (mappingCache)
          {
            mappingCache[pair] = typeMapping;
          }
        }

        _typeStack.Pop();

        return typeMapping;
      }

      private class SourceDestinationMapping
      {
        public PropertyOrFieldInfo Source { get; set; }
        public PropertyOrFieldInfo Destination { get; set; }
      }

      private static IEnumerable<SourceDestinationMapping> GetTypeMembers(DefaultMemberProvider memberProvider, MappingOptions options, int currentDepth)
      {
        var destinationMembers = memberProvider.GetDestinationMembers();

        foreach (var destinationMember in destinationMembers)
        {
          var destination = destinationMember;
          var sourceMember = memberProvider.GetMatchingSourceMember(destinationMember);

          if (options != null)
          {
            var option = new MemberOption();

            options(sourceMember, destinationMember, option, currentDepth);

            switch (option.State)
            {
              case MemberOptionState.Ignored:
                continue;
            }

            if (option.Source != null)
            {

              if (option.Source.DeclaringType != sourceMember.DeclaringType)
              {
                throw new InvalidOperationException("Cannot use member declared on another type.");
              }

              sourceMember = option.Source;
            }

            if (option.Destination != null)
            {

              if (option.Destination.DeclaringType != destination.DeclaringType)
              {
                throw new InvalidOperationException("Cannot use member declared on another type.");
              }

              destination = option.Destination;
            }

          }

          var mapping = new SourceDestinationMapping
          {
            Source = sourceMember,
            Destination = destination
          };

          yield return mapping;

        }
      }

      private static bool AreMembersIEnumerable(PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember)
      {
        return typeof(IEnumerable).IsAssignableFrom(sourceMember.PropertyOrFieldType)
                    && typeof(IEnumerable).IsAssignableFrom(destinationMember.PropertyOrFieldType);
      }

      private void GenerateComplexTypeMapping(int currentDepth, MappingOptions options, CustomMapping customMapping, ProposedTypeMapping typeMapping, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember)
      {
        var complexPair = new TypePair(sourceMember.PropertyOrFieldType, destinationMember.PropertyOrFieldType);

        var complexTypeMapping = GetComplexTypeMapping(currentDepth + 1, complexPair, options, customMapping);

        if (complexTypeMapping != null)
        {

          complexTypeMapping = complexTypeMapping.Clone();

          complexTypeMapping.DestinationMember = destinationMember;
          complexTypeMapping.SourceMember = sourceMember;

          CustomMapping customMappingForType;

          TryGetCustomMapping(complexPair, out customMappingForType);

          complexTypeMapping.CustomMapping = customMappingForType;

          typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
        }
        else
        {
          typeMapping.DoNotCache = true;
        }
      }

      private void GenerateEnumerableMapping(int currentDepth, MappingOptions options, CustomMapping customMapping, ProposedTypeMapping typeMapping, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember)
      {
        var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(sourceMember.PropertyOrFieldType);
        var typeOfDestinationEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(destinationMember.PropertyOrFieldType);

        var canAssignSourceItemsToDestination = CanAssignSourceItemsToDestination(mapper, destinationMember, sourceMember, typeOfSourceEnumerable, typeOfDestinationEnumerable);

        if (canAssignSourceItemsToDestination)
        {

          typeMapping.ProposedTypeMappings.Add(
            new ProposedTypeMapping
            {
              DestinationMember = destinationMember,
              SourceMember = sourceMember,
              ProposedMappings = new List<ProposedMemberMapping>()
            });

        }
        else
        {
          var complexPair = new TypePair(typeOfSourceEnumerable, typeOfDestinationEnumerable);

          var complexTypeMapping = GetComplexTypeMapping(currentDepth + 1, complexPair, options, customMapping);

          if (complexTypeMapping != null)
          {
            complexTypeMapping = complexTypeMapping.Clone();

            complexTypeMapping.DestinationMember = destinationMember;
            complexTypeMapping.SourceMember = sourceMember;

            CustomMapping customMappingForType;

            TryGetCustomMapping(complexPair, out customMappingForType);

            complexTypeMapping.CustomMapping = customMappingForType;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
          }
          else
          {
            typeMapping.DoNotCache = true;
          }
        }
      }

      private static bool CanAssignSourceItemsToDestination(IMemberMapper mapper, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember, Type typeOfSourceEnumerable, Type typeOfDestinationEnumerable)
      {
        if (typeOfDestinationEnumerable == typeOfSourceEnumerable)
        {

          if (typeOfSourceEnumerable.IsPrimitive || typeOfSourceEnumerable == typeof(string))
          {
            return true;
          }

          if (sourceMember.DeclaringType == destinationMember.DeclaringType && mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
          {
            return false;
          }

          return true;

        }
        return false;
      }

      private bool CanUseDirectAssignment(TypePair pair, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember, Type nullableType)
      {

        if (sourceMember == null)
        {
          return false;
        }

        if (!destinationMember.PropertyOrFieldType.IsAssignableFrom(sourceMember.PropertyOrFieldType))
        {
          if (sourceMember.PropertyOrFieldType.IsNullableValueType() && destinationMember.PropertyOrFieldType.IsAssignableFrom(nullableType))
          {
            return true;
          }
          else if (ConversionTypeHelper.AreConvertible(sourceMember.PropertyOrFieldType, destinationMember.PropertyOrFieldType))
          {
            return true;
          }
          else
          {
            return false;
          }
        }

        if (pair.SourceType == pair.DestinationType && mapper.Options.Conventions.MakeCloneIfDestinationIsTheSameAsSource)
        {
          if (sourceMember.PropertyOrFieldType.IsValueType || sourceMember.PropertyOrFieldType == typeof(string))
          {
            return true;
          }
          else
          {
            return false;
          }
        }

        return true;
      }

      private static Type TryGetNullableType(PropertyOrFieldInfo sourceMember)
      {
        Type nullableType = null;

        if (sourceMember != null && sourceMember.PropertyOrFieldType.IsNullableValueType())
        {
          nullableType = sourceMember.PropertyOrFieldType.GetGenericArguments().Single();
        }
        return nullableType;
      }

      private bool HasNoSourceMember(Expression customExpression, PropertyOrFieldInfo sourceMember)
      {
        return sourceMember == null
                  && customExpression == null
                  && mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember
                  && !mapper.Options.Conventions.AutomaticallyFlattenHierarchies;
      }

      private ProposedTypeMapping GetComplexTypeMapping(int currentDepth, TypePair complexPair, MappingOptions options, CustomMapping customMapping, bool skipCache = false)
      {
        if (complexPair.SourceType == complexPair.DestinationType)
        {
          var maxDepth = mapper.Options.Cloning.MaxCloneDepth;

          if (maxDepth.HasValue && currentDepth > maxDepth)
          {
            return null;
          }
        }
        else
        {
          var maxDepth = mapper.Options.Conventions.MaxDepth;

          if (maxDepth.HasValue && currentDepth > maxDepth)
          {
            return null;
          }

        }

        ProposedTypeMapping complexTypeMapping;

        if (skipCache || !mappingCache.TryGetValue(complexPair, out complexTypeMapping))
        {
          ProposedMap proposedMap;
          if (mapper.MapRepository != null && mapper.MapRepository.TryGetMap(mapper, options, complexPair, out proposedMap))
          {
            complexTypeMapping = proposedMap.ProposedTypeMapping;
          }
          else
          {
            complexTypeMapping = GetTypeMapping(currentDepth, complexPair, options, customMapping);
          }
        }

        return complexTypeMapping;
      }

      public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMappingExpression = null)
      {
        var map = new ProposedMap<TSource, TDestination>(this.mapper);

        var pair = new TypePair(typeof(TSource), typeof(TDestination));

        map.SourceType = pair.SourceType;
        map.DestinationType = pair.DestinationType;

        CustomMapping customMapping = null;

        //if (customMappingExpression != null)
        //{
        //  customMapping = CustomMapping.GetCustomMapping(typeof(TDestination), customMappingExpression);
        //  customMappingCache[pair] = customMapping;
        //}

        customMapping = GetCustomMappingFromExpression(pair, customMappingExpression, customMapping);


        TryGetCustomMapping(pair, out customMapping);

        var mapping = GetComplexTypeMapping(0, pair, options, customMapping, true);


        if (mapping.CustomMapping == null)
        {
          mapping.CustomMapping = customMapping;
        }

        map.ProposedTypeMapping = mapping;

        return map;
      }

      public ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(MappingOptions options = null, Expression<Func<TSource, TParam, object>> customMappingExpression = null)
      {
        var map = new ProposedMap<TSource, TDestination, TParam>(this.mapper);

        map.ParameterTypes.Add(typeof(TParam));

        var pair = new TypePair(typeof(TSource), typeof(TDestination));

        map.SourceType = pair.SourceType;
        map.DestinationType = pair.DestinationType;

        CustomMapping customMapping = null;

        //if (customMappingExpression != null)
        //{
        //  customMapping = CustomMapping.GetCustomMapping(typeof(TDestination), customMappingExpression);
        //  customMappingCache[pair] = customMapping;
        //}

        customMapping = GetCustomMappingFromExpression(pair, customMappingExpression, customMapping);


        TryGetCustomMapping(pair, out customMapping);

        var mapping = GetComplexTypeMapping(0, pair, options, customMapping, true);


        if (mapping.CustomMapping == null)
        {
          mapping.CustomMapping = customMapping;
        }

        map.ProposedTypeMapping = mapping;

        return map;
      }

      public ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null, LambdaExpression customMappingExpression = null, params Type[] parameters)
      {

        var map = new ProposedMap(this.mapper);

        foreach (var param in parameters)
        {
          map.ParameterTypes.Add(param);
        }

        map.SourceType = pair.SourceType;
        map.DestinationType = pair.DestinationType;

        CustomMapping customMapping = null;

        customMapping = GetCustomMappingFromExpression(pair, customMappingExpression, customMapping);

        TryGetCustomMapping(pair, out customMapping);

        var mapping = GetComplexTypeMapping(0, pair, options, customMapping, true);

        if (mapping.CustomMapping == null)
        {
          mapping.CustomMapping = customMapping;
        }

        map.ProposedTypeMapping = mapping;

        return map;

      }

      private CustomMapping GetCustomMappingFromExpression(TypePair pair, LambdaExpression customMappingExpression, CustomMapping customMapping)
      {
        lock (customMappingCache)
        {
          if (customMappingExpression != null)
          {
            customMapping = CustomMapping.GetCustomMapping(pair.DestinationType, customMappingExpression);
            customMappingCache[pair] = customMapping;
          }
        }
        return customMapping;
      }

      private int DistanceFromType(Type topLevelType, Type lowerLevelType, int distanceSoFar)
      {
        if (topLevelType == lowerLevelType)
        {
          return distanceSoFar;
        }
        else if (topLevelType.BaseType != null)
        {
          return DistanceFromType(topLevelType.BaseType, lowerLevelType, distanceSoFar + 1);
        }

        return -1;
      }

      private bool TryGetCustomMapping(TypePair pair, out CustomMapping customMapping)
      {
        customMappingCache.TryGetValue(pair, out customMapping);

        var matchingMappings = (from m in customMappingCache
                                where m.Key.DestinationType.IsAssignableFrom(pair.DestinationType)
                                && m.Key.DestinationType != pair.DestinationType
                                orderby DistanceFromType(pair.DestinationType, m.Key.DestinationType, 0) ascending
                                select m.Value).ToList();

        customMapping = customMapping ?? matchingMappings.FirstOrDefault();

        if (customMapping != null)
        {
          customMapping.CombineWithOtherCustomMappings(matchingMappings);

          return true;
        }
        else
        {
          return false;
        }
      }
    }
  }
}
