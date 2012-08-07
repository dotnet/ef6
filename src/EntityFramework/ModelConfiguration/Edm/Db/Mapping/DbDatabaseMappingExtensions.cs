// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal static class DbDatabaseMappingExtensions
    {
        internal static DbDatabaseMapping Initialize(
            this DbDatabaseMapping databaseMapping, EdmModel model, DbDatabaseMetadata database)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(databaseMapping != null);
            Contract.Requires(database != null);

            databaseMapping.Model = model;
            databaseMapping.Database = database;
            var entityContainerMapping
                = new DbEntityContainerMapping
                      {
                          EntityContainer = model.Containers.Single()
                      };
            databaseMapping.EntityContainerMappings.Add(entityContainerMapping);

            return databaseMapping;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal static MetadataWorkspace ToMetadataWorkspace(this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            var metadataWorkspace = new MetadataWorkspace();

            var itemCollection
                = databaseMapping.Model.ToEdmItemCollection();
            var storeItemCollection
                = databaseMapping.Database.ToStoreItemCollection();
            var storageMappingItemCollection
                = databaseMapping.ToStorageMappingItemCollection(itemCollection, storeItemCollection);

            metadataWorkspace.RegisterItemCollection(itemCollection);
            metadataWorkspace.RegisterItemCollection(storeItemCollection);
            metadataWorkspace.RegisterItemCollection(storageMappingItemCollection);

            return metadataWorkspace;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal static StorageMappingItemCollection ToStorageMappingItemCollection(
            this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            return databaseMapping.ToStorageMappingItemCollection(
                databaseMapping.Model.ToEdmItemCollection(),
                databaseMapping.Database.ToStoreItemCollection());
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static StorageMappingItemCollection ToStorageMappingItemCollection(
            this DbDatabaseMapping databaseMapping, EdmItemCollection itemCollection,
            StoreItemCollection storeItemCollection)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(itemCollection != null);
            Contract.Requires(storeItemCollection != null);

            var stringBuilder = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(
                stringBuilder, new XmlWriterSettings
                                   {
                                       Indent = true
                                   }))
            {
                new MslSerializer().Serialize(databaseMapping, xmlWriter);
            }

            using (var xmlReader = XmlReader.Create(new StringReader(stringBuilder.ToString())))
            {
                return new StorageMappingItemCollection(itemCollection, storeItemCollection, new[] { xmlReader });
            }
        }

        internal static DbEntityTypeMapping GetEntityTypeMapping(
            this DbDatabaseMapping databaseMapping, EdmEntityType entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            var mappings = databaseMapping.GetEntityTypeMappings(entityType);

            if (mappings.Count() <= 1)
            {
                return mappings.SingleOrDefault();
            }
            else
            {
                // Return the property mapping
                return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
            }
        }

        internal static IEnumerable<DbEntityTypeMapping> GetEntityTypeMappings(
            this DbDatabaseMapping databaseMapping, EdmEntityType entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            return (from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    from etm in esm.EntityTypeMappings
                    where etm.EntityType == entityType
                    select etm);
        }

        internal static DbEntityTypeMapping GetEntityTypeMapping(
            this DbDatabaseMapping databaseMapping, Type entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            var mappings = (from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                            from etm in esm.EntityTypeMappings
                            where etm.GetClrType() == entityType
                            select etm);

            if (mappings.Count() <= 1)
            {
                return mappings.SingleOrDefault();
            }
            else
            {
                // Return the property mapping
                return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
            }
        }

        internal static IEnumerable<Tuple<DbEdmPropertyMapping, DbTableMetadata>> GetComplexPropertyMappings(
            this DbDatabaseMapping databaseMapping, Type complexType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(complexType != null);

            return from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                   from etm in esm.EntityTypeMappings
                   from etmf in etm.TypeMappingFragments
                   from epm in etmf.PropertyMappings
                   where epm.PropertyPath
                       .Any(
                           p => p.PropertyType.IsComplexType
                                && p.PropertyType.ComplexType.GetClrType() == complexType)
                   select Tuple.Create(epm, etmf.Table);
        }

        internal static DbEntitySetMapping GetEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EdmEntitySet entitySet)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entitySet != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings
                .SingleOrDefault(e => e.EntitySet == entitySet);
        }

        internal static IEnumerable<DbEntitySetMapping> GetEntitySetMappings(this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings;
        }

        internal static IEnumerable<DbAssociationSetMapping> GetAssociationSetMappings(
            this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .AssociationSetMappings;
        }

        internal static DbEntitySetMapping AddEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EdmEntitySet entitySet)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entitySet != null);

            var entitySetMapping = new DbEntitySetMapping
                                       {
                                           EntitySet = entitySet
                                       };

            databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings
                .Add(entitySetMapping);

            return entitySetMapping;
        }

        internal static DbAssociationSetMapping AddAssociationSetMapping(
            this DbDatabaseMapping databaseMapping, EdmAssociationSet associationSet)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(associationSet != null);

            var associationSetMapping
                = new DbAssociationSetMapping
                      {
                          AssociationSet = associationSet
                      }.Initialize();

            databaseMapping
                .EntityContainerMappings
                .Single()
                .AssociationSetMappings
                .Add(associationSetMapping);

            return associationSetMapping;
        }
    }
}
