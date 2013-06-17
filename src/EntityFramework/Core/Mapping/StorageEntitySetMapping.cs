// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Represents the Mapping metadata for an EnitytSet in CS space.
    /// </summary>
    /// <example>
    ///     For Example if conceptually you could represent the CS MSL file as following
    ///     --Mapping
    ///     --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --EntityTypeMapping
    ///     --MappingFragment
    ///     --AssociationSetMapping
    ///     --AssociationTypeMapping
    ///     --MappingFragment
    ///     This class represents the metadata for the EntitySetMapping elements in the
    ///     above example. And it is possible to access the EntityTypeMaps underneath it.
    /// </example>
    internal class StorageEntitySetMapping : StorageSetMapping
    {
        /// <summary>
        ///     Construct a EntitySet mapping object
        /// </summary>
        /// <param name="extent"> EntitySet metadata object </param>
        /// <param name="entityContainerMapping"> The entity Container Mapping that contains this Set mapping </param>
        public StorageEntitySetMapping(EntitySet extent, StorageEntityContainerMapping entityContainerMapping)
            : base(extent, entityContainerMapping)
        {
            Check.NotNull(extent, "extent");

            m_modificationFunctionMappings = new List<StorageEntityTypeModificationFunctionMapping>();
            m_implicitlyMappedAssociationSetEnds = new List<AssociationSetEnd>();
        }

        private readonly List<StorageEntityTypeModificationFunctionMapping> m_modificationFunctionMappings;
        private readonly List<AssociationSetEnd> m_implicitlyMappedAssociationSetEnds;

        /// <summary>
        ///     Gets all function mappings for this entity set.
        /// </summary>
        public IList<StorageEntityTypeModificationFunctionMapping> ModificationFunctionMappings
        {
            get { return m_modificationFunctionMappings.AsReadOnly(); }
        }

        public void ClearModificationFunctionMappings()
        {
            m_modificationFunctionMappings.Clear();
        }

        /// <summary>
        ///     Gets all association sets that are implicitly "covered" through function mappings.
        /// </summary>
        public IList<AssociationSetEnd> ImplicitlyMappedAssociationSetEnds
        {
            get { return m_implicitlyMappedAssociationSetEnds.AsReadOnly(); }
        }

        public IEnumerable<StorageEntityTypeMapping> EntityTypeMappings
        {
            get { return TypeMappings.OfType<StorageEntityTypeMapping>(); }
        }

        public EntitySet EntitySet
        {
            get { return (EntitySet)Set; }
        }

        /// <summary>
        ///     Whether the EntitySetMapping has empty content
        ///     Returns true if there are no Function Maps and no table Mapping fragments
        /// </summary>
        internal override bool HasNoContent
        {
            get
            {
                if (m_modificationFunctionMappings.Count != 0)
                {
                    return false;
                }
                return base.HasNoContent;
            }
        }

        /// <summary>
        ///     Requires:
        ///     - Function mapping refers to a sub-type of this entity set's element type
        ///     - Function mappings for types are not redundantly specified
        ///     Adds a new function mapping for this class.
        /// </summary>
        /// <param name="modificationFunctionMapping"> Function mapping to add. May not be null. </param>
        internal void AddModificationFunctionMapping(StorageEntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            AssertModificationFunctionMappingInvariants(modificationFunctionMapping);

            m_modificationFunctionMappings.Add(modificationFunctionMapping);

            // check if any association sets are indirectly mapped within this function mapping
            // through association navigation bindings
            if (null != modificationFunctionMapping.DeleteFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(
                    modificationFunctionMapping.DeleteFunctionMapping.CollocatedAssociationSetEnds);
            }
            if (null != modificationFunctionMapping.InsertFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(
                    modificationFunctionMapping.InsertFunctionMapping.CollocatedAssociationSetEnds);
            }
            if (null != modificationFunctionMapping.UpdateFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(
                    modificationFunctionMapping.UpdateFunctionMapping.CollocatedAssociationSetEnds);
            }
        }

        [Conditional("DEBUG")]
        internal void AssertModificationFunctionMappingInvariants(StorageEntityTypeModificationFunctionMapping modificationFunctionMapping)
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            Debug.Assert(
                modificationFunctionMapping.EntityType.Equals(Set.ElementType) ||
                Helper.IsSubtypeOf(modificationFunctionMapping.EntityType, Set.ElementType),
                "attempting to add a modification function mapping with the wrong entity type");
            foreach (var existingMapping in m_modificationFunctionMappings)
            {
                Debug.Assert(
                    !existingMapping.EntityType.Equals(modificationFunctionMapping.EntityType),
                    "modification function mapping already exists for this type");
            }
        }
    }
}
