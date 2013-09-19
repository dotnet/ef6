// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents a particular usage of a structure defined in EntityType. In the conceptual-model, this represents a set that can 
    /// query and persist entities. In the store-model it represents a table. 
    /// From a store-space model-convention it can be used to configure
    /// table name with <see cref="EntitySetBase.Table"/> property and table schema with <see cref="EntitySetBase.Schema"/> property.
    /// </summary>
    public class EntitySet : EntitySetBase
    {
        internal EntitySet()
        {
        }

        /// <summary>
        /// The constructor for constructing the EntitySet with a given name and an entity type
        /// </summary>
        /// <param name="name"> The name of the EntitySet </param>
        /// <param name="schema"> The db schema </param>
        /// <param name="table"> The db table </param>
        /// <param name="definingQuery"> The provider specific query that should be used to retrieve the EntitySet </param>
        /// <param name="entityType"> The entity type of the entities that this entity set type contains </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal EntitySet(string name, string schema, string table, string definingQuery, EntityType entityType)
            : base(name, schema, table, definingQuery, entityType)
        {
        }

        private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyDependents;
        private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyPrincipals;
        private volatile bool _hasForeignKeyRelationships;
        private volatile bool _hasIndependentRelationships;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntitySet; }
        }

        /// <summary>
        /// Gets the entity type of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" /> object that represents the entity type of this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// .
        /// </returns>
        public new virtual EntityType ElementType
        {
            get { return (EntityType)base.ElementType; }
        }

        /// <summary>
        /// Returns the associations and constraints where "this" EntitySet particpates as the Principal end.
        /// From the results of this list, you can retrieve the Dependent IRelatedEnds
        /// </summary>
        internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyDependents
        {
            get
            {
                if (_foreignKeyDependents == null)
                {
                    InitializeForeignKeyLists();
                }
                return _foreignKeyDependents;
            }
        }

        /// <summary>
        /// Returns the associations and constraints where "this" EntitySet particpates as the Dependent end.
        /// From the results of this list, you can retrieve the Principal IRelatedEnds
        /// </summary>
        internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyPrincipals
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _foreignKeyPrincipals;
            }
        }

        /// <summary>
        /// True if this entity set participates in any foreign key relationships, otherwise false.
        /// </summary>
        internal bool HasForeignKeyRelationships
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _hasForeignKeyRelationships;
            }
        }

        /// <summary>
        /// True if this entity set participates in any independent relationships, otherwise false.
        /// </summary>
        internal bool HasIndependentRelationships
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _hasIndependentRelationships;
            }
        }

        private void InitializeForeignKeyLists()
        {
            var dependents = new List<Tuple<AssociationSet, ReferentialConstraint>>();
            var principals = new List<Tuple<AssociationSet, ReferentialConstraint>>();
            var foundFkRelationship = false;
            var foundIndependentRelationship = false;
            foreach (var associationSet in MetadataHelper.GetAssociationsForEntitySet(this))
            {
                if (associationSet.ElementType.IsForeignKey)
                {
                    foundFkRelationship = true;
                    Debug.Assert(associationSet.ElementType.ReferentialConstraints.Count == 1, "Expected exactly one constraint for FK");
                    var constraint = associationSet.ElementType.ReferentialConstraints[0];
                    if (constraint.ToRole.GetEntityType().IsAssignableFrom(ElementType)
                        ||
                        ElementType.IsAssignableFrom(constraint.ToRole.GetEntityType()))
                    {
                        // Dependents
                        dependents.Add(new Tuple<AssociationSet, ReferentialConstraint>(associationSet, constraint));
                    }
                    if (constraint.FromRole.GetEntityType().IsAssignableFrom(ElementType)
                        ||
                        ElementType.IsAssignableFrom(constraint.FromRole.GetEntityType()))
                    {
                        // Principals
                        principals.Add(new Tuple<AssociationSet, ReferentialConstraint>(associationSet, constraint));
                    }
                }
                else
                {
                    foundIndependentRelationship = true;
                }
            }

            _hasForeignKeyRelationships = foundFkRelationship;
            _hasIndependentRelationships = foundIndependentRelationship;

            var readOnlyDependents = dependents.AsReadOnly();
            var readOnlyPrincipals = principals.AsReadOnly();

            Interlocked.CompareExchange(ref _foreignKeyDependents, readOnlyDependents, null);
            Interlocked.CompareExchange(ref _foreignKeyPrincipals, readOnlyPrincipals, null);
        }

        /// <summary>
        /// The factory method for constructing the EntitySet object.
        /// </summary>
        /// <param name="name">The name of the EntitySet.</param>
        /// <param name="schema">The db schema. Can be null.</param>
        /// <param name="table">The db table. Can be null.</param>
        /// <param name="definingQuery">
        /// The provider specific query that should be used to retrieve data for this EntitySet. Can be null.
        /// </param>
        /// <param name="entityType">The entity type of the entities that this entity set type contains.</param>
        /// <param name="metadataProperties">
        /// Metadata properties that will be added to the newly created EntitySet. Can be null.
        /// </param>
        /// <exception cref="System.ArgumentException">Thrown if the name argument is null or empty string.</exception>
        /// <notes>The newly created EntitySet will be read only.</notes>
        public static EntitySet Create(
            string name, string schema, string table, string definingQuery, EntityType entityType,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(entityType, "entityType");

            var entitySet = new EntitySet(name, schema, table, definingQuery, entityType);

            if (metadataProperties != null)
            {
                entitySet.AddMetadataProperties(metadataProperties.ToList());
            }

            entitySet.SetReadOnly();
            return entitySet;
        }
    }
}
