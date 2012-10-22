// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class INamedDataModelItemExtensions
    {
        public static string UniquifyName(this IEnumerable<INamedDataModelItem> namedDataModelItems, string name)
        {
            Contract.Requires(namedDataModelItems != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var uniqueName = name;
            var i = 0;

            while (namedDataModelItems.Any(n => string.Equals(n.Name, uniqueName, StringComparison.Ordinal)))
            {
                uniqueName = name + ++i;
            }

            return uniqueName;
        }

        public static bool TryGetByName(this IEnumerable<DataModelAnnotation> list, string itemName, out DataModelAnnotation result)
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
            result = default(DataModelAnnotation);
            return false;
        }

        internal static string GetQualifiedName(this INamedDataModelItem item, string qualifiedPrefix)
        {
            return qualifiedPrefix + "." + item.Name;
        }
    }
}
