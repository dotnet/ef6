namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Reflection;

    internal class ProviderServicesFactory
    {
        public virtual DbProviderServices GetInstance(string providerTypeName, string providerInvariantName)
        {
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

        public virtual DbProviderServices GetInstanceByConvention(string providerInvariantName)
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

            return GetInstance(
                string.Format(
                    CultureInfo.InvariantCulture,
                    providerTemplate,
                    new AssemblyName(typeof(DbContext).Assembly.FullName).Version,
                    ", Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                providerInvariantName);
        }
    }
}