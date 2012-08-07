// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Convention to convert any data types that were explicitly specified, via data annotations or <see cref="DbModelBuilder" /> API, 
    ///     to be lower case. The default SqlClient provider is case sensitive and requires data types to be lower case. This convention
    ///     allows the <see cref="T:System.ComponentModel.DataAnnotations.ColumnAttrbiute" /> and <see cref="DbModelBuilder" /> API to be case insensitive.
    /// </summary>
    public sealed class ColumnTypeCasingConvention : IDbConvention<DbTableColumnMetadata>
    {
        internal ColumnTypeCasingConvention()
        {
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        void IDbConvention<DbTableColumnMetadata>.Apply(DbTableColumnMetadata tableColumn, DbDatabaseMetadata database)
        {
            if (!string.IsNullOrWhiteSpace(tableColumn.TypeName))
            {
                tableColumn.TypeName = tableColumn.TypeName.ToLowerInvariant();
            }
        }
    }
}
