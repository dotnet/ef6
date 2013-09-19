// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Groups a pair of strings that identify a provider and server version together into a single object.
    /// </summary>
    /// <remarks>
    /// Instances of this class act as the key for resolving a <see cref="DbSpatialServices" /> for a specific
    /// provider from a <see cref="IDbDependencyResolver" />. This is typically used when registering spatial services
    /// in <see cref="DbConfiguration" /> or when the spatial services specific to a provider is
    /// resolved by an implementation of <see cref="DbProviderServices" />.
    /// </remarks>
    public sealed class DbProviderInfo
    {
        private readonly string _providerInvariantName;
        private readonly string _providerManifestToken;

        /// <summary>
        /// Creates a new object for a given provider invariant name and manifest token.
        /// </summary>
        /// <param name="providerInvariantName">
        /// A string that identifies that provider. For example, the SQL Server
        /// provider uses the string "System.Data.SqlCient".
        /// </param>
        /// <param name="providerManifestToken">
        /// A string that identifies that version of the database server being used. For example, the SQL Server
        /// provider uses the string "2008" for SQL Server 2008. This cannot be null but may be empty.
        /// The manifest token is sometimes referred to as a version hint.
        /// </param>
        public DbProviderInfo(string providerInvariantName, string providerManifestToken)
        {
            Check.NotEmpty(providerInvariantName, "providerInvariantName");
            Check.NotNull(providerManifestToken, "providerManifestToken");

            _providerInvariantName = providerInvariantName;
            _providerManifestToken = providerManifestToken;
        }

        /// <summary>
        /// A string that identifies that provider. For example, the SQL Server
        /// provider uses the string "System.Data.SqlCient".
        /// </summary>
        public string ProviderInvariantName
        {
            get { return _providerInvariantName; }
        }

        /// <summary>
        /// A string that identifies that version of the database server being used. For example, the SQL Server
        /// provider uses the string "2008" for SQL Server 2008. This cannot be null but may be empty.
        /// </summary>
        public string ProviderManifestToken
        {
            get { return _providerManifestToken; }
        }

        private bool Equals(DbProviderInfo other)
        {
            return string.Equals(_providerInvariantName, other._providerInvariantName)
                   && string.Equals(_providerManifestToken, other._providerManifestToken);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var asKey = obj as DbProviderInfo;
            return asKey != null && Equals(asKey);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_providerInvariantName.GetHashCode() * 397) ^ _providerManifestToken.GetHashCode();
            }
        }
    }
}
