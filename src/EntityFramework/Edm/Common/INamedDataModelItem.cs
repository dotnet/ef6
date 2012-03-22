namespace System.Data.Entity.Edm.Common
{
    using System.Collections.Generic;

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

    internal static class INamedDataModelItemExtensions
    {
        public static bool TryGetByName<TNamedItem>(
            this IEnumerable<TNamedItem> list, string itemName, out TNamedItem result)
            where TNamedItem : INamedDataModelItem
        {
            foreach (var listItem in list)
            {
                if (listItem != null
                    && string.Equals(listItem.Name, itemName, StringComparison.Ordinal))
                {
                    result = listItem;
                    return true;
                }
            }
            result = default(TNamedItem);
            return false;
        }
    }
}
