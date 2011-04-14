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

    private readonly byte[] syncRoot = new byte[0];

    public IMapGenerator MapGenerator { get; set; }

    private readonly IMemberMapper mapper;

    public DefaultMappingStrategy(IMemberMapper mapper)
    {
      this.mapper = mapper;
    }

    private ProposedTypeMapping GetTypeMapping(TypePair pair, MappingOptions options = null, CustomMapping customMapping = null)
    {
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


      //HashSet<string> customProperties = new HashSet<string>();

      //if (customMapping != null)
      //{
      //  var lambda = customMapping as LambdaExpression;

      //  if (lambda == null) throw new ArgumentException("Only LambdaExpression is allowed here");

      //  var newType = lambda.Body as NewExpression;

      //  if (newType == null) throw new ArgumentException("Only NewExpression is allowed to specify a custom mapping");

      //  customProperties = new HashSet<string>(newType.Members.Select(m => m.Name));

      //  foreach (var member in newType.Members)
      //  {
      //    PropertyInfo prop;
      //    //if (destinationProperties.TryGetValue(member.Name, out prop))
      //    //{
      //    //  Console.WriteLine(prop);
      //    //}
      //  }

      //}


      var sourceProperties = (from p in sourceType.GetProperties()
                              where p.CanRead && !p.GetIndexParameters().Any()
                              select (PropertyOrFieldInfo)p)
                              .Union(from f in sourceType.GetFields()
                                     where !f.IsStatic
                                     select (PropertyOrFieldInfo)f)
                              .ToDictionary(k => k.Name);

      foreach (var destinationProperty in destinationProperties)
      {

        var ignoreAttribute = destinationProperty.GetCustomAttributes(typeof(IgnoreMember), false).SingleOrDefault() as IgnoreMember;

        if (ignoreAttribute != null && (string.IsNullOrEmpty(ignoreAttribute.Profile) || ignoreAttribute.Profile == mapper.Profile))
        {
          continue;
        }

        PropertyOrFieldInfo sourceProperty;

        //if (customProperties.Contains(destinationProperty.Name))
        {
          //continue;
        }

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
          throw new IncompatibleMappingException(destinationProperty);
        }
        else if (mapper.Options.Conventions.AutomaticallyFlattenHierarchies)
        {
        }

        



        if (sourceProperty != null
          && destinationProperty.PropertyOrFieldType.IsAssignableFrom(sourceProperty.PropertyOrFieldType))
        {

          if (options != null)
          {
            var option = new MappingOption();

            options(sourceProperty, destinationProperty, option);

            switch (option.State)
            {
              case MappingOptionState.Ignored:
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

              ProposedTypeMapping complexTypeMapping;

              if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
              {
                complexTypeMapping = GetTypeMapping(complexPair, options, customMapping);
              }

              complexTypeMapping = complexTypeMapping.Clone();

              complexTypeMapping.DestinationMember = destinationProperty;
              complexTypeMapping.SourceMember = sourceProperty;

              typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
            }
          }
          else
          {
            var complexPair = new TypePair(sourceProperty.PropertyOrFieldType, destinationProperty.PropertyOrFieldType);

            ProposedTypeMapping complexTypeMapping;

            if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
            {
              complexTypeMapping = GetTypeMapping(complexPair, options, customMapping);
            }

            complexTypeMapping = complexTypeMapping.Clone();

            complexTypeMapping.DestinationMember = destinationProperty;
            complexTypeMapping.SourceMember = sourceProperty;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
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

      mappingCache[pair] = typeMapping;

      return typeMapping;
    }

    public ProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMappingExpression = null)
    {
      var map = new ProposedMap<TSource, TDestination>(this.mapper);

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      map.MapGenerator = this.MapGenerator;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      ProposedTypeMapping mapping;

      CustomMapping customMapping = null;

      if (customMappingExpression != null)
      {
        customMapping = CustomMapping.GetCustomMapping(typeof(TDestination), customMappingExpression);
      }

      if (!this.mappingCache.TryGetValue(pair, out mapping))
      {
        mapping = GetTypeMapping(pair, options, customMapping);
      }

      map.ProposedTypeMapping = mapping;
      map.CustomMapping = customMapping;

      return map;
    }

    public ProposedMap CreateMap(TypePair pair, MappingOptions options = null)
    {

      var map = new ProposedMap(this.mapper);

      map.MapGenerator = this.MapGenerator;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      ProposedTypeMapping mapping;

      if (!this.mappingCache.TryGetValue(pair, out mapping))
      {
        mapping = GetTypeMapping(pair, options);
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
