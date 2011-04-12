using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Globalization;

namespace ThisMember.Core
{
  public class MapperOptions
  {
    public Action<IMemberMapper, TypePair> BeforeMapping { get; set; }
    public Action<IMemberMapper, TypePair, object>  AfterMapping { get; set; }

    public MapperStrictnessOptions Strictness { get; set; }

    public MapperConventionOptions Conventions { get; set; }

    public MapperSafetyOptions Safety { get; set; } 

    public MapperOptions()
    {
      BeforeMapping = null;
      AfterMapping = null;

      Strictness = new MapperStrictnessOptions
      {
        ThrowWithoutCorrespondingSourceMember = false
      };

      Conventions = new MapperConventionOptions
      {
        CallToStringWhenDestinationIsString = true,
        DateTime = new MapperDateTimeOptions
        {
          ParseCulture = null,
          ParseStringsToDateTime = true
        }
      };

      Safety = new MapperSafetyOptions
      {
        PerformNullChecksOnCustomMappings = true
      };

    }

    //public static readonly MapperOptions Default = new MapperOptions();

  }

  public class MapperStrictnessOptions
  {
    /// <summary>
    /// If a destination member does not have a corresponding source member (either through convention or custom mapping)
    /// throw an exception.
    /// <remarks>Defaults to false.</remarks>
    /// </summary>
    public bool ThrowWithoutCorrespondingSourceMember { get; set; }
  }

  public class MapperDateTimeOptions
  {
    /// <summary>
    /// If the destination member is a DateTime but the source member is a string, attempt to convert by parsing the string.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool ParseStringsToDateTime { get; set; }

    /// <summary>
    /// The culture used to parse a string into a DateTime.
    /// </summary>
    /// <remarks>Defaults to null, in which case system culture is used.</remarks>
    public CultureInfo ParseCulture { get; set; }
  }

  public class MapperConventionOptions
  {
    /// <summary>
    /// If the destination member is of type String but the source member is not and no custom mapping has been specified,
    /// call ToString() on the corresponding source member.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool CallToStringWhenDestinationIsString { get; set; }

    public MapperDateTimeOptions DateTime { get; set; }
  }

  public class MapperSafetyOptions
  {
    /// <summary>
    /// If set to true, ThisMember will attempt to make traversal of navigation properties 'safe' for your custom mappings. 
    /// Meaning that source.Customer.Address.Street would be converted to:
    /// source.Customer != null ? src.Customer.Address != null ? src.Customer.Address.Street : default(string) : default(string)
    /// However, this safety cannot be guaranteed for any more complex custom mappings than that. 
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool PerformNullChecksOnCustomMappings { get; set; }
  }
}
