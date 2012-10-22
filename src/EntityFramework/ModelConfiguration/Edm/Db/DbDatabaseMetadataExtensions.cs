// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
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

        public static EdmModel DbInitialize(
            this EdmModel database, double version = DataModelVersions.Version3)
        {
            Contract.Requires(database != null);

            database.Version = version;
            database.Name = "CodeFirstDatabase";
            database.Containers.Add(new EntityContainer(database.Name, DataSpace.SSpace));
            database.Namespaces.Add(
                new EdmNamespace
                    {
                        Name = database.Name + "Schema"
                    });

            return database;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static StoreItemCollection ToStoreItemCollection(this EdmModel database)
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

        public static EntityType AddTable(this EdmModel database, string name)
        {
            Contract.Requires(database != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var uniqueIdentifier = database.GetEntityTypes().UniquifyName(name);

            var table
                = new EntityType(uniqueIdentifier, XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            database.Namespaces.Single().EntityTypes.Add(table);
            database.AddEntitySet(table.Name, table, uniqueIdentifier);

            return table;
        }

        public static EntityType AddTable(
            this EdmModel database, string name, EntityType pkSource)
        {
            var table = database.AddTable(name);

            // Add PK columns to the new table
            foreach (var property in pkSource.DeclaredKeyProperties)
            {
                table.AddKeyMember(property.Clone());
            }

            return table;
        }

        public static EntityType FindTableByName(this EdmModel database, DatabaseName tableName)
        {
            Contract.Requires(database != null);
            Contract.Requires(tableName != null);

            return database.GetEntityTypes().SingleOrDefault(
                t =>
                    {
                        var databaseName = t.GetTableName();
                        return databaseName != null
                                   ? databaseName.Equals(tableName)
                                   : string.Equals(t.Name, tableName.Name, StringComparison.Ordinal);
                    });
        }
    }
}
