namespace System.Data.Entity.Internal
{
    /// <summary>
    ///     The types of member entries supported.
    /// </summary>
    internal enum MemberEntryType
    {
        ReferenceNavigationProperty,
        CollectionNavigationProperty,
        ScalarProperty,
        ComplexProperty,
    }
}