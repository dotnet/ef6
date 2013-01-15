// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to set the table name to be a pluralized version of the entity type name.
    /// </summary>
    public class PluralizingTableNameConvention : IDbConvention<EntityType>
    {
        private static readonly IPluralizationService _pluralizationService
            = DbConfiguration.GetService<IPluralizationService>();

        public void Apply(EntityType dbDataModelItem, EdmModel model)
        {
            Check.NotNull(dbDataModelItem, "dbDataModelItem");
            Check.NotNull(model, "model");

            if (dbDataModelItem.GetTableName() == null)
            {
                var entitySet = model.GetEntitySet(dbDataModelItem);

                entitySet.Table
                    = model.GetEntitySets()
                           .Where(es => es.Schema == entitySet.Schema)
                           .Except(new[] { entitySet })
                           .UniquifyIdentifier(_pluralizationService.Pluralize(entitySet.Table));
            }
        }
    }
}
