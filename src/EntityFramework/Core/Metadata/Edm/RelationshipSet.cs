// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// Class for representing a relationship set
    /// </summary>
    public abstract class RelationshipSet : EntitySetBase
    {
        /// <summary>
        /// The constructor for constructing the RelationshipSet with a given name and an relationship type
        /// </summary>
        /// <param name="name"> The name of the RelationshipSet </param>
        /// <param name="schema"> The db schema </param>
        /// <param name="table"> The db table </param>
        /// <param name="definingQuery"> The provider specific query that should be used to retrieve the EntitySet </param>
        /// <param name="relationshipType"> The entity type of the entities that this entity set type contains </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal RelationshipSet(string name, string schema, string table, string definingQuery, RelationshipType relationshipType)
            : base(name, schema, table, definingQuery, relationshipType)
        {
        }

        /// <summary>
        /// Gets the relationship type of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" />.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipType" /> object that represents the relationship type of this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" />
        /// .
        /// </returns>
        public new RelationshipType ElementType
        {
            get { return (RelationshipType)base.ElementType; }
        }

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.RelationshipSet; }
        }
    }
}
