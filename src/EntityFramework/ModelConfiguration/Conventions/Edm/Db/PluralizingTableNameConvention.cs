// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Design.PluralizationServices;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Convention to set the table name to be a pluralized version of the entity type name.
    /// </summary>
    public class PluralizingTableNameConvention : IDbConvention<DbTableMetadata>
    {
        private static readonly PluralizationService _pluralizationService
            = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));

        public void Apply(DbTableMetadata dbDataModelItem, DbDatabaseMetadata database)
        {
            if (dbDataModelItem.GetTableName() == null)
            {
                var schema = database.Schemas.Single(s => s.Tables.Contains(dbDataModelItem));

                dbDataModelItem.DatabaseIdentifier
                    = schema.Tables.Except(new[] { dbDataModelItem })
                        .UniquifyIdentifier(_pluralizationService.Pluralize(dbDataModelItem.DatabaseIdentifier));
            }
        }
    }
}
