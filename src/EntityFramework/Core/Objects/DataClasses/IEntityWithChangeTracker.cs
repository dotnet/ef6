// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Minimum interface that a data class must implement in order to be managed by a change tracker.
    /// </summary>
    public interface IEntityWithChangeTracker
    {
        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityChangeTracker" /> used to report changes.
        /// </summary>
        /// <param name="changeTracker">
        /// The <see cref="T:System.Data.Entity.Core.Objects.DataClasses.IEntityChangeTracker" /> used to report changes.
        /// </param>
        void SetChangeTracker(IEntityChangeTracker changeTracker);
    }
}
