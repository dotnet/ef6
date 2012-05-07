namespace System.Data.Entity.Core.Mapping
{
    /// <summary>
    /// Represents the various kind of member mapping
    /// </summary>
    internal enum MemberMappingKind
    {
        ScalarPropertyMapping = 0,

        NavigationPropertyMapping = 1,

        AssociationEndMapping = 2,

        ComplexPropertyMapping = 3,
    }
}