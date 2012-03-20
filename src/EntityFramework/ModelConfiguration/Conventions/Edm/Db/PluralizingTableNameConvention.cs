namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Design.PluralizationServices;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Convention to set the table name to be a pluralized version of the entity type name.
    /// </summary>
    public sealed class PluralizingTableNameConvention : IDbConvention<DbTableMetadata>
    {
        private static readonly PluralizationService _pluralizationService
            = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));

        internal PluralizingTableNameConvention()
        {
        }

        void IDbConvention<DbTableMetadata>.Apply(DbTableMetadata table, DbDatabaseMetadata database)
        {
            if (table.GetTableName() == null)
            {
                var schema = database.Schemas.Where(s => s.Tables.Contains(table)).Single();

                table.DatabaseIdentifier
                    = schema.Tables.Except(new[] { table })
                        .UniquifyIdentifier(_pluralizationService.Pluralize(table.DatabaseIdentifier));
            }
        }
    }
}