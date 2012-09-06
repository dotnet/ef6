// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

#if NET40
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
#endif

    internal class DefaultDbProviderFactoryService : IDbProviderFactoryService
    {
#if NET40
        private readonly ConcurrentDictionary<Type, DbProviderFactory> _cache
            = new ConcurrentDictionary<Type, DbProviderFactory>(
                new[]
                    {
                        new KeyValuePair<Type, DbProviderFactory>(typeof(EntityConnection), EntityProviderFactory.Instance)
                    });

        private readonly ProviderRowFinder _finder;
#endif

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "finder")]
        public DefaultDbProviderFactoryService(ProviderRowFinder finder = null)
        {
#if NET40
            _finder = finder ?? new ProviderRowFinder();
#endif
        }

        public DbProviderFactory GetProviderFactory(DbConnection connection)
        {
#if NET40
            var connectionType = connection.GetType();

            return _cache.GetOrAdd(
                connectionType,
                t =>
                    {
                        var row = _finder.FindRow(t, r => ExactMatch(r, t))
                                  ?? _finder.FindRow(null, r => ExactMatch(r, t))
                                  ?? _finder.FindRow(t, r => AssignableMatch(r, t))
                                  ?? _finder.FindRow(null, r => AssignableMatch(r, t));

                        if (row == null)
                        {
                            throw new NotSupportedException(Strings.ProviderNotFound(connection.ToString()));
                        }

                        return DbProviderFactories.GetFactory(row);
                    });
#else
            return DbProviderFactories.GetFactory(connection);
#endif
        }

#if NET40
        private static bool ExactMatch(DataRow row, Type connectionType)
        {
            Contract.Requires(row != null);
            Contract.Requires(connectionType != null);

            return DbProviderFactories.GetFactory(row).CreateConnection().GetType() == connectionType;
        }

        private static bool AssignableMatch(DataRow row, Type connectionType)
        {
            Contract.Requires(row != null);
            Contract.Requires(connectionType != null);

            return connectionType.IsInstanceOfType(DbProviderFactories.GetFactory(row).CreateConnection());
        }
#endif
    }
}
