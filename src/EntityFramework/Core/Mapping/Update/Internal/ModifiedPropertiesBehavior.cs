namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    internal enum ModifiedPropertiesBehavior
    {
        /// <summary>
        /// Indicates that all properties are modified. Used for added and deleted entities and for
        /// modified complex type sub-records.
        /// </summary>
        AllModified,

        /// <summary>
        /// Indicates that no properties are modified. Used for unmodified complex type sub-records.
        /// </summary>
        NoneModified,

        /// <summary>
        /// Indicates that some properties are modified. Used for modified entities.
        /// </summary>
        SomeModified,
    }
}