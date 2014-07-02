// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Base class for configuring an index.
    /// </summary>
    /// <typeparam name="TSelf">The type of the final concrete class.</typeparam>
    public abstract class IndexConfigurationBase<TSelf>
        where TSelf : IndexConfigurationBase<TSelf>
    {
        private readonly Properties.Index.IndexConfiguration _configuration;


        internal IndexConfigurationBase(Properties.Index.IndexConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _configuration = configuration;
        }

        internal Properties.Index.IndexConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Configures the index to be clustered.
        /// </summary>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public TSelf IsClustered()
        {
            return IsClustered(true);
        }

        /// <summary>
        /// Configures whether or not the index will be clustered.
        /// </summary>
        /// <param name="clustered"> Value indicating if the index should be clustered or not. </param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public TSelf IsClustered(bool clustered)
        {
            Configuration.IsClustered = clustered;

            return (TSelf) this;
        }

        /// <summary>
        /// Configures the index to have a specific name.
        /// </summary>
        /// <param name="name"> Value indicating what the index name should be.</param>
        /// <returns> The same IndexConfigurationBase instance so that multiple calls can be chained. </returns>
        public TSelf HasName(string name)
        {
            Configuration.Name = name;

            return (TSelf) this;
        }
    }
}
