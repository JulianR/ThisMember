using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core;
using System.Linq.Expressions;

namespace ThisMember.ConsoleHost
{

  class Foo
  {
    public string Z { get; set; }
  }

  class Bar
  {
    public string Z { get; set; }
  }

  class SourceElement
  {
    public int X { get; set; }

    public List<Foo> Collection { get; set; }
  }

  class DestinationElement
  {
    public int X { get; set; }

    public List<Bar> Collection { get; set; }
  }

  class SourceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public IList<SourceElement> IDs { get; set; }
    public SourceType Bar { get; set; }
    public RecursiveSourceClass Foo { get; set; }
  }

  class DestinationType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public IEnumerable<DestinationElement> IDs { get; set; }
    public DestinationType Bar { get; set; }
    public RecursiveDestinationClass Foo { get; set; }
  }

  class RecursiveSourceClass
  {
    public int ID { get; set; }
    public RecursiveSourceClass Child { get; set; }
    public SourceType Foo { get; set; }
  }

  class RecursiveDestinationClass
  {
    public int ID { get; set; }
    public RecursiveDestinationClass Child { get; set; }
    public DestinationType Foo { get; set; }
  }

  public class VisibilityVisitor : ExpressionVisitor
  {
    protected override Expression VisitParameter(ParameterExpression node)
    {
      return base.VisitParameter(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      return base.VisitConstant(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      return base.VisitMethodCall(node);
    }
  }

  class Program
  {

    static RecursiveDestinationClass MapRecursive(RecursiveSourceClass source, RecursiveDestinationClass dest)
    {
      var sourceStack = new Stack<RecursiveSourceClass>();
      var destStack = new Stack<RecursiveDestinationClass>();

      sourceStack.Push(source);
      destStack.Push(dest);

    Lbl:
      while (sourceStack.Count > 0)
      {
        var _source = sourceStack.Pop();
        var _dest = destStack.Pop();

        _dest.ID = _source.ID;

        if (_source.Child != null)
        {
          sourceStack.Push(_source.Child);
          destStack.Push(_dest.Child = new RecursiveDestinationClass());
        }

        if (_source.Foo != null)
        {

          var fooSourceStack = new Stack<SourceType>();
          var fooDestStack = new Stack<DestinationType>();

          fooSourceStack.Push(_source.Foo);
          fooDestStack.Push(null);

          while (fooSourceStack.Count > 0)
          {
            var _fooSource = fooSourceStack.Pop();

            var _fooDest = new DestinationType();

            _fooDest.ID = _fooSource.ID;
            _fooDest.Name = _fooSource.Name;

            if (_fooSource.Bar != null)
            {
              fooSourceStack.Push(_fooSource.Bar);
              fooDestStack.Push(_fooDest.Bar = new DestinationType());
            }

            if (_fooSource.Foo != null)
            {
              sourceStack.Push(_fooSource.Foo);
              destStack.Push(_fooDest.Foo = new RecursiveDestinationClass());
              goto Lbl;
            }

            _dest.Foo = _fooDest;

          }
        }


      }

      return dest;

    }

    static void Main(string[] args)
    {

      var res = MapRecursive(new RecursiveSourceClass
      {
        ID = 1,
        Child = new RecursiveSourceClass
        {
          ID = 2,
          Child = new RecursiveSourceClass
          {
            ID = 3,
            Foo = new SourceType
            {
              ID = 2,
              Name = "Foo1",
              Bar = new SourceType
              {
                Foo = new RecursiveSourceClass
                {
                  ID = 4,
                  Child = new RecursiveSourceClass
                  {
                    ID = 5
                  }
                }
              }
            }
          },
          Foo = new SourceType
          {
            ID = 1,
            Name = "Foo"
          }
        }

      }, new RecursiveDestinationClass());
      //Expressions.CreateMethod(null);

      //int ix= 1;

      //Expression<Func<int>> xp = () =>  ix * ix;

      //var func = Expressions.CreateMethod((src, dest) => new
      //{
      //  String = src.Value.ToString(),
      //  Val = src.String.Length
      //}) as Func<Source, Destination, Destination>;

      //var src1 = new Source
      //{
      //  Value = 1
      //};

      //var dest1 = func(src1, new Destination());


      var mapper = new MemberMapper();

      int i = 2;

      var map = mapper.CreateMapProposal<SourceType, DestinationType>(customMapping: (src) => new
      {
        //ID = src.IDs.Count + 100 + i,
        ID = (from x in Enumerable.Range(0, 100)
              select x).Sum() + i,
        Name = src.Name.Length.ToString() + " " + src.Name
      }).FinalizeMap();

      i++;

      //var map = mapper.CreateMap(typeof(SourceType), typeof(DestinationType)).FinalizeMap();

      var source = new SourceType
      {
        ID = 1,
        IDs = new List<SourceElement>
        {
          new SourceElement
          {
            X = 10,
            Collection = new List<Foo>
            {
              new Foo
              {
                Z = "string"
              },
              new Foo
              {
                Z = "string1"
              },
              new Foo
              {
                Z = "string2"
              }
            }
          }
        },
        Name = "X"
      };

      var result = mapper.Map<SourceType, DestinationType>(source);

      //map.FinalizeMap();

      //new ProposedMap<SourceType, DestinationType>().AddExpression(source => source.ID, destination => destination.ID);

    }

    static void Foo()
    {

    }
  }
}
