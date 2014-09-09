// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to introduce indexes for foreign keys.
    /// </summary>
    public class ForeignKeyIndexConvention : IStoreModelConvention<AssociationType>
    {
        /// <inheritdoc />
        public virtual void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, "item");

            if (item.Constraint == null)
            {
                return;
            }

            var consolidatedIndexes
                = ConsolidatedIndex.BuildIndexes(
                    item.Name,
                    item.Constraint.ToProperties.Select(p => Tuple.Create(p.Name, p)));

            var dependentColumnNames = item.Constraint.ToProperties.Select(p => p.Name);

            if (!consolidatedIndexes.Any(c => c.Columns.SequenceEqual(dependentColumnNames)))
            {
                var name = IndexOperation.BuildDefaultName(dependentColumnNames);

                var order = 0;
                foreach (var dependentColumn in item.Constraint.ToProperties)
                {
                    var newAnnotation = new IndexAnnotation(new IndexAttribute(name, order++));

                    var existingAnnotation = dependentColumn.Annotations.GetAnnotation(XmlConstants.IndexAnnotationWithPrefix);
                    if (existingAnnotation != null)
                    {
                        newAnnotation = (IndexAnnotation)((IndexAnnotation)existingAnnotation).MergeWith(newAnnotation);
                    }

                    dependentColumn.AddAnnotation(XmlConstants.IndexAnnotationWithPrefix, newAnnotation);
                }
            }
        }
    }
}
