namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     The base for all all Entity Data Model (EDM) item types that with a <see cref = "Name" /> property.
    /// </summary>
    internal abstract class EdmNamedMetadataItem
        : EdmMetadataItem, INamedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned name.
        /// </summary>
        public virtual string Name { get; set; }
    }
}
