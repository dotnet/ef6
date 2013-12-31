// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class ColumnDiscoverer : IPropertyConfigurationDiscoverer
    {
        private readonly CodeHelper _code;
        private readonly IDbDependencyResolver _dependencyResolver;

        public ColumnDiscoverer(CodeHelper code)
            : this(code, DependencyResolver.Instance)
        {
            Debug.Assert(code != null, "code is null.");
        }

        // Internal for testing
        internal ColumnDiscoverer(CodeHelper code, IDbDependencyResolver dependencyResolver)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(dependencyResolver != null, "dependencyResolver is null.");

            _code = code;
            _dependencyResolver = dependencyResolver;
        }

        public IConfiguration Discover(EdmProperty property, DbModel model)
        {
            Debug.Assert(property != null, "property is null.");
            Debug.Assert(model != null, "model is null.");

            var columnProperty = model.GetColumn(property);

            string name = null;
            if (_code.Property(property) != columnProperty.Name)
            {
                name = columnProperty.Name;
            }

            var providerManifest = model.GetProviderManifest(_dependencyResolver);

            var defaultTypeName = providerManifest.GetStoreType(property.TypeUsage)
                .EdmType.Name;

            string typeName = null;
            if (!columnProperty.TypeName.EqualsIgnoreCase(defaultTypeName))
            {
                typeName = columnProperty.TypeName;
            }

            var entityType = (EntityType)property.DeclaringType;
            var keyIndex = entityType.KeyMembers.IndexOf(property);

            int? order = null;
            if (keyIndex != -1 && entityType.KeyMembers.Count > 1)
            {
                order = keyIndex;
            }

            if (name == null && typeName == null && order == null)
            {
                // By convention
                return null;
            }

            return new ColumnConfiguration { Name = name, TypeName = typeName, Order = order };
        }
    }
}

