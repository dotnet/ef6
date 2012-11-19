// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to detect primary key properties.
    ///     Recognized naming patterns in order of precedence are:
    ///     1. 'Id'
    ///     2. [type name]Id
    ///     Primary key detection is case insensitive.
    /// </summary>
    public class IdKeyDiscoveryConvention : KeyDiscoveryConvention
    {
        private const string Id = "Id";

        protected override EdmProperty MatchKeyProperty(
            EntityType entityType, IEnumerable<EdmProperty> primitiveProperties)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(primitiveProperties, "primitiveProperties");

            var matches = primitiveProperties
                .Where(p => Id.Equals(p.Name, StringComparison.OrdinalIgnoreCase));

            if (!matches.Any())
            {
                matches = primitiveProperties
                    .Where(p => (entityType.Name + Id).Equals(p.Name, StringComparison.OrdinalIgnoreCase));
            }

            // If the number of matches is more than one, then multiple properties matched differing only by
            // case--for example, "Id" and "ID". In such as case we throw and point the developer to using
            // data annotations or the fluent API to disambiguate.
            if (matches.Count() > 1)
            {
                throw Error.MultiplePropertiesMatchedAsKeys(matches.First().Name, entityType.Name);
            }

            return matches.SingleOrDefault();
        }
    }
}
