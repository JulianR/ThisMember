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
  }

  class DestinationType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public IEnumerable<DestinationElement> IDs { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {

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
        ID = (from x in Enumerable.Range(0,100)
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
