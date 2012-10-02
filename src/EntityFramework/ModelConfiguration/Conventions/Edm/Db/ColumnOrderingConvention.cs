// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to apply column ordering specified via <see cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" /> 
    ///     or the <see cref="DbModelBuilder" /> API.
    /// </summary>
    public class ColumnOrderingConvention : IDbConvention<DbTableMetadata>
    {
        public void Apply(DbTableMetadata dbDataModelItem, DbDatabaseMetadata database)
        {
            ValidateColumns(dbDataModelItem);

            dbDataModelItem.Columns = OrderColumns(dbDataModelItem.Columns);
            dbDataModelItem.ForeignKeyConstraints.Each(fk => fk.DependentColumns = OrderColumns(fk.DependentColumns));
        }

        protected virtual void ValidateColumns(DbTableMetadata table)
        {
        }

        private static IList<DbTableColumnMetadata> OrderColumns(IEnumerable<DbTableColumnMetadata> columns)
        {
            var columnOrders
                = from c in columns
                  select new
                             {
                                 Column = c,
                                 Order = c.GetOrder() ?? int.MaxValue
                             };

            return columnOrders
                .OrderBy(c => c.Order)
                .Select(c => c.Column)
                .ToList();
        }
    }
}
