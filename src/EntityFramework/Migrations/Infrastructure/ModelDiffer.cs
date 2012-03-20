namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Diagnostics.Contracts;
    using System.Xml.Linq;

    [ContractClass(typeof(ModelDifferContracts))]
    internal abstract class ModelDiffer
    {
        public abstract IEnumerable<MigrationOperation> Diff(
            XDocument sourceModel,
            XDocument targetModel,
            string connectionString);

        internal static string GetQualifiedTableName(string table, string schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(table));
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));

            if (GetStandardizedSchemaName(schema) == null)
            {
                return table;
            }

            return schema + "." + table;
        }

        internal static string GetStandardizedSchemaName(string schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(schema));

            if (schema.EqualsIgnoreCase("dbo"))
            {
                return null;
            }

            return schema;
        }

        #region Contracts

        [ContractClassFor(typeof(ModelDiffer))]
        internal abstract class ModelDifferContracts : ModelDiffer
        {
            public override IEnumerable<MigrationOperation> Diff(
                XDocument sourceModel,
                XDocument targetModel,
                string connectionString)
            {
                Contract.Requires(sourceModel != null);
                Contract.Requires(targetModel != null);

                return default(IEnumerable<MigrationOperation>);
            }
        }

        #endregion
    }
}