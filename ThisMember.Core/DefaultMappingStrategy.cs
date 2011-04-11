using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

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

    private ProposedTypeMapping GetTypeMapping(TypePair pair, MappingOptions options = null, Expression customMapping = null)
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



      var destinationProperties = (from p in destinationType.GetProperties()
                                   where p.CanWrite && !p.GetIndexParameters().Any()
                                   select p);

      HashSet<string> customProperties = new HashSet<string>();

      if (customMapping != null)
      {
        var lambda = customMapping as LambdaExpression;

        if (lambda == null) throw new ArgumentException("Only LambdaExpression is allowed here");

        var newType = lambda.Body as NewExpression;

        if (newType == null) throw new ArgumentException("Only NewExpression is allowed to specify a custom mapping");

        customProperties = new HashSet<string>(newType.Members.Select(m => m.Name));

        foreach (var member in newType.Members)
        {
          PropertyInfo prop;
          //if (destinationProperties.TryGetValue(member.Name, out prop))
          //{
          //  Console.WriteLine(prop);
          //}
        }

      }


      var sourceProperties = (from p in sourceType.GetProperties()
                              where p.CanRead && !p.GetIndexParameters().Any()
                              select p).ToDictionary(k => k.Name);

      foreach (var destinationProperty in destinationProperties)
      {
        PropertyInfo sourceProperty;

        if (customProperties.Contains(destinationProperty.Name))
        {
          //continue;
        }

        if (sourceProperties.TryGetValue(destinationProperty.Name, out sourceProperty)
          && destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
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

          if (typeof(IEnumerable).IsAssignableFrom(sourceProperty.PropertyType)
            && typeof(IEnumerable).IsAssignableFrom(destinationProperty.PropertyType))
          {

            var typeOfSourceEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(sourceProperty.PropertyType);
            var typeOfDestinationEnumerable = CollectionTypeHelper.GetTypeInsideEnumerable(destinationProperty.PropertyType);

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
                complexTypeMapping = GetTypeMapping(complexPair, options);
              }

              complexTypeMapping = complexTypeMapping.Clone();

              complexTypeMapping.DestinationMember = destinationProperty;
              complexTypeMapping.SourceMember = sourceProperty;

              typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
            }
          }
          else
          {
            var complexPair = new TypePair(sourceProperty.PropertyType, destinationProperty.PropertyType);

            ProposedTypeMapping complexTypeMapping;

            if (!mappingCache.TryGetValue(complexPair, out complexTypeMapping))
            {
              complexTypeMapping = GetTypeMapping(complexPair, options);
            }

            complexTypeMapping = complexTypeMapping.Clone();

            complexTypeMapping.DestinationMember = destinationProperty;
            complexTypeMapping.SourceMember = sourceProperty;

            typeMapping.ProposedTypeMappings.Add(complexTypeMapping);
          }
        }
      }

      lock (syncRoot)
      {
        mappingCache.Add(pair, typeMapping);
      }

      return typeMapping;
    }

    public ProposedMap<TSource, TDestination> CreateMap<TSource, TDestination>(MappingOptions options = null, Expression<Func<TSource, object>> customMapping = null)
    {
      var map = new ProposedMap<TSource, TDestination>(this.mapper);

      var pair = new TypePair(typeof(TSource), typeof(TDestination));

      map.MapGenerator = this.MapGenerator;

      map.SourceType = pair.SourceType;
      map.DestinationType = pair.DestinationType;

      ProposedTypeMapping mapping;

      if (!this.mappingCache.TryGetValue(pair, out mapping))
      {
        mapping = GetTypeMapping(pair, options, customMapping);
      }

      map.ProposedTypeMapping = mapping;
      map.CustomMapping = CustomMapping.GetCustomMapping(typeof(TDestination), customMapping);

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

  }
}
