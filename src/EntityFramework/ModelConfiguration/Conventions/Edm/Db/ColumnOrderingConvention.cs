// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to apply column ordering specified via
    ///     <see
    ///         cref="T:System.ComponentModel.DataAnnotations.ColumnAttribute" />
    ///     or the <see cref="DbModelBuilder" /> API.
    /// </summary>
    public class ColumnOrderingConvention : IModelConvention<EntityType>
    {
        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "dbDataModelItem");
            Check.NotNull(model, "model");

            ValidateColumns(edmDataModelItem, model.GetEntitySet(edmDataModelItem).Table);

            OrderColumns(edmDataModelItem.Properties)
                .Each(
                    c =>
                        {
                            var isKey = c.IsPrimaryKeyColumn;

                            edmDataModelItem.RemoveMember(c);
                            edmDataModelItem.AddMember(c);

                            if (isKey)
                            {
                                edmDataModelItem.AddKeyMember(c);
                            }
                        });

            edmDataModelItem.ForeignKeyBuilders
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
