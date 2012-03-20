namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     EdmDataModelItem is the base for all types in the Entity Data Model (EDM) metadata construction and modification API.
    /// </summary>
    internal abstract class EdmDataModelItem
        : DataModelItem
    {
        internal abstract EdmItemKind GetItemKind();

        /// <summary>
        ///     Gets an <see cref = "EdmItemKind" /> value indicating which Entity Data Model (EDM) concept is represented by this item.
        /// </summary>
        public EdmItemKind ItemKind
        {
            get { return GetItemKind(); }
        }
    }
}