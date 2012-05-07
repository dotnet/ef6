namespace System.Data.Entity.Edm.Common
{
    /// <summary>
    ///     INamedDataModelItem is implemented by model-specific base types for all types with a <see cref = "Name" /> property. <seealso cref = "EdmNamedMetadataItem" />
    /// </summary>
    internal interface INamedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned name.
        /// </summary>
        string Name { get; set; }
    }
}
