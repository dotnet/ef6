// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Configures an index.
    /// </summary>
    public class IndexConfiguration
    {
        private readonly Properties.Index.IndexConfiguration _configuration;

        internal IndexConfiguration(Properties.Index.IndexConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _configuration = configuration;
        }

        /// <summary>
        /// Configures the index to be unique.
        /// </summary>
        /// <returns> The same IndexConfiguration instance so that multiple calls can be chained. </returns>
        public IndexConfiguration IsUnique()
        {
            return IsUnique(true);
        }

        /// <summary>
        /// Configures whether the index will be unique.
        /// </summary>
        /// <param name="unique"> Value indicating if the index should be unique or not. </param>
        /// <returns> The same IndexConfiguration instance so that multiple calls can be chained. </returns>
        public IndexConfiguration IsUnique(bool unique)
        {
            _configuration.IsUnique = unique;

            return this;
        }

        /// <summary>
        /// Configures the index to be clustered.
        /// </summary>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public IndexConfiguration IsClustered()
        {
            return IsClustered(true);
        }

        /// <summary>
        /// Configures whether or not the index will be clustered.
        /// </summary>
        /// <param name="clustered"> Value indicating if the index should be clustered or not. </param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public IndexConfiguration IsClustered(bool clustered)
        {
            _configuration.IsClustered = clustered;

            return this;
        }

        /// <summary>
        /// Configures the index to have a specific name.
        /// </summary>
        /// <param name="name"> Value indicating what the index name should be.</param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public IndexConfiguration HasName(string name)
        {
            _configuration.Name = name;

            return this;
        }
    }
}
