namespace System.Data.Entity.Edm.Db
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     NamedDbItem is the base for all types in the Database Metadata construction and modification API with a <see cref = "Name" /> property.
    /// </summary>
    internal abstract class DbNamedMetadataItem
        : DbMetadataItem, INamedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned name.
        /// </summary>
        public virtual string Name { get; set; }
    }
}