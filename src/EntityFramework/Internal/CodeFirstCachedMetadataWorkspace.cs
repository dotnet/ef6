// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Implements ICachedMetadataWorkspace for a Code First model.
    /// </summary>
    internal class CodeFirstCachedMetadataWorkspace : ICachedMetadataWorkspace
    {
        #region Fields and constructors

        private readonly MetadataWorkspace _metadataWorkspace;
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly DbProviderInfo _providerInfo;
        private readonly string _defaultContainerName;

        /// <summary>
        /// Builds and stores the workspace based on the given code first configuration.
        /// </summary>
        /// <param name="databaseMapping"> The code first EDM model. </param>
        public CodeFirstCachedMetadataWorkspace(DbDatabaseMapping databaseMapping)
        {
            DebugCheck.NotNull(databaseMapping);

            _providerInfo = databaseMapping.ProviderInfo;
            _metadataWorkspace = databaseMapping.ToMetadataWorkspace();
            _assemblies = databaseMapping.Model.GetClrTypes().Select(t => t.Assembly).Distinct().ToList();

            Debug.Assert(databaseMapping.Model.Containers.Count() == 1, "Expecting Code First to create only one container.");

            _defaultContainerName = databaseMapping.Model.Containers.First().Name;
        }

        #endregion

        #region ICachedMetadataWorkspace implementation

        /// <summary>
        /// Gets the <see cref="MetadataWorkspace" />.
        /// If the workspace is not compatible with the provider manifest obtained from the given
        /// connection then an exception is thrown.
        /// </summary>
        /// <param name="connection"> The connection to use to create or check SSDL provider info. </param>
        /// <returns> The workspace. </returns>
        public MetadataWorkspace GetMetadataWorkspace(DbConnection connection)
        {
            DebugCheck.NotNull(connection);

            var providerInvariantName = connection.GetProviderInvariantName();

            if (!string.Equals(_providerInfo.ProviderInvariantName, providerInvariantName, StringComparison.Ordinal))
            {
                throw Error.CodeFirstCachedMetadataWorkspace_SameModelDifferentProvidersNotSupported();
            }

            return _metadataWorkspace;
        }

        /// <summary>
        /// The default container name for code first is the container name that is set from the DbModelBuilder
        /// </summary>
        public string DefaultContainerName
        {
            get { return _defaultContainerName; }
        }

        /// <summary>
        /// The list of assemblies that contain entity types for this workspace, which may be empty, but
        /// will never be null.
        /// </summary>
        public IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        /// <summary>
        /// The provider info used to construct the workspace.
        /// </summary>
        public DbProviderInfo ProviderInfo
        {
            get { return _providerInfo; }
        }

        #endregion
    }
}
