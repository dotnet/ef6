// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to apply column ordering specified via <see cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" /> 
    ///     or the <see cref="DbModelBuilder" /> API.
    /// </summary>
    public class ColumnOrderingConvention : IDbConvention<EntityType>
    {
        public void Apply(EntityType dbDataModelItem, EdmModel model)
        {
            ValidateColumns(dbDataModelItem, model.GetEntitySet(dbDataModelItem).Table);

            OrderColumns(dbDataModelItem.Properties)
                .Each(
                    c =>
                        {
                            var isKey = c.IsPrimaryKeyColumn;

                            dbDataModelItem.RemoveMember(c);
                            dbDataModelItem.AddMember(c);

                            if (isKey)
                            {
                                dbDataModelItem.AddKeyMember(c);
                            }
                        });

            dbDataModelItem.ForeignKeyBuilders
                .Each(fk => fk.DependentColumns = OrderColumns(fk.DependentColumns));
        }

        protected virtual void ValidateColumns(EntityType table, string tableName)
        {
        }

        private static IEnumerable<EdmProperty> OrderColumns(IEnumerable<EdmProperty> columns)
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
