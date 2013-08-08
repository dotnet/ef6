// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Internal interface used to provide a non-typed way to store a reference to an object
    /// that knows the type and cardinality of the source end of a relationship
    /// </summary>
    internal interface IRelationshipFixer
    {
        /// <summary>
        /// Used during relationship fixup when the source end of the relationship is not
        /// yet in the relationships list, and needs to be created
        /// </summary>
        /// <param name="navigation"> RelationshipNavigation to be set on new RelatedEnd </param>
        /// <param name="relationshipManager"> RelationshipManager to use for creating the new end </param>
        /// <returns> Reference to the new collection or reference on the other end of the relationship </returns>
        RelatedEnd CreateSourceEnd(RelationshipNavigation navigation, RelationshipManager relationshipManager);
    }
}
