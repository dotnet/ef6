// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Configures an index.
    /// </summary>
    public class IndexConfiguration : IndexConfigurationBase<IndexConfiguration>
    {
        internal IndexConfiguration(Properties.Index.IndexConfiguration configuration)
            : base(configuration)
        {
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
            Configuration.IsUnique = unique;

            return this;
        }
    }
}
