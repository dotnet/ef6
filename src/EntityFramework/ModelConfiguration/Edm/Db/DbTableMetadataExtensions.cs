namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbTableMetadataExtensions
    {
        private const string TableNameAnnotation = "TableName";
        private const string KeyNamesTypeAnnotation = "KeyNamesType";

        public static DbTableColumnMetadata AddColumn(this DbTableMetadata table, string name)
        {
            Contract.Requires(table != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var tableColumn = new DbTableColumnMetadata
                                  {
                                      Name = table.Columns.UniquifyName(name)
                                  }.Initialize();

            tableColumn.SetPreferredName(name);

            table.Columns.Add(tableColumn);

            return tableColumn;
        }

        public static bool ContainsEquivalentForeignKey(
            this DbTableMetadata table, DbForeignKeyConstraintMetadata foreignKey)
        {
            return table.ForeignKeyConstraints
                .Any(
                    fk => fk.PrincipalTable == foreignKey.PrincipalTable
                          && fk.DependentColumns.SequenceEqual(foreignKey.DependentColumns));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static object GetConfiguration(this DbTableMetadata table)
        {
            Contract.Requires(table != null);

            return table.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this DbTableMetadata table, object configuration)
        {
            Contract.Requires(table != null);
            Contract.Requires(configuration != null);

            table.Annotations.SetConfiguration(configuration);
        }

        public static DatabaseName GetTableName(this DbTableMetadata table)
        {
            Contract.Requires(table != null);

            return (DatabaseName)table.Annotations.GetAnnotation(TableNameAnnotation);
        }

        public static void SetTableName(this DbTableMetadata table, DatabaseName tableName)
        {
            Contract.Requires(table != null);
            Contract.Requires(tableName != null);

            table.Annotations.SetAnnotation(TableNameAnnotation, tableName);
        }

        public static EdmEntityType GetKeyNamesType(this DbTableMetadata table)
        {
            Contract.Requires(table != null);

            return (EdmEntityType)table.Annotations.GetAnnotation(KeyNamesTypeAnnotation);
        }

        public static void SetKeyNamesType(this DbTableMetadata table, EdmEntityType entityType)
        {
            Contract.Requires(table != null);
            Contract.Requires(entityType != null);

            table.Annotations.SetAnnotation(KeyNamesTypeAnnotation, entityType);
        }
    }
}
