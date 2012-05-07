namespace System.Data.Entity.Edm.Common
{
    using System.Collections.Generic;

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