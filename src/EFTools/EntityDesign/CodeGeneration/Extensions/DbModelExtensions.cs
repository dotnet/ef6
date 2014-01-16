// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Linq;

    internal static class DbModelExtensions
    {
        private static readonly IDictionary<DbProviderInfo, DbProviderManifest> _providerManifestCache =
            new Dictionary<DbProviderInfo, DbProviderManifest>();

        public static DbProviderManifest GetProviderManifest(
            this DbModel model,
            IDbDependencyResolver dependencyResolver)
        {
            Debug.Assert(model != null, "model is null.");
            Debug.Assert(dependencyResolver != null, "dependencyResolver is null.");

            if (model.ProviderManifest != null)
            {
                return model.ProviderManifest;
            }

            DbProviderManifest providerManifest;
            if (!_providerManifestCache.TryGetValue(model.ProviderInfo, out providerManifest))
            {
                providerManifest = dependencyResolver
                    .GetService<DbProviderServices>(model.ProviderInfo.ProviderInvariantName)
                    .GetProviderManifest(model.ProviderInfo.ProviderManifestToken);
                _providerManifestCache.Add(model.ProviderInfo, providerManifest);
            }

            return providerManifest;
        }

        public static EdmProperty GetColumn(this DbModel model, EdmProperty property)
        {
            var entityType = property.DeclaringType;
            var entitySet = model.ConceptualModel.Container.EntitySets.First(s => s.ElementType == entityType);

            return model.ConceptualToStoreMapping
                .EntitySetMappings.First(m => m.EntitySet == entitySet)
                .EntityTypeMappings.First()
                .Fragments.First()
                .PropertyMappings.OfType<ScalarPropertyMapping>().First(m => m.Property == property)
                .Column;
        }
    }
}
