using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using ThisMember.Core.Exceptions;
using ThisMember.Core.Options;

namespace ThisMember.Core
{
  internal class DefaultMappingStrategy : IMappingStrategy
  {
    // Cache for type mappings we have found so far
    private readonly Dictionary<TypePair, ProposedTypeMapping> mappingCache = new Dictionary<TypePair, ProposedTypeMapping>();

    // Cache for custom mappings 
    private readonly Dictionary<TypePair, CustomMapping> customMappingCache = new Dictionary<TypePair, CustomMapping>();

    private readonly IMemberMapper mapper;

    public DefaultMappingStrategy(IMemberMapper mapper)
    {
      this.mapper = mapper;
      this.MemberProviderFactory = new DefaultMemberProviderFactory();
    }

    public IMemberProviderFactory MemberProviderFactory { get; set; }

    public ProposedMap<TSource, TDestination> CreateMapProposal<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMappingExpression = null)
    {
      var processor = new StrategyProcessor(this, mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal<TSource, TDestination>(options, customMappingExpression);
    }

    public ProposedMap<TSource, TDestination, TParam> CreateMapProposal<TSource, TDestination, TParam>(MappingOptions options = null, Expression<Func<TSource, TParam, object>> customMappingExpression = null)
    {
      var processor = new StrategyProcessor(this, mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal<TSource, TDestination, TParam>(options, customMappingExpression);
    }

    public ProposedMap CreateMapProposal(TypePair pair, MappingOptions options = null, LambdaExpression customMappingExpression = null, params Type[] parameters)
    {
      var processor = new StrategyProcessor(this, mapper, mappingCache, customMappingCache);

      return processor.CreateMapProposal(pair, options, customMappingExpression, parameters);
    }

    public void ClearMapCache()
    {
      this.mappingCache.Clear();
      this.customMappingCache.Clear();
    }

    /// <summary>
    ///  Private class that is instantiated with every call to the strategy's CreateMap method.
    ///  This so we won't have to worry about statefulness of the strategy and thread-safety. 
    /// </summary>
    private class StrategyProcessor
    {
      private readonly IMemberMapper mapper;

      private readonly Dictionary<TypePair, ProposedTypeMapping> mappingCache;

      private readonly Dictionary<TypePair, CustomMapping> customMappingCache;

      private readonly DefaultMappingStrategy strategy;

      public StrategyProcessor(DefaultMappingStrategy strategy,  IMemberMapper mapper, Dictionary<TypePair, ProposedTypeMapping> mappingCache, Dictionary<TypePair, CustomMapping> customMappingCache)
      {
        this.mapper = mapper;
        this.mappingCache = mappingCache;
        this.customMappingCache = customMappingCache;
        this.strategy = strategy;
      }

      // Keep a stack to deal with recursive members on types
      private Stack<TypePair> typeStack = new Stack<TypePair>();

      /// <summary>
      /// Returns a type mapping of the TypePair you pass in. 
      /// </summary>
      /// <returns></returns>
      private ProposedTypeMapping GetTypeMapping(int currentDepth, TypePair pair, MappingOptions options = null, CustomMapping customMapping = null)
      {
        if (!typeStack.Contains(pair))
        {
          typeStack.Push(pair);
        }
        else if (mapper.Options.Safety.IfRecursiveRelationshipIsDetected == RecursivePropertyOptions.ThrowIfRecursionIsDetected)
        {
          // Oh noes, recursion!
          throw new RecursiveRelationshipException(pair);
        }
        // if it's a recursive relationship, by default we return null which is handled after the method returns
        else 
        {
          return null;
        }

        var typeMapping = new ProposedTypeMapping();

        typeMapping.SourceMember = null;
        typeMapping.DestinationMember = null;

        Type destinationType, sourceType;

        // If it's an enumerable type (List<>, IEnumerable<>, etc) then we're currently interested
        // in the type 'inside' the enumerable.
        if (CollectionTypeHelper.IsEnumerable(pair.DestinationType))
        {
          destinationType = CollectionTypeHelper.GetTypeInsideEnumerable(pair.DestinationType);
        }
        else
        {
          destinationType = pair.DestinationType;
        }

        // Same here.
        if (CollectionTypeHelper.IsEnumerable(pair.SourceType))
        {
          sourceType = CollectionTypeHelper.GetTypeInsideEnumerable(pair.SourceType);
        }
        else
        {
          sourceType = pair.SourceType;
        }

        // The memberprovider is responsible for linking a destination member with a source member
        var memberProvider = this.strategy.MemberProviderFactory.GetMemberProvider(sourceType, destinationType, mapper);

        // Loop through all members it could find
        foreach (var mapping in GetTypeMembers(memberProvider, options, currentDepth))
        {
          var destinationMember = mapping.Destination;

          // Does the memberprovider see any reason to ignore this member?
          if (memberProvider.IsMemberIgnored(sourceType, destinationMember))
          {
            continue;
          }

          Expression customExpression = null;

          // Try to extract an expression that was supplied for this destination member
          if (customMapping != null)
          {
            customExpression = customMapping.GetExpressionForMember(destinationMember);
          }

          var sourceMember = mapping.Source;

          // Did the user supply a function to transform the source member's value?
          if (mapping.ConversionFunction != null)
          {
            // If no custom mapping yet, then we need to create one
            // as it's where we'll be storing the conversion function
            if (customMapping == null)
            {
              customMapping = new CustomMapping
              {
                DestinationType = destinationType
              };
              customMappingCache.Add(pair, customMapping);

              typeMapping.CustomMapping = customMapping;
            }

            // Let the custom mapping be the owner of the conversion function
            customMapping.AddConversionFunction(sourceMember, destinationMember, mapping.ConversionFunction);

          }

          ProposedHierarchicalMapping hierarchicalMapping = null;

          // No source member or can't write to the destination?
          if (HasNoSourceMember(customExpression, sourceMember) || !destinationMember.CanWrite)
          {
            if (mapper.Options.Conventions.AutomaticallyFlattenHierarchies)
            {
              // Propose a mapping that flattens a hierarchy if possible.
              // For example, map type.CompanyName to otherType.Company.Name
              hierarchicalMapping = memberProvider.ProposeHierarchicalMapping(destinationMember);
            }

            // No way to map this thing? Add it to incompatible members if the option has been turned on.
            // Will cause an (intended) exception later on, allowing you to verify your mappings
            // for correctness and completeness.
            if (hierarchicalMapping == null && mapper.Options.Strictness.ThrowWithoutCorrespondingSourceMember)
            {
              typeMapping.IncompatibleMappings.Add(destinationMember);
            }
          }

          // Nullable value types screw up everything!
          var nullableType = NullableTypeHelper.TryGetNullableType(sourceMember);

          // Can we do a simple right to left assignment between the members?
          // So, are they basically the same type or do we need to do further mapping?
          var canUseSimpleTypeMapping = CanUseDirectAssignment(pair, destinationMember, sourceMember, nullableType, hierarchicalMapping);

          if (canUseSimpleTypeMapping)
          {
            // If simple mapping is possible create a mapping between the members
            typeMapping.ProposedMappings.Add
            (
              new ProposedMemberMapping
              {
                SourceMember = sourceMember,
                DestinationMember = destinationMember,
                HierarchicalMapping = hierarchicalMapping
              }
            );
          }
          // No simple assignment, but a custom expression is supplied
          // and that's just as good as having a direct assignment mapping
          else if(customExpression != null)
          {
            typeMapping.ProposedMappings.Add
            (
              new ProposedMemberMapping
              {
                SourceMember = sourceMember,
                DestinationMember = destinationMember,
                HierarchicalMapping = hierarchicalMapping
              }
            );
          }
          // We have a source member but can't directly assign the source to the destination.
          // Further mapping is needed.
          else if (sourceMember != null)
          {
            // Is the member of an IEnumerable type? 
            if (AreMembersIEnumerable(destinationMember, sourceMember))
            {
              // Create a deeper mapping for IEnumerable members
              GenerateEnumerableMapping(currentDepth, options, customMapping, typeMapping, destinationMember, sourceMember);
            }
            else
            {
              // Create a deeper mapping for a 'regular' type.
              GenerateComplexTypeMapping(currentDepth, options, customMapping, typeMapping, destinationMember, sourceMember);
            }
          }
          // All we have is a destination member and a custom expression
          // that gives the destination member a value. Good enough! 
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

        // Don't cache the typemapping when this flag has been set.
        // That happens when the maximum depth was reached during the mapping
        // for this particular type and we didn't explore the full depth
        // of the type. We don't wanna reuse this typemapping at a later time
        // because the mapping might be for something completely different
        // at a depth at which the full depth CAN be explored.
        if (!typeMapping.DoNotCache)
        {
          lock (mappingCache)
          {
            mappingCache[pair] = typeMapping;
          }
        }

        typeStack.Pop();

        return typeMapping;
      }

      private class SourceDestinationMapping
      {
        public PropertyOrFieldInfo Source { get; set; }
        public PropertyOrFieldInfo Destination { get; set; }
        public LambdaExpression ConversionFunction { get; set; }
      }

      private static IEnumerable<SourceDestinationMapping> GetTypeMembers(IMemberProvider memberProvider, MappingOptions options, int currentDepth)
      {
        var destinationMembers = memberProvider.GetDestinationMembers();

        foreach (var destinationMember in destinationMembers)
        {
          var destination = destinationMember;
          var sourceMember = memberProvider.GetMatchingSourceMember(destinationMember);
          LambdaExpression conversion = null;

          // User supplied a custom method that can influence the mapping
          if (options != null)
          {
            // A class that allows you to customize a few things about a mapping
            var option = new MemberOption(sourceMember, destinationMember);

            // Execute the user supplied function
            options(sourceMember, destinationMember, option, currentDepth);


            conversion = option.ConversionFunction;

            switch (option.State)
            {
              // User indicated in the `options` method that he wants to ignore the member
              case MemberOptionState.Ignored:
                continue;
            }

            // Source member is set
            if (option.Source != null)
            {
              // If the user supplied an invalid source member
              if (option.Source.DeclaringType != sourceMember.DeclaringType)
              {
                throw new InvalidOperationException("Cannot use member declared on another type.");
              }

              sourceMember = option.Source;
            }

            // Destination member is set
            if (option.Destination != null)
            {
              // If the user supplied an invalid destination member
              if (option.Destination.DeclaringType != destination.DeclaringType)
              {
                throw new InvalidOperationException("Cannot use member declared on another type.");
              }

              destination = option.Destination;
            }

          }

          // Simple container class
          var mapping = new SourceDestinationMapping
          {
            Source = sourceMember,
            Destination = destination,
            ConversionFunction = conversion
          };

          yield return mapping;

        }
      }

      private static bool AreMembersIEnumerable(PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember)
      {
        return CollectionTypeHelper.IsEnumerable(sourceMember.PropertyOrFieldType)
          && CollectionTypeHelper.IsEnumerable(destinationMember.PropertyOrFieldType);
      }

      /// <summary>
      /// Go one deeper into the type hierarchy to map between a source and destination member.
      /// </summary>
      private void GenerateComplexTypeMapping(int currentDepth, MappingOptions options, CustomMapping customMapping, ProposedTypeMapping typeMapping, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember)
      {
        var complexPair = new TypePair(sourceMember.PropertyOrFieldType, destinationMember.PropertyOrFieldType);

        // Go one deeper
        var complexTypeMapping = GetComplexTypeMapping(currentDepth + 1, complexPair, options, customMapping);

        // If a mapping has been found
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
          // If no mapping has been found, don't cache the 'owning' typemapping as it will cause issues later
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

      private bool CanUseDirectAssignment(TypePair pair, PropertyOrFieldInfo destinationMember, PropertyOrFieldInfo sourceMember, Type nullableType, ProposedHierarchicalMapping hierarchicalMapping)
      {
        Type sourceMemberType = null;

        if (sourceMember == null)
        {
          if (hierarchicalMapping == null)
          {
            return false;
          }
        }
        else
        {
          sourceMemberType = sourceMember.PropertyOrFieldType;
        }

        if (hierarchicalMapping != null)
        {
          sourceMemberType = hierarchicalMapping.ReturnType;
        }

        if (!destinationMember.PropertyOrFieldType.IsAssignableFrom(sourceMemberType))
        {
          if (sourceMemberType.IsNullableValueType() && destinationMember.PropertyOrFieldType.IsAssignableFrom(nullableType))
          {
            return true;
          }
          else if (ConversionTypeHelper.AreConvertible(sourceMemberType, destinationMember.PropertyOrFieldType))
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
          if (sourceMemberType.IsValueType || sourceMemberType == typeof(string))
          {
            return true;
          }
          else
          {
            return false;
          }
        }

        // Can't assign enumerable memebers to eachother, we leave that decision to the code
        // generator which can determine if we need to preserve the contents of the enumerable.
        if (CollectionTypeHelper.IsEnumerable(destinationMember.PropertyOrFieldType)
          && CollectionTypeHelper.IsEnumerable(sourceMemberType)
          && mapper.Options.Conventions.PreserveDestinationListContents) // but only if the option is turned on at all
        {
          return false;
        }

        return true;
      }

      

      private bool HasNoSourceMember(Expression customExpression, PropertyOrFieldInfo sourceMember)
      {
        return sourceMember == null
                  && customExpression == null;
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
        if (CollectionTypeHelper.IsEnumerable(pair))
        {
          var source = CollectionTypeHelper.GetTypeInsideEnumerable(pair.SourceType);
          var destination = CollectionTypeHelper.GetTypeInsideEnumerable(pair.DestinationType);
          pair = new TypePair(source, destination);
        }

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
