// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Configures a primary key index.
    /// </summary>
    public class PrimaryKeyIndexConfiguration
    {
        private readonly Properties.Index.IndexConfiguration _configuration;

        internal PrimaryKeyIndexConfiguration(Properties.Index.IndexConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _configuration = configuration;
        }

        /// <summary>
        /// Configures the index to be clustered.
        /// </summary>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public PrimaryKeyIndexConfiguration IsClustered()
        {
            return IsClustered(true);
        }

        /// <summary>
        /// Configures whether or not the index will be clustered.
        /// </summary>
        /// <param name="clustered"> Value indicating if the index should be clustered or not. </param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public PrimaryKeyIndexConfiguration IsClustered(bool clustered)
        {
            _configuration.IsClustered = clustered;

            return this;
        }

        /// <summary>
        /// Configures the index to have a specific name.
        /// </summary>
        /// <param name="name"> Value indicating what the index name should be.</param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public PrimaryKeyIndexConfiguration HasName(string name)
        {
            _configuration.Name = name;

            return this;
        }
    }
}
