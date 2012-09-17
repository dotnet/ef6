// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     Convention to apply column ordering specified via <see cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" /> 
    ///     or the <see cref="DbModelBuilder" /> API. This convention throws if a duplicate configured column order
    ///     is detected.
    /// </summary>
    public class ColumnOrderingConventionStrict : ColumnOrderingConvention
    {
        protected override void ValidateColumns(DbTableMetadata table)
        {
            var hasDuplicates
                = table.Columns
                    .Select(c => c.GetOrder())
                    .Where(o => o != null)
                    .GroupBy(o => o)
                    .Any(g => g.Count() > 1);

            if (hasDuplicates)
            {
                throw Error.DuplicateConfiguredColumnOrder(table.DatabaseIdentifier);
            }
        }
    }
}
