namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Represents the base item class for all the mapping metadata
    /// </summary>
    internal abstract class Map : GlobalItem
    {
        protected Map()
            : base(MetadataFlags.Readonly)
        {
        }

        #region Properties

        /// <summary>
        /// Returns the Item that is being mapped either for ES or OE spaces.
        /// The EDM type will be an EntityContainer type in ES mapping case.
        /// In the OE mapping case it could be any type.
        /// </summary>
        internal abstract MetadataItem EdmItem { get; }

        #endregion
    }
}
