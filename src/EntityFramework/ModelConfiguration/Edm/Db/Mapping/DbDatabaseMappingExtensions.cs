// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
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
        public static DbDatabaseMapping Initialize(
            this DbDatabaseMapping databaseMapping, EdmModel model, EdmModel database)
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
        public static MetadataWorkspace ToMetadataWorkspace(this DbDatabaseMapping databaseMapping)
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
        public static StorageMappingItemCollection ToStorageMappingItemCollection(
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

        public static DbEntityTypeMapping GetEntityTypeMapping(
            this DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            var mappings = databaseMapping.GetEntityTypeMappings(entityType);

            if (mappings.Count() <= 1)
            {
                return mappings.SingleOrDefault();
            }

            // Return the property mapping
            return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
        }

        public static IEnumerable<DbEntityTypeMapping> GetEntityTypeMappings(
            this DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entityType != null);

            return (from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                    from etm in esm.EntityTypeMappings
                    where etm.EntityType == entityType
                    select etm);
        }

        public static DbEntityTypeMapping GetEntityTypeMapping(
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

            // Return the property mapping
            return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
        }

        public static IEnumerable<Tuple<DbEdmPropertyMapping, EntityType>> GetComplexPropertyMappings(
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
                           p => p.IsComplexType
                                && p.ComplexType.GetClrType() == complexType)
                   select Tuple.Create(epm, etmf.Table);
        }

        public static DbEntitySetMapping GetEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EntitySet entitySet)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(entitySet != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings
                .SingleOrDefault(e => e.EntitySet == entitySet);
        }

        public static IEnumerable<DbEntitySetMapping> GetEntitySetMappings(this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings;
        }

        public static IEnumerable<DbAssociationSetMapping> GetAssociationSetMappings(
            this DbDatabaseMapping databaseMapping)
        {
            Contract.Requires(databaseMapping != null);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .AssociationSetMappings;
        }

        public static DbEntitySetMapping AddEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EntitySet entitySet)
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

        public static DbAssociationSetMapping AddAssociationSetMapping(
            this DbDatabaseMapping databaseMapping, AssociationSet associationSet)
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
