// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal static class DbProviderFactoryExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbProviderFactory factory)
        {
            Contract.Requires(factory != null);

            const int assemblyQualifiedNameIndex = 3;
            const int invariantNameIndex = 2;

            var connectionProviderFactoryType = factory.GetType();
            var connectionProviderFactoryAssemblyName = new AssemblyName(
                connectionProviderFactoryType.Assembly.FullName);

            foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
            {
                var assemblyQualifiedTypeName = (string)row[assemblyQualifiedNameIndex];

                AssemblyName rowProviderFactoryAssemblyName = null;

                // parse the provider factory assembly qualified type name
                Type.GetType(
                    assemblyQualifiedTypeName,
                    a =>
                        {
                            rowProviderFactoryAssemblyName = a;

                            return null;
                        },
                    (_, __, ___) => null);

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
                                return (string)row[invariantNameIndex];
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail("GetFactory failed with: " + ex);
                            // Ignore bad providers.
                        }
                    }
                }
            }

            throw Error.ModelBuilder_ProviderNameNotFound(factory);
        }

        internal static DbProviderServices GetProviderServices(this DbProviderFactory factory)
        {
            Contract.Requires(factory != null);

            // The EntityClient provider invariant name is not normally registered so we can't use
            // the normal method for looking up this factory.
            if (factory is EntityProviderFactory)
            {
                return EntityProviderServices.Instance;
            }

            var invariantName = factory.GetProviderInvariantName();
            Contract.Assert(invariantName != null);

            return DbConfiguration.Instance.GetProvider(invariantName);
        }
    }
}
