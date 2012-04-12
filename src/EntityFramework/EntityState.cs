namespace System.Data.Entity
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Describes state of an entity
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [Flags]
    public enum EntityState
    {
        /// <summary>
        /// The entity is not being tracked by the context.
        /// An entity is in this state immediately after it has been created with the new operator
        /// or with one of the <see cref="DbSet"/> Create methods.
        /// </summary>
        Detached = 0x00000001,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database, and its property
        /// values have not changed from the values in the database.
        /// </summary>
        Unchanged = 0x00000002,

        /// <summary>
        /// The entity is being tracked by the context but does not yet exist in the database.
        /// </summary>
        Added = 0x00000004,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database, but has been marked
        /// for deletion from the database the next time SaveChanges is called.
        /// </summary>
        Deleted = 0x00000008,

        /// <summary>
        /// The entity is being tracked by the context and exists in the database, and some or all of its
        /// property values have been modified.
        /// </summary>
        Modified = 0x00000010
    }
}
