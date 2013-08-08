// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Interface that a data class must implement if exposes relationships
    /// </summary>
    public interface IEntityWithRelationships
    {
        /// <summary>Returns the relationship manager that manages relationships for an instance of an entity type.</summary>
        /// <remarks>
        /// Classes that expose relationships must implement this property
        /// by constructing and setting RelationshipManager in their constructor.
        /// The implementation of this property should use the static method RelationshipManager.Create
        /// to create a new RelationshipManager when needed. Once created, it is expected that this
        /// object will be stored on the entity and will be provided through this property.
        /// </remarks>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.DataClasses.RelationshipManager" /> for this entity.
        /// </returns>
        RelationshipManager RelationshipManager { get; }
    }
}
