// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class INamedDataModelItemExtensions
    {
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
