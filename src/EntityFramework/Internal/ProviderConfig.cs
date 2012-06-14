namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal class ProviderConfig
    {
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        public ProviderConfig(EntityFrameworkSection entityFrameworkSettings)
        {
            _entityFrameworkSettings = entityFrameworkSettings;
        }

        public DbProviderServices GetDbProviderServices(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerSection =
                _entityFrameworkSettings.Providers.OfType<ProviderElement>().FirstOrDefault(
                    e => providerInvariantName.Equals(e.InvariantName, StringComparison.OrdinalIgnoreCase));

            var providerTypeName = providerSection != null
                                       ? providerSection.ProviderTypeName
                                       : GetProviderTypeByConvention(providerInvariantName);
            var providerType = Type.GetType(providerTypeName, throwOnError: false);

            if (providerType == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_ProviderTypeMissing(providerTypeName, providerInvariantName));
            }

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var instanceMember = providerType.GetProperty("Instance", bindingFlags)
                                 ?? (MemberInfo)providerType.GetField("Instance", bindingFlags);
            if (instanceMember == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_InstanceMissing(providerTypeName));
            }

            var providerInstance = instanceMember.GetValue() as DbProviderServices;
            if (providerInstance == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_NotDbProviderServices(providerTypeName));
            }

            return providerInstance;
        }

        public virtual string GetProviderTypeByConvention(string providerInvariantName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(providerInvariantName));

            var providerTemplate =
                providerInvariantName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase)
                    ? "System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer, Version={0}{1}"
                    : providerInvariantName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase)
                          ? "System.Data.Entity.SqlServerCompact.SqlCeProviderServices, EntityFramework.SqlServerCompact, Version={0}{1}"
                          : null;

            if (providerTemplate == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_NoProviderFound(providerInvariantName));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                providerTemplate,
                new AssemblyName(typeof(DbContext).Assembly.FullName).Version,
                ", Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }
    }
}
