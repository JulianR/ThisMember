using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Core.Interfaces;
using System.Globalization;

namespace ThisMember.Core.Options
{
  public class MapperOptions
  {
    public MapperStrictnessOptions Strictness { get; set; }

    public MapperConventionOptions Conventions { get; set; }

    public MapperSafetyOptions Safety { get; set; }

    public CompilationOptions Compilation { get; set; }

    public ProjectionOptions Projection { get; set; }

    public CloneOptions Cloning { get; set; }

    public MapperOptions()
    {

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
        },
        AutomaticallyFlattenHierarchies = false,
        MakeCloneIfDestinationIsTheSameAsSource = true,
        IgnoreMemberAttributeShouldBeRespected = true,
        ReuseNonNullComplexMembersOnDestination = true,
        IgnoreCaseWhenFindingMatch = true,
        IgnoreMembersWithNullValueOnSource = false,
        PreserveDestinationListContents = true
      };

      Safety = new MapperSafetyOptions
      {
        PerformNullChecksOnCustomMappings = true,
        IfSourceIsNull = SourceObjectNullOptions.ReturnNullWhenSourceIsNull,
        IfRecursiveRelationshipIsDetected = RecursivePropertyOptions.IgnoreRecursiveProperties,
        ThrowIfDestinationIsNull = true,
        EnsureCollectionIsNotArrayType = true
      };

      Compilation = new CompilationOptions
      {
        CompileToDynamicAssembly = true
      };

      Projection = new ProjectionOptions
      {
        MapCollectionMembers = true
      };

      Cloning = new CloneOptions
      {
        MaxCloneDepth = 2,
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

    /// <summary>
    /// Attempts to map hierarchies such as order.Customer.Name to destination.CustomerName.
    /// </summary>
    /// <remarks>NOT SUPPORTED YET.</remarks>
    public bool AutomaticallyFlattenHierarchies { get; set; }

    /// <summary>
    /// If the source type is the same as the destination type, make a deep clone
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool MakeCloneIfDestinationIsTheSameAsSource { get; set; }

    /// <summary>
    /// When set to true, the [IgnoreMember] attribute should be respected by ThisMember. When set to false, the attribute gets ignored.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool IgnoreMemberAttributeShouldBeRespected { get; set; }

    /// <summary>
    /// If the destination contains a complex mapping, say, customer.Address and an existing
    /// Customer is passed in as the destination with a non-null value for Address, then that existing
    /// object is used, with the source object being applied over it, instead of a new instance of Address
    /// being used. 
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool ReuseNonNullComplexMembersOnDestination { get; set; }

    /// <summary>
    /// If set to true, this will automatically map a property named ProductId to another property named ProductID
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool IgnoreCaseWhenFindingMatch { get; set; }

    /// <summary>
    /// If set to true, this will stop a map from using members on the source object that have a null
    /// equivalent value (so either nullable value types or reference types).
    /// If you passed in an existing destination object, the values of properties are preserved.
    /// </summary>
    /// <remarks>Defaults to false.</remarks>
    public bool IgnoreMembersWithNullValueOnSource { get; set; }

    /// <summary>
    /// If set to true, ThisMember will preserve the values inside properties that are of type IList. This means
    /// that if you pass in a destination object with a list property that has been filled, those values
    /// are preserved, the list will only be added to. Does not work for arrays.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool PreserveDestinationListContents { get; set; }

    /// <summary>
    /// Controls to what depth into the object hierarchy properties get mapped for all mappings.
    /// If a clone is being made then Cloning.MaxCloneDepth takes precedence.
    /// </summary>
    /// <remarks>Defaults to null.</remarks>
    public int? MaxDepth { get; set; }

  }

  public class CloneOptions
  {
    /// <summary>
    /// Maximum depth that ThisMember will traverse into the type hierarchy when making a deep clone of an object. 
    /// Setting this to null means unlimited. There is no chance of a stackoverflow
    /// happening if it is set too high, but for complex and large types the generated mapping code may become very large (thousands of lines), complex and slow. 
    /// </summary>
    /// <remarks>Defaults to 2.</remarks>
    public int? MaxCloneDepth { get; set; }
  }

  public enum SourceObjectNullOptions
  {
    /// <summary>
    /// If the source object is null, return null.
    /// </summary>
    ReturnNullWhenSourceIsNull,
    /// <summary>
    /// ThisMember will not perform a null-check and simply crash and burn if the source is null.
    /// </summary>
    AllowNullReferenceExceptionWhenSourceIsNull,
    /// <summary>
    /// If the source object is null, return a valid destination object anyway.
    /// </summary>
    ReturnDestinationObject
  }

  public enum RecursivePropertyOptions
  {
    /// <summary>
    /// Skip over properties that would cause an infinite recursion.
    /// </summary>
    IgnoreRecursiveProperties,
    /// <summary>
    /// Throw an exception when a property is detected that could cause infinite recursion.
    /// </summary>
    ThrowIfRecursionIsDetected,

  }

  public class CompilationOptions
  {
    /// <summary>
    /// Compiliation to a dynamic assembly can be turned off this way. Compiling to a dynamic assembly can result in faster code, but can cause problems when your 
    /// custom mappings access private members or use closures. Try turning this off if you get strange exceptions.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool CompileToDynamicAssembly { get; set; }
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


    /// <summary>
    /// What ThisMember should do when the source object you pass in is null. 
    /// </summary>
    /// <remarks>Defaults to ReturnNullWhenSourceIsNull.</remarks>
    public SourceObjectNullOptions IfSourceIsNull { get; set; }


    /// <summary>
    /// What to do when a recursive relationship is detected. For example a User type that defines an Address that defines a User.
    /// </summary>
    /// <remarks>Defaults to IgnoreRecursiveProperties</remarks>
    public RecursivePropertyOptions IfRecursiveRelationshipIsDetected { get; set; }

    /// <summary>
    /// If you pass in a destination object to apply the source object to, throw an ArgumentNullException.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool ThrowIfDestinationIsNull { get; set; }

    /// <summary>
    /// Ensures that an ICollection property is not an array before calling Add on it, which would result in a
    /// 'Collection was of a fixed size' System.NotSupportedException.  
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool EnsureCollectionIsNotArrayType { get; set; }
  }

  public class ProjectionOptions
  {
    /// <summary>
    /// Determines whether or not collection-type members should be included in the projection.
    /// This is usually not supported when the resulting expression gets translated into SQL
    /// by for example the Entity Framework. 
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool MapCollectionMembers { get; set; }
  }
}
