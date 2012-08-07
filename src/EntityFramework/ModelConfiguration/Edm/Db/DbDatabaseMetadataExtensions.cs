// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal static class DbDatabaseMetadataExtensions
    {
        public const string DefaultSchema = "dbo";
        private const string ProviderInfoAnnotation = "ProviderInfo";

        public static DbDatabaseMetadata Initialize(
            this DbDatabaseMetadata database, double version = DataModelVersions.Version3)
        {
            Contract.Requires(database != null);

            database.Version = version;
            database.Name = "CodeFirstDatabase";
            database.Schemas.Add(
                new DbSchemaMetadata
                    {
                        Name = DefaultSchema,
                        DatabaseIdentifier = DefaultSchema
                    });

            return database;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static StoreItemCollection ToStoreItemCollection(this DbDatabaseMetadata database)
        {
            Contract.Requires(database != null);

            // Provider information should be first class in EDM when we ship
            // but for now we use our annotation
            var providerInfo = database.GetProviderInfo();

            Contract.Assert(providerInfo != null);

            var stringBuilder = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(
                stringBuilder, new XmlWriterSettings
                                   {
                                       Indent = true
                                   }))
            {
                new SsdlSerializer().Serialize(
                    database, providerInfo.ProviderInvariantName, providerInfo.ProviderManifestToken, xmlWriter);
            }

            var xml = stringBuilder.ToString();

            using (var xmlReader = XmlReader.Create(new StringReader(xml)))
            {
                return new StoreItemCollection(new[] { xmlReader });
            }
        }

        public static DbTableMetadata AddTable(this DbDatabaseMetadata database, string name)
        {
            Contract.Requires(database != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Assert(database.Schemas.Count == 1);

            var schema = database.Schemas.Single();
            var uniqueIdentifier = schema.Tables.UniquifyName(name);

            var table = new DbTableMetadata
                            {
                                Name = uniqueIdentifier,
                                DatabaseIdentifier = uniqueIdentifier
                            };

            schema.Tables.Add(table);

            return table;
        }

        public static DbTableMetadata AddTable(
            this DbDatabaseMetadata database, string name, DbTableMetadata pkSource)
        {
            var table = database.AddTable(name);

            // Add PK columns to the new table
            foreach (var col in pkSource.KeyColumns)
            {
                var pk = col.Clone();
                table.Columns.Add(pk);
            }

            return table;
        }

        public static void RemoveTable(this DbDatabaseMetadata database, DbTableMetadata table)
        {
            Contract.Requires(database != null);
            Contract.Requires(table != null);

            database.Schemas.Select(s => s.Tables).Each(ts => ts.Remove(table));
        }

        public static DbTableMetadata FindTableByName(this DbDatabaseMetadata database, DatabaseName tableName)
        {
            Contract.Requires(database != null);
            Contract.Requires(tableName != null);

            return database.Schemas.Single().Tables.SingleOrDefault(
                t =>
                    {
                        var databaseName = t.GetTableName();
                        return databaseName != null
                                   ? databaseName.Equals(tableName)
                                   : string.Equals(t.Name, tableName.Name, StringComparison.Ordinal);
                    });
        }

        public static DbProviderInfo GetProviderInfo(this DbDatabaseMetadata database)
        {
            Contract.Requires(database != null);

            return (DbProviderInfo)database.Annotations.GetAnnotation(ProviderInfoAnnotation);
        }

        public static void SetProviderInfo(this DbDatabaseMetadata database, DbProviderInfo providerInfo)
        {
            Contract.Requires(database != null);
            Contract.Requires(providerInfo != null);

            database.Annotations.SetAnnotation(ProviderInfoAnnotation, providerInfo);
        }
    }
}
