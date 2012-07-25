// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Interface that defines an entity containing a key.
    /// </summary>
    public interface IEntityWithKey
    {
        /// <summary>
        /// Returns the EntityKey for this entity.
        /// If an object is being managed by a change tracker, it is expected that
        /// IEntityChangeTracker methods EntityMemberChanging and EntityMemberChanged will be
        /// used to report changes on EntityKey. This allows the change tracker to validate the
        /// EntityKey's new value and to verify if the change tracker is in a state where it can
        /// allow updates to the EntityKey.
        /// </summary>
        EntityKey EntityKey { get; set; }
    }
}
