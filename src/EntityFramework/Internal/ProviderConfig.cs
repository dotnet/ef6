// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal class ProviderConfig
    {
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        public ProviderConfig()
        {
        }

        public ProviderConfig(EntityFrameworkSection entityFrameworkSettings)
        {
            Contract.Requires(entityFrameworkSettings != null);

            _entityFrameworkSettings = entityFrameworkSettings;
        }

        public virtual DbProviderServices TryGetDbProviderServices(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerElement = TryGetProviderElement(providerInvariantName);

            return providerElement != null && providerElement.ProviderTypeName != null
                       ? new ProviderServicesFactory().GetInstance(providerElement.ProviderTypeName, providerInvariantName)
                       : null;
        }

        public virtual Func<MigrationSqlGenerator> TryGetMigrationSqlGeneratorFactory(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerElement = TryGetProviderElement(providerInvariantName);

            if (providerElement != null
                && providerElement.SqlGeneratorElement != null
                && !string.IsNullOrWhiteSpace(providerElement.SqlGeneratorElement.SqlGeneratorTypeName))
            {
                var typeName = providerElement.SqlGeneratorElement.SqlGeneratorTypeName;
                var providerType = Type.GetType(typeName, throwOnError: false);

                if (providerType == null)
                {
                    throw new InvalidOperationException(Strings.SqlGeneratorTypeMissing(typeName, providerInvariantName));
                }
                return () => providerType.CreateInstance<MigrationSqlGenerator>(Strings.CreateInstance_BadSqlGeneratorType);
            }

            return () => null;
        }

        public virtual DbSpatialServices TryGetSpatialProvider()
        {
            var providerTypeName = _entityFrameworkSettings.SpatialProviderTypeName;

            if (string.IsNullOrWhiteSpace(providerTypeName))
            {
                return null;
            }

            var providerType = Type.GetType(providerTypeName, throwOnError: false);

            if (providerType == null)
            {
                throw new InvalidOperationException(Strings.DbSpatialServicesTypeNotFound(providerTypeName));
            }

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var instanceMember = providerType.GetProperty("Instance", bindingFlags)
                                 ?? (MemberInfo)providerType.GetField("Instance", bindingFlags);
            if (instanceMember == null)
            {
                throw new InvalidOperationException(Strings.DbSpatialServices_InstanceMissing(providerTypeName));
            }

            var providerInstance = instanceMember.GetValue() as DbSpatialServices;
            if (providerInstance == null)
            {
                throw new InvalidOperationException(Strings.DbSpatialServices_NotDbSpatialServices(providerTypeName));
            }

            return providerInstance;
        }

        private ProviderElement TryGetProviderElement(string providerInvariantName)
        {
            return _entityFrameworkSettings.Providers
                .OfType<ProviderElement>()
                .FirstOrDefault(
                    e => providerInvariantName.Equals(e.InvariantName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
