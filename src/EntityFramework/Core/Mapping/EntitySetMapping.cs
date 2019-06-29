// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents the Mapping metadata for an EntitySet in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// This class represents the metadata for the EntitySetMapping elements in the
    /// above example. And it is possible to access the EntityTypeMaps underneath it.
    /// </example>
    public class EntitySetMapping : EntitySetBaseMapping
    {
        private readonly EntitySet _entitySet;
        private readonly List<EntityTypeMapping> _entityTypeMappings;
        private readonly List<EntityTypeModificationFunctionMapping> _modificationFunctionMappings;
        private Lazy<List<AssociationSetEnd>> _implicitlyMappedAssociationSetEnds;

        /// <summary>
        /// Initialiazes a new EntitySetMapping instance.
        /// </summary>
        /// <param name="entitySet">The entity set to be mapped.</param>
        /// <param name="containerMapping">The parent container mapping.</param>
        public EntitySetMapping(EntitySet entitySet, EntityContainerMapping containerMapping)
            : base(containerMapping)
        {
            Check.NotNull(entitySet, "entitySet");

            _entitySet = entitySet;
            _entityTypeMappings = new List<EntityTypeMapping>();
            _modificationFunctionMappings = new List<EntityTypeModificationFunctionMapping>();
            _implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(
                InitializeImplicitlyMappedAssociationSetEnds);
        }

        /// <summary>
        /// Gets the entity set that is mapped.
        /// </summary>
        public EntitySet EntitySet
        {
            get { return _entitySet; }
        }

        internal override EntitySetBase Set
        {
            get { return EntitySet; }
        }

        /// <summary>
        /// Gets the contained entity type mappings.
        /// </summary>
        public ReadOnlyCollection<EntityTypeMapping> EntityTypeMappings
        {
            get { return new ReadOnlyCollection<EntityTypeMapping>(_entityTypeMappings); }
        }

        internal override IEnumerable<TypeMapping> TypeMappings
        {
            get { return _entityTypeMappings; }
        }

        /// <summary>
        /// Gets the corresponding function mappings.
        /// </summary>
        public ReadOnlyCollection<EntityTypeModificationFunctionMapping> ModificationFunctionMappings
        {
            get { return new ReadOnlyCollection<EntityTypeModificationFunctionMapping>(_modificationFunctionMappings); }
        }

        // Gets all association sets that are implicitly "covered" through function mappings.
        internal IEnumerable<AssociationSetEnd> ImplicitlyMappedAssociationSetEnds
        {
            get { return _implicitlyMappedAssociationSetEnds.Value; }
        }

        // Returns true if there are no Function Maps and no table Mapping fragments.
        internal override bool HasNoContent
        {
            get { return (_modificationFunctionMappings.Count == 0) ? base.HasNoContent : false; } 
        }

        /// <summary>
        /// Adds a type mapping.
        /// </summary>
        /// <param name="typeMapping">The type mapping to add.</param>
        public void AddTypeMapping(EntityTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _entityTypeMappings.Add(typeMapping);
        }

        /// <summary>
        /// Removes a type mapping.
        /// </summary>
        /// <param name="typeMapping">The type mapping to remove.</param>
        public void RemoveTypeMapping(EntityTypeMapping typeMapping)
        {
            Check.NotNull(typeMapping, "typeMapping");
            ThrowIfReadOnly();

            _entityTypeMappings.Remove(typeMapping);
        }

        internal void ClearModificationFunctionMappings()
        {
            Debug.Assert(!IsReadOnly);

            _modificationFunctionMappings.Clear();
        }

        /// <summary>
        /// Adds a function mapping.
        /// </summary>
        /// <param name="modificationFunctionMapping">The function mapping to add.</param>
        public void AddModificationFunctionMapping(EntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");
            ThrowIfReadOnly();

            AssertModificationFunctionMappingInvariants(modificationFunctionMapping);

            _modificationFunctionMappings.Add(modificationFunctionMapping);

            if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
            {
                _implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(
                    InitializeImplicitlyMappedAssociationSetEnds);
            }
        }

        /// <summary>
        /// Removes a function mapping.
        /// </summary>
        /// <param name="modificationFunctionMapping">The function mapping to remove.</param>
        public void RemoveModificationFunctionMapping(EntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");
            ThrowIfReadOnly();

            _modificationFunctionMappings.Remove(modificationFunctionMapping);

            if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
            {
                _implicitlyMappedAssociationSetEnds = new Lazy<List<AssociationSetEnd>>(
                    InitializeImplicitlyMappedAssociationSetEnds);
            }
        }

        internal override void SetReadOnly()
        {
            _entityTypeMappings.TrimExcess();
            _modificationFunctionMappings.TrimExcess();

            if (_implicitlyMappedAssociationSetEnds.IsValueCreated)
            {
                _implicitlyMappedAssociationSetEnds.Value.TrimExcess();
            }

            SetReadOnly(_entityTypeMappings);
            SetReadOnly(_modificationFunctionMappings);

            base.SetReadOnly();
        }

        // Requires:
        // - Function mapping refers to a sub-type of this entity set's element type.
        // - Function mappings for types are not redundantly specified
        [Conditional("DEBUG")]
        private void AssertModificationFunctionMappingInvariants(EntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            Debug.Assert(
                modificationFunctionMapping.EntityType.Equals(Set.ElementType) ||
                Helper.IsSubtypeOf(modificationFunctionMapping.EntityType, Set.ElementType),
                "attempting to add a modification function mapping with the wrong entity type");
            foreach (var existingMapping in _modificationFunctionMappings)
            {
                Debug.Assert(
                    !existingMapping.EntityType.Equals(modificationFunctionMapping.EntityType),
                    "modification function mapping already exists for this type");
            }
        }

        private List<AssociationSetEnd> InitializeImplicitlyMappedAssociationSetEnds()
        {
            var implicitlyMappedAssociationSetEnds = new List<AssociationSetEnd>();

            foreach (var modificationFunctionMapping in _modificationFunctionMappings)
            {
                // check if any association sets are indirectly mapped within this function mapping
                // through association navigation bindings
                if (null != modificationFunctionMapping.DeleteFunctionMapping)
                {
                    implicitlyMappedAssociationSetEnds.AddRange(
                        modificationFunctionMapping.DeleteFunctionMapping.CollocatedAssociationSetEnds);
                }
                if (null != modificationFunctionMapping.InsertFunctionMapping)
                {
                    implicitlyMappedAssociationSetEnds.AddRange(
                        modificationFunctionMapping.InsertFunctionMapping.CollocatedAssociationSetEnds);
                }
                if (null != modificationFunctionMapping.UpdateFunctionMapping)
                {
                    implicitlyMappedAssociationSetEnds.AddRange(
                        modificationFunctionMapping.UpdateFunctionMapping.CollocatedAssociationSetEnds);
                }
            }

            if (IsReadOnly)
            {
                implicitlyMappedAssociationSetEnds.TrimExcess();
            }

            return implicitlyMappedAssociationSetEnds;
        }
    }
}
