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

    public DebugOptions Debug { get; set; }

    public MapperOptions()
    {

      Strictness = new MapperStrictnessOptions
      {
        ThrowWithoutCorrespondingSourceMember = false
      };

      Conventions = new MapperConventionOptions
      {
        CallToStringWhenDestinationIsString = true,
        AutomaticallyFlattenHierarchies = true,
        MakeCloneIfDestinationIsTheSameAsSource = true,
        IgnoreMemberAttributeShouldBeRespected = true,
        ReuseNonNullComplexMembersOnDestination = true,
        IgnoreCaseWhenFindingMatch = true,
        IgnoreMembersWithNullValueOnSource = false,
        PreserveDestinationListContents = true,
        MaxDepth = null
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

      Debug = new DebugOptions
      {
        DebugInformationEnabled = false
      };

    }
  }

  public class MapperStrictnessOptions
  {
    /// <summary>
    /// If a destination member does not have a corresponding source member (either through convention or custom mapping)
    /// throw an exception.
    /// Defaults to false.
    /// </summary>
    public bool ThrowWithoutCorrespondingSourceMember { get; set; }
  }

  public class MapperConventionOptions
  {
    /// <summary>
    /// If the destination member is of type String but the source member is not and no custom mapping has been specified,
    /// call ToString() on the corresponding source member.    
    /// Defaults to true.
    /// </summary>
    public bool CallToStringWhenDestinationIsString { get; set; }

    /// <summary>
    /// Attempts to map hierarchies such as order.Customer.Name to destination.CustomerName.
    /// Defaults to true.
    /// </summary>
    public bool AutomaticallyFlattenHierarchies { get; set; }

    /// <summary>
    /// If the source type is the same as the destination type, make a deep clone
    /// </summary>
    /// Defaults to true.
    public bool MakeCloneIfDestinationIsTheSameAsSource { get; set; }

    /// <summary>
    /// When set to true, the [IgnoreMember] attribute should be respected by ThisMember. When set to false, the attribute gets ignored.
    /// Defaults to true.
    /// </summary>
    public bool IgnoreMemberAttributeShouldBeRespected { get; set; }

    /// <summary>
    /// If the destination contains a complex mapping, say, customer.Address and an existing
    /// Customer is passed in as the destination with a non-null value for Address, then that existing
    /// object is used, with the source object being applied over it, instead of a new instance of Address
    /// being used. 
    /// Defaults to true.
    /// </summary>
    public bool ReuseNonNullComplexMembersOnDestination { get; set; }

    /// <summary>
    /// If set to true, this will automatically map a property named ProductId to another property named ProductID.
    /// Defaults to true.
    /// </summary>
    public bool IgnoreCaseWhenFindingMatch { get; set; }

    /// <summary>
    /// If set to true, this will stop a map from using members on the source object that have a null
    /// equivalent value (so either nullable value types or reference types).
    /// If you passed in an existing destination object, the values of properties are preserved.
    /// Defaults to false.
    /// </summary>
    public bool IgnoreMembersWithNullValueOnSource { get; set; }

    /// <summary>
    /// If set to true, ThisMember will preserve the values inside properties that are of type IList. This means
    /// that if you pass in a destination object with a list property that has been filled, those values
    /// are preserved, the list will only be added to. Does not work for arrays.
    /// Defaults to true.
    /// </summary>
    public bool PreserveDestinationListContents { get; set; }

    /// <summary>
    /// Controls to what depth into the object hierarchy properties get mapped for all mappings.
    /// If a clone is being made then Cloning.MaxCloneDepth takes precedence.
    /// Defaults to null.
    /// </summary>
    public int? MaxDepth { get; set; }

  }

  public class CloneOptions
  {
    /// <summary>
    /// Maximum depth that ThisMember will traverse into the type hierarchy when making a deep clone of an object. 
    /// Setting this to null means unlimited. There is no chance of a stackoverflow
    /// happening if it is set too high, but for complex and large types the generated mapping code may become very large (thousands of lines), complex and slow. 
    /// Defaults to 2.
    /// </summary>
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

  public class DebugOptions
  {
    /// <summary>
    /// Set this to true to retain some debug information on the final map, so you can inspect
    /// the generated expression tree. 
    /// Defaults to false.
    /// </summary>
    public bool DebugInformationEnabled { get; set; }
  }

  public class CompilationOptions
  {
    /// <summary>
    /// Compiliation to a dynamic assembly can be turned off this way. Compiling to a dynamic assembly can result in faster code, but can cause problems when your 
    /// custom mappings access private members or use closures. Try turning this off if you get strange exceptions.
    /// Defaults to true.
    /// </summary>
    public bool CompileToDynamicAssembly { get; set; }
  }

  public class MapperSafetyOptions
  {
    /// <summary>
    /// If set to true, ThisMember will attempt to make traversal of navigation properties 'safe' for your custom mappings. 
    /// Meaning that source.Customer.Address.Street would be converted to:
    /// source.Customer != null ? src.Customer.Address != null ? src.Customer.Address.Street : default(string) : default(string)
    /// However, this safety cannot be guaranteed for any more complex custom mappings than that. 
    /// Defaults to true.
    /// </summary>
    public bool PerformNullChecksOnCustomMappings { get; set; }


    /// <summary>
    /// What ThisMember should do when the source object you pass in is null. 
    /// Defaults to ReturnNullWhenSourceIsNull.
    /// </summary>
    public SourceObjectNullOptions IfSourceIsNull { get; set; }


    /// <summary>
    /// What to do when a recursive relationship is detected. For example a User type that defines an Address that defines a User.
    /// Defaults to IgnoreRecursiveProperties
    /// </summary>
    public RecursivePropertyOptions IfRecursiveRelationshipIsDetected { get; set; }

    /// <summary>
    /// If you pass in a destination object to apply the source object to, throw an ArgumentNullException.
    /// Defaults to true.
    /// </summary>
    public bool ThrowIfDestinationIsNull { get; set; }

    /// <summary>
    /// Ensures that an ICollection property is not an array before calling Add on it, which would result in a
    /// 'Collection was of a fixed size' System.NotSupportedException.  
    /// Defaults to true.
    /// </summary>
    public bool EnsureCollectionIsNotArrayType { get; set; }
  }

  public class ProjectionOptions
  {
    /// <summary>
    /// Determines whether or not collection-type members should be included in the projection.
    /// This is usually not supported when the resulting expression gets translated into SQL
    /// by for example the Entity Framework. 
    /// Defaults to true.
    /// </summary>
    public bool MapCollectionMembers { get; set; }
  }
}
