// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Convention to apply column ordering specified via
    /// <see
    ///     cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" />
    /// or the <see cref="DbModelBuilder" /> API.
    /// </summary>
    public class ColumnOrderingConvention : IStoreModelConvention<EntityType>
    {
        /// <inheritdoc />
        public virtual void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            ValidateColumns(item, model.GetStoreModel().GetEntitySet(item).Table);

            OrderColumns(item.Properties)
                .Each(
                    c =>
                    {
                        var isKey = c.IsPrimaryKeyColumn;

                        item.RemoveMember(c);
                        item.AddMember(c);

                        if (isKey)
                        {
                            item.AddKeyMember(c);
                        }
                    });

            item.ForeignKeyBuilders
                           .Each(fk => fk.DependentColumns = OrderColumns(fk.DependentColumns));
        }

        /// <summary>
        /// Validates the ordering configuration supplied for columns.
        /// This base implementation is a no-op.
        /// </summary>
        /// <param name="table">The name of the table that the columns belong to.</param>
        /// <param name="tableName">The definition of the table.</param>
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
