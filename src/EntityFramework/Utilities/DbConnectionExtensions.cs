namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal static class DbConnectionExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbConnection connection)
        {
            Contract.Requires(connection != null);

            var connectionProviderFactoryType = DbProviderServices.GetProviderFactory(connection).GetType();
            var connectionProviderFactoryAssemblyName = new AssemblyName(
                connectionProviderFactoryType.Assembly.FullName);

            foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
            {
                var assemblyQualifiedTypeName = (string)row[3];

                AssemblyName rowProviderFactoryAssemblyName = null;

                // parse the provider factory assembly qualified type name
                Type.GetType(
                    assemblyQualifiedTypeName,
                    a =>
                        {
                            rowProviderFactoryAssemblyName = a;

                            return null;
                        },
                    (_, __, ___) => { return null; });

                if (rowProviderFactoryAssemblyName != null)
                {
                    if (string.Equals(
                        connectionProviderFactoryAssemblyName.Name,
                        rowProviderFactoryAssemblyName.Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var providerFactory = DbProviderFactories.GetFactory(row);

                            if (providerFactory.GetType().Equals(connectionProviderFactoryType))
                            {
                                return (string)row[2];
                            }
                        }
                        catch
                        {
                            // Ignore bad providers.
                        }
                    }
                }
            }

            throw Error.ModelBuilder_ProviderNameNotFound(connection);
        }

        public static DbProviderInfo GetProviderInfo(
            this DbConnection connection, out DbProviderManifest providerManifest)
        {
            Contract.Requires(connection != null);

            var providerServices = DbProviderServices.GetProviderServices(connection);
            var providerManifestToken = providerServices.GetProviderManifestTokenChecked(connection);
            var providerInfo = new DbProviderInfo(connection.GetProviderInvariantName(), providerManifestToken);

            providerManifest = providerServices.GetProviderManifest(providerManifestToken);

            return providerInfo;
        }
    }
}
