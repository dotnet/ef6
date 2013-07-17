// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class Net40DefaultDbProviderFactoryResolver : IDbProviderFactoryResolver
    {
        private readonly ConcurrentDictionary<Type, DbProviderFactory> _cache
            = new ConcurrentDictionary<Type, DbProviderFactory>(
                new[]
                    {
                        new KeyValuePair<Type, DbProviderFactory>(typeof(EntityConnection), EntityProviderFactory.Instance)
                    });

        private readonly ProviderRowFinder _finder;

        public Net40DefaultDbProviderFactoryResolver()
            : this(new ProviderRowFinder())
        {
        }

        public Net40DefaultDbProviderFactoryResolver(
            ProviderRowFinder finder)
        {
            DebugCheck.NotNull(finder);

            _finder = finder;
        }

        public DbProviderFactory ResolveProviderFactory(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

            return GetProviderFactory(connection, DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>());
        }

        public DbProviderFactory GetProviderFactory(DbConnection connection, IEnumerable<DataRow> dataRows)
        {
            DebugCheck.NotNull(connection);
            DebugCheck.NotNull(dataRows);

            var connectionType = connection.GetType();

            return _cache.GetOrAdd(
                connectionType,
                t =>
                    {
                        var row = _finder.FindRow(t, r => ExactMatch(r, t), dataRows)
                                  ?? _finder.FindRow(null, r => ExactMatch(r, t), dataRows)
                                  ?? _finder.FindRow(t, r => AssignableMatch(r, t), dataRows)
                                  ?? _finder.FindRow(null, r => AssignableMatch(r, t), dataRows);

                        if (row == null)
                        {
                            throw new NotSupportedException(Strings.ProviderNotFound(connection.ToString()));
                        }

                        return DbProviderFactories.GetFactory(row);
                    });
        }

        private static bool ExactMatch(DataRow row, Type connectionType)
        {
            DebugCheck.NotNull(row);
            DebugCheck.NotNull(connectionType);

            return DbProviderFactories.GetFactory(row).CreateConnection().GetType() == connectionType;
        }

        private static bool AssignableMatch(DataRow row, Type connectionType)
        {
            DebugCheck.NotNull(row);
            DebugCheck.NotNull(connectionType);

            return connectionType.IsInstanceOfType(DbProviderFactories.GetFactory(row).CreateConnection());
        }
    }
}
