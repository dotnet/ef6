// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     A key used for resolving <see cref="ExecutionStrategy"/>. It consists of the ADO.NET provider invariant name
    ///     and the database server address as specified in the connection string.
    /// </summary>
    public class ExecutionStrategyKey
    {
        public ExecutionStrategyKey(string invariantProviderName, string dataSource)
        {
            Check.NotEmpty(invariantProviderName, "invariantProviderName");
            Check.NotEmpty(dataSource, "dataSource");

            InvariantProviderName = invariantProviderName;
            DataSourceName = dataSource;
        }

        public string InvariantProviderName { get; private set; }
        public string DataSourceName { get; private set; }

        public override bool Equals(object obj)
        {
            var otherKey = obj as ExecutionStrategyKey;
            if (ReferenceEquals(otherKey, null))
            {
                return false;
            }

            return InvariantProviderName == otherKey.InvariantProviderName
                   && DataSourceName == otherKey.DataSourceName;
        }

        public override int GetHashCode()
        {
            return InvariantProviderName.GetHashCode() ^ DataSourceName.GetHashCode();
        }
    }
}
