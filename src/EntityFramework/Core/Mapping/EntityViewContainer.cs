// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Base class for the type created at design time to store the generated views.
    /// </summary>
    [Obsolete("The mechanism to provide pre-generated views has changed. Implement a class that derives from " +
        "System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCache and has a parameterless constructor, " +
        "then associate it with a type that derives from DbContext or ObjectContext " +
        "by using System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCacheTypeAttribute.",
        error: true)]
    public abstract class EntityViewContainer
    {
        /// <summary>
        /// Returns the cached dictionary of (ExtentName,EsqlView)
        /// </summary>
        internal IEnumerable<KeyValuePair<string, string>> ExtentViews
        {
            get
            {
                for (var i = 0; i < ViewCount; i++)
                {
                    yield return GetViewAt(i);
                }
            }
        }

        /// <summary>Returns the key/value pair at the specified index, which contains the view and its key.</summary>
        /// <returns>The key/value pair at  index , which contains the view and its key.</returns>
        /// <param name="index">The index of the view.</param>
        protected abstract KeyValuePair<string, string> GetViewAt(int index);

        /// <summary>
        /// Gets or sets the name of <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </summary>
        /// <returns>The container name.</returns>
        public string EdmEntityContainerName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> in storage schema.
        /// </summary>
        /// <returns>Container name.</returns>
        public string StoreEntityContainerName { get; set; }

        /// <summary>Hash value.</summary>
        /// <returns>Hash value.</returns>
        public string HashOverMappingClosure { get; set; }

        /// <summary>Hash value of views.</summary>
        /// <returns>Hash value.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverAll")]
        public string HashOverAllExtentViews { get; set; }

        /// <summary>Gets or sets view count.</summary>
        /// <returns>View count.</returns>
        public int ViewCount { get; protected set; }
    }
}
