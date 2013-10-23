// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    internal static class EntitySetDefiningQueryConverter
    {
        private const string StoreContainerName = "StoreModelContainer";
        private const string ModelContainerName = "ModelContainer";

        private const string MslTemplate =
            @"<Mapping Space=""C-S"" xmlns=""{0}"">" +
            @"  <EntityContainerMapping StorageEntityContainer=""" + StoreContainerName + @""" CdmEntityContainer=""" + ModelContainerName
            + @""" />" +
            @"</Mapping>";

        private const string SchemaAttributeName = "Schema";
        private const string NameAttributeName = "Name";

        public static IEnumerable<EntitySet> Convert(
            IList<EntitySet> sourceEntitySets,
            Version targetEntityFrameworkVersion,
            string providerInvariantName,
            string providerManifestToken,
            IDbDependencyResolver dependencyResolver)
        {
            Debug.Assert(sourceEntitySets != null, "sourceEntitySets != null");
            Debug.Assert(sourceEntitySets.All(e => e.DefiningQuery == null), "unexpected defining query");
            Debug.Assert(!string.IsNullOrWhiteSpace(providerInvariantName), "invalid providerInvariantName");
            Debug.Assert(!string.IsNullOrWhiteSpace(providerManifestToken), "invalid providerManifestToken");
            Debug.Assert(dependencyResolver != null, "dependencyResolver != null");

            if (!sourceEntitySets.Any())
            {
                // it's empty anyways
                return sourceEntitySets;
            }

            var providerServices = dependencyResolver.GetService<DbProviderServices>(providerInvariantName);
            Debug.Assert(providerServices != null, "providerServices != null");

            var transientWorkspace =
                CreateTransientMetadataWorkspace(
                    sourceEntitySets,
                    targetEntityFrameworkVersion,
                    providerInvariantName,
                    providerManifestToken,
                    providerServices.GetProviderManifest(providerManifestToken));

            return sourceEntitySets.Select(
                e => CloneWithDefiningQuery(
                    e,
                    CreateDefiningQuery(e, transientWorkspace, providerServices)));
        }

        // internal for testing
        internal static EntitySet CloneWithDefiningQuery(EntitySet sourceEntitySet, string definingQuery)
        {
            Debug.Assert(sourceEntitySet != null, "sourceEntitySet != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(definingQuery), "invalid definingQuery");

            var metadataProperties =
                sourceEntitySet.MetadataProperties.Where(p => p.PropertyKind != PropertyKind.System).ToList();

            // these properties make it possible for the designer to track 
            // back these types to their source db objects
            if (sourceEntitySet.Schema != null)
            {
                metadataProperties.Add(
                    StoreModelBuilder.CreateStoreModelBuilderMetadataProperty(
                        SchemaAttributeName,
                        sourceEntitySet.Schema));
            }

            if (sourceEntitySet.Table != null)
            {
                metadataProperties.Add(
                    StoreModelBuilder.CreateStoreModelBuilderMetadataProperty(
                        NameAttributeName,
                        sourceEntitySet.Table));
            }

            return
                EntitySet.Create(
                    sourceEntitySet.Name,
                    null,
                    null,
                    definingQuery,
                    sourceEntitySet.ElementType,
                    metadataProperties);
        }

        // internal for testing
        internal static string CreateDefiningQuery(EntitySet entitySet, MetadataWorkspace workspace, DbProviderServices providerServices)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            Debug.Assert(workspace != null, "workspace != null");
            Debug.Assert(providerServices != null, "providerServices != null");

            var inputBinding = entitySet.Scan().BindAs(entitySet.Name);

            var projectList = new List<KeyValuePair<string, DbExpression>>(entitySet.ElementType.Members.Count);
            foreach (var member in entitySet.ElementType.Members)
            {
                Debug.Assert(member.BuiltInTypeKind == BuiltInTypeKind.EdmProperty, "Every member must be a edmproperty");
                var propertyInfo = (EdmProperty)member;
                projectList.Add(
                    new KeyValuePair<string, DbExpression>(
                        member.Name,
                        inputBinding.Variable.Property(propertyInfo)));
            }
            var query = inputBinding.Project(DbExpressionBuilder.NewRow(projectList));
            var dbCommandTree = new DbQueryCommandTree(workspace, DataSpace.SSpace, query);

            return providerServices.CreateCommandDefinition(dbCommandTree).CreateCommand().CommandText;
        }

        // internal for testing
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static MetadataWorkspace CreateTransientMetadataWorkspace(
            IList<EntitySet> sourceEntitySets,
            Version targetEntityFrameworkVersion,
            string providerInvariantName,
            string providerManifestToken,
            DbProviderManifest providerManifest)
        {
            Debug.Assert(sourceEntitySets != null, "sourceEntitySets != null");
            Debug.Assert(targetEntityFrameworkVersion != null, "targetEntityFrameworkVersion != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(providerInvariantName), "invalid providerInvariantName");
            Debug.Assert(!string.IsNullOrWhiteSpace(providerManifestToken), "invalid providerManifestToken");
            Debug.Assert(providerManifest != null, "providerManifest != null");

            var targetDoubleEntityFrameworkVersion = EntityFrameworkVersion.VersionToDouble(targetEntityFrameworkVersion);

            var storeModel =
                EdmModel.CreateStoreModel(
                    EntityContainer.Create(
                        StoreContainerName,
                        DataSpace.SSpace,
                        sourceEntitySets,
                        Enumerable.Empty<EdmFunction>(),
                        null),
                    new DbProviderInfo(providerInvariantName, providerManifestToken),
                    providerManifest,
                    targetDoubleEntityFrameworkVersion);

            foreach (var entityType in sourceEntitySets.Select(e => e.ElementType))
            {
                storeModel.AddItem(entityType);
            }

            var storeItemCollection = new StoreItemCollection(storeModel);

            var edmItemCollection =
                new EdmItemCollection(
                    EdmModel.CreateConceptualModel(
                        EntityContainer.Create(
                            ModelContainerName,
                            DataSpace.CSpace,
                            Enumerable.Empty<EntitySet>(),
                            Enumerable.Empty<EdmFunction>(),
                            null),
                        targetDoubleEntityFrameworkVersion));

            var msl =
                string.Format(
                    CultureInfo.InvariantCulture,
                    MslTemplate,
                    SchemaManager.GetMSLNamespaceName(targetEntityFrameworkVersion));

            StorageMappingItemCollection mappingItemCollection;

            using (var stringReader = new StringReader(msl))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    mappingItemCollection = new StorageMappingItemCollection(edmItemCollection, storeItemCollection, new[] { reader });
                }
            }

            return new MetadataWorkspace(
                () => edmItemCollection,
                () => storeItemCollection,
                () => mappingItemCollection);
        }
    }
}
