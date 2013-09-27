// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal static class DbDatabaseMappingExtensions
    {
        public static DbDatabaseMapping Initialize(
            this DbDatabaseMapping databaseMapping, EdmModel model, EdmModel database)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(model);
            DebugCheck.NotNull(database);

            databaseMapping.Model = model;
            databaseMapping.Database = database;

            databaseMapping.AddEntityContainerMapping(new EntityContainerMapping(model.Containers.Single()));

            return databaseMapping;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by test code.")]
        public static MetadataWorkspace ToMetadataWorkspace(this DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            var itemCollection = new EdmItemCollection(databaseMapping.Model);
            var storeItemCollection = new StoreItemCollection(databaseMapping.Database);
            var storageMappingItemCollection = databaseMapping.ToStorageMappingItemCollection(itemCollection, storeItemCollection);

            var workspace = new MetadataWorkspace(
                () => itemCollection,
                () => storeItemCollection,
                () => storageMappingItemCollection);

            new CodeFirstOSpaceLoader().LoadTypes(itemCollection, (ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace));

            return workspace;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static StorageMappingItemCollection ToStorageMappingItemCollection(
            this DbDatabaseMapping databaseMapping, EdmItemCollection itemCollection,
            StoreItemCollection storeItemCollection)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(itemCollection);
            DebugCheck.NotNull(storeItemCollection);

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

        public static EntityTypeMapping GetEntityTypeMapping(
            this DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);

            var mappings = databaseMapping.GetEntityTypeMappings(entityType);

            if (mappings.Count <= 1)
            {
                return mappings.FirstOrDefault();
            }

            // Return the property mapping
            return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
        }

        public static IList<EntityTypeMapping> GetEntityTypeMappings(
            this DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);

            // please don't convert this section of code to a Linq expression since
            // it is performance sensitive, especially for larger models.
            var mappings = new List<EntityTypeMapping>();
            foreach (var esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings)
            {
                foreach (var etm in esm.EntityTypeMappings)
                {
                    if (etm.EntityType == entityType)
                    {
                        mappings.Add(etm);
                    }
                }
            }
            return mappings;
        }

        public static EntityTypeMapping GetEntityTypeMapping(
            this DbDatabaseMapping databaseMapping, Type clrType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(clrType);

            // please don't convert this section of code to a Linq expression since
            // it is performance sensitive, especially for larger models.
            var mappings = new List<EntityTypeMapping>();
            foreach (var esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings)
            {
                foreach (var etm in esm.EntityTypeMappings)
                {
                    if (etm.GetClrType() == clrType)
                    {
                        mappings.Add(etm);
                    }
                }
            }

            if (mappings.Count <= 1)
            {
                return mappings.FirstOrDefault();
            }

            // Return the property mapping
            return mappings.SingleOrDefault(m => m.IsHierarchyMapping);
        }

        public static IEnumerable<Tuple<ColumnMappingBuilder, EntityType>> GetComplexPropertyMappings(
            this DbDatabaseMapping databaseMapping, Type complexType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(complexType);

            return from esm in databaseMapping.EntityContainerMappings.Single().EntitySetMappings
                   from etm in esm.EntityTypeMappings
                   from etmf in etm.MappingFragments
                   from epm in etmf.ColumnMappings
                   where epm.PropertyPath
                            .Any(
                                p => p.IsComplexType
                                     && p.ComplexType.GetClrType() == complexType)
                   select Tuple.Create(epm, etmf.Table);
        }

        public static IEnumerable<ModificationFunctionParameterBinding> GetComplexParameterBindings(
            this DbDatabaseMapping databaseMapping, Type complexType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(complexType);

            return from esm in databaseMapping.GetEntitySetMappings()
                   from mfm in esm.ModificationFunctionMappings
                   from pb in mfm.PrimaryParameterBindings
                   where pb.MemberPath.Members
                           .OfType<EdmProperty>()
                           .Any(
                               p => p.IsComplexType
                                    && p.ComplexType.GetClrType() == complexType)
                   select pb;
        }

        public static EntitySetMapping GetEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EntitySet entitySet)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entitySet);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings
                .SingleOrDefault(e => e.EntitySet == entitySet);
        }

        public static IEnumerable<EntitySetMapping> GetEntitySetMappings(this DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .EntitySetMappings;
        }

        public static IEnumerable<AssociationSetMapping> GetAssociationSetMappings(
            this DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            return databaseMapping
                .EntityContainerMappings
                .Single()
                .AssociationSetMappings;
        }

        public static EntitySetMapping AddEntitySetMapping(
            this DbDatabaseMapping databaseMapping, EntitySet entitySet)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entitySet);

            var entitySetMapping = new EntitySetMapping(entitySet, null);

            databaseMapping
                .EntityContainerMappings
                .Single()
                .AddSetMapping(entitySetMapping);

            return entitySetMapping;
        }

        public static AssociationSetMapping AddAssociationSetMapping(
            this DbDatabaseMapping databaseMapping, AssociationSet associationSet, EntitySet entitySet)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(associationSet);

            var containerMapping = databaseMapping
                .EntityContainerMappings
                .Single();

            var associationSetMapping
                = new AssociationSetMapping(associationSet, entitySet, containerMapping).Initialize();

            containerMapping.AddSetMapping(associationSetMapping);

            return associationSetMapping;
        }
    }
}
