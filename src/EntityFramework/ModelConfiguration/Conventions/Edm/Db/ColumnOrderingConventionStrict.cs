// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    /// Convention to apply column ordering specified via
    /// <see
    ///     cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" />
    /// or the <see cref="DbModelBuilder" /> API. This convention throws if a duplicate configured column order
    /// is detected.
    /// </summary>
    public class ColumnOrderingConventionStrict : ColumnOrderingConvention
    {
        /// <summary>
        /// Validates the ordering configuration supplied for columns to ensure 
        /// that the same ordinal was not supplied for two columns.
        /// </summary>
        /// <param name="table">The name of the table that the columns belong to.</param>
        /// <param name="tableName">The definition of the table.</param>
        protected override void ValidateColumns(EntityType table, string tableName)
        {
            var hasDuplicates
                = table.Properties
                       .Select(c => c.GetOrder())
                       .Where(o => o != null)
                       .GroupBy(o => o)
                       .Any(g => g.Count() > 1);

            if (hasDuplicates)
            {
                throw Error.DuplicateConfiguredColumnOrder(tableName);
            }
        }
    }
}
