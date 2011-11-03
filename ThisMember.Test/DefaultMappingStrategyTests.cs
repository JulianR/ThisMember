using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;
using System.Linq.Expressions;
using ThisMember.Core.Interfaces;
using System.Reflection;

namespace ThisMember.Test
{
  [TestClass]
  public class DefaultMappingStrategyTests
  {

    private class Poco_From
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public List<int> OtherIDs { get; set; }
    }

    private class Poco_To
    {
      public int ID { get; set; }
      public string Name { get; set; }
      public List<int> OtherIDs { get; set; }
    }

    private class ExpectedMappings<TSource, TDestination>
    {

      public List<ExpectedMapping<TSource, TDestination>> Mappings { get; set; }

      public ExpectedMappings()
      {
        Mappings = new List<ExpectedMapping<TSource, TDestination>>();
      }

      public void Add(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination)
      {
        Mappings.Add(new ExpectedMapping<TSource, TDestination>(source, destination));
      }
    }

    private class ExpectedMapping<TSource, TDestination>
    {
      public Expression<Func<TDestination, object>> Destination { get; set; }
      public Expression<Func<TSource, object>> Source { get; set; }

      public ExpectedMapping(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination)
      {
        Destination = destination;
        Source = source;
      }

    }

    private static PropertyOrFieldInfo GetMemberInfoFromExpression(Expression body)
    {
      if ((body != null) && ((body.NodeType == ExpressionType.Convert) || (body.NodeType == ExpressionType.ConvertChecked)))
      {
        body = ((UnaryExpression)body).Operand;
      }
      var expression2 = (MemberExpression)body;
      return expression2.Member;
    }

    static bool ContainsMappingFor<T, T2>(ProposedMap map, ExpectedMappings<T, T2> mappings)
    {
      int foundMappings = 0;
      foreach (var mapping in mappings.Mappings)
      {

        if (!map.ProposedTypeMapping.ProposedMappings.Contains(
        new ProposedMemberMapping()
        {
          SourceMember = GetMemberInfoFromExpression(mapping.Source.Body),
          DestinationMember = GetMemberInfoFromExpression(mapping.Destination.Body)
        })
        && !map.ProposedTypeMapping.ProposedTypeMappings.Contains(
        new ProposedTypeMapping
        {
          SourceMember = GetMemberInfoFromExpression(mapping.Source.Body),
          DestinationMember = GetMemberInfoFromExpression(mapping.Destination.Body)
        })
        )
        {
          return false;
        }
        else
        {
          foundMappings++;
        }

      }

      if (map.ProposedTypeMapping.ProposedMappings.Count + map.ProposedTypeMapping.ProposedTypeMappings.Count != foundMappings)
      {
        return false;
      }

      return true;
    }

    [TestMethod]
    public void ExpectedPropertiesWillBeMapped()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.MappingStrategy.CreateMapProposal(new TypePair(typeof(Poco_From), typeof(Poco_To)));

      proposition.FinalizeMap();

      var expectation = new ExpectedMappings<Poco_From, Poco_To>();

      expectation.Add(t => t.ID, t => t.ID);
      expectation.Add(t => t.Name, t => t.Name);
      expectation.Add(t => t.OtherIDs, t => t.OtherIDs);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );


    }

    private interface IPocoOneProperty_From
    {
      int ID { get; set; }
    }

    private class PocoOneProperty_From : IPocoOneProperty_From
    {
      public int ID { get; set; }
    }

    private class PocoOneProperty_To
    {
      public int ID { get; set; }
    }

    [TestMethod]
    public void ExpectedPropertiesFromInterfaceWillBeMapped()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.MappingStrategy.CreateMapProposal(new TypePair(typeof(IPocoOneProperty_From), typeof(PocoOneProperty_To)));

      var expectation = new ExpectedMappings<IPocoOneProperty_From, PocoOneProperty_To>();

      expectation.Add(t => t.ID, t => t.ID);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );
    }

    [TestMethod]
    public void IgnoringMemberIgnoresTheMember()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.MappingStrategy.CreateMapProposal(
        new TypePair(typeof(Poco_From), typeof(Poco_To)),
        (s, m, option, depth) =>
        {
          if (s.Name == "ID")
          {
            option.IgnoreMember();
          }
        }
      );

      var expectation = new ExpectedMappings<Poco_From, Poco_To>();

      expectation.Add(t => t.Name, t => t.Name);
      expectation.Add(t => t.OtherIDs, t => t.OtherIDs);

      Assert.IsTrue
      (
        ContainsMappingFor(proposition, expectation)
      );
    }

    [TestMethod]
    public void FinalFunctionIsOfExpectedType()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.MappingStrategy.CreateMapProposal(new TypePair(typeof(IPocoOneProperty_From), typeof(PocoOneProperty_To)));

      var map = proposition.FinalizeMap();

      Assert.IsTrue(map.MappingFunction.GetType() == typeof(Func<IPocoOneProperty_From, PocoOneProperty_To, PocoOneProperty_To>));

    }

    private class ListDestinationType
    {
      public List<int> Source { get; set; }
    }

    private class ListSourceType
    {
      public List<int> Source { get; set; }
    }

    private class IEnumerableSourceType
    {
      public IEnumerable<int> Source { get; set; }
    }

    private class ArrayDestinationType
    {
      public int[] Source { get; set; }
    }

    [TestMethod]
    public void ListIntPropertyGetsNormalMapping()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.CreateMapProposal(typeof(ListSourceType), typeof(ListDestinationType));

      var expectation = new ExpectedMappings<ListSourceType, ListDestinationType>();

      expectation.Add(t => t.Source, t => t.Source);

      Assert.IsTrue(ContainsMappingFor(proposition, expectation));
    }

    [TestMethod]
    public void ListIntPropertyGetsMappedToArray()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.CreateMapProposal(typeof(ListSourceType), typeof(ArrayDestinationType));

      var expectation = new ExpectedMappings<ListSourceType, ArrayDestinationType>();

      expectation.Add(t => t.Source, t => t.Source);

      Assert.IsTrue(ContainsMappingFor(proposition, expectation));
    }

    private class SourceElementType
    {
      public int ID { get; set; }
    }

    private class ListComplexSourceType
    {
      public List<SourceElementType> Source { get; set; }
    }

    private class DestinationElementType
    {
      public int ID { get; set; }
    }

    private class ListComplexDestinationType
    {
      public List<DestinationElementType> Source { get; set; }
    }

    [TestMethod]
    public void ComplexListGetsMapped()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.CreateMapProposal(typeof(ListComplexSourceType), typeof(ListComplexDestinationType));

      var expectation = new ExpectedMappings<ListComplexSourceType, ListComplexDestinationType>();

      expectation.Add(t => t.Source, t => t.Source);

      Assert.IsTrue(ContainsMappingFor(proposition, expectation));
    }

    private class IEnumerableComplexSourceType
    {
      public List<SourceElementType> Source { get; set; }
    }

    [TestMethod]
    public void ComplexListGetsMappedFromIEnumerable()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.CreateMapProposal(typeof(IEnumerableComplexSourceType), typeof(ListComplexDestinationType));

      var expectation = new ExpectedMappings<IEnumerableComplexSourceType, ListComplexDestinationType>();

      expectation.Add(t => t.Source, t => t.Source);

      Assert.IsTrue(ContainsMappingFor(proposition, expectation));
    }

    public class SourceFields
    {
      public int ID = 10;
    }

    public class DestinationFields
    {
      public int ID;
    }

    [TestMethod]
    public void FieldsAreMapped()
    {
      var mapper = new MemberMapper();

      var proposition = mapper.CreateMapProposal<SourceFields, DestinationFields>();

      var expectation = new ExpectedMappings<SourceFields, DestinationFields>();

      expectation.Add(t => t.ID, t => t.ID);

      Assert.IsTrue(ContainsMappingFor(proposition, expectation));

    }

  }
}
