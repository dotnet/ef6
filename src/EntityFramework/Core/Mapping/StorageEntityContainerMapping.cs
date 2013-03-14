// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using CellGroup = System.Data.Entity.Core.Common.Utils.Set<ViewGeneration.Structures.Cell>;

    /// <summary>
    ///     Represents the Mapping metadata for the EntityContainer map in CS space.
    ///     Only one EntityContainerMapping element is allowed in the MSL file for CS mapping.
    ///     <example>
    ///         For Example if conceptually you could represent the CS MSL file as following
    ///         ---Mapping
    ///         --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///         --EntitySetMapping
    ///         --AssociationSetMapping
    ///         The type represents the metadata for EntityContainerMapping element in the above example.
    ///         The SetMapping elements that are children of the EntityContainerMapping element
    ///         can be accessed through the properties on this type.
    ///     </example>
    ///     <remarks>
    ///         We currently assume that an Entity Container on the C side
    ///         is mapped to a single Entity Container in the S - space.
    ///     </remarks>
    /// </summary>
    public class StorageEntityContainerMapping : Map
    {
        /// <summary>
        ///     Construct a new EntityContainer mapping object
        ///     passing in the C-space EntityContainer  and
        ///     the s-space Entity container metadata objects.
        /// </summary>
        /// <param name="entityContainer"> Entity Continer type that is being mapped on the C-side </param>
        /// <param name="storageEntityContainer"> Entity Continer type that is being mapped on the S-side </param>
        public StorageEntityContainerMapping(
            EntityContainer entityContainer, EntityContainer storageEntityContainer,
            StorageMappingItemCollection storageMappingItemCollection, bool validate, bool generateUpdateViews)
        {
            Check.NotNull(entityContainer, "entityContainer");

            m_entityContainer = entityContainer;
            m_storageEntityContainer = storageEntityContainer;
            m_storageMappingItemCollection = storageMappingItemCollection;
            m_memoizedCellGroupEvaluator = new Memoizer<InputForComputingCellGroups, OutputFromComputeCellGroups>(
                ComputeCellGroups, new InputForComputingCellGroups());
            identity = entityContainer.Identity;
            m_validate = validate;
            m_generateUpdateViews = generateUpdateViews;
        }

        internal StorageEntityContainerMapping(EntityContainer entityContainer)
            : this(entityContainer, null, null, false, false)
        {
        }

        private readonly string identity;
        private readonly bool m_validate;
        private readonly bool m_generateUpdateViews;
        private readonly EntityContainer m_entityContainer; //Entity Continer type that is being mapped on the C-side
        private readonly EntityContainer m_storageEntityContainer; //Entity Continer type that the C-space container is being mapped to

        private readonly Dictionary<string, StorageSetMapping> m_entitySetMappings =
            new Dictionary<string, StorageSetMapping>(StringComparer.Ordinal);

        //A collection of EntitySetMappings under this EntityContainer mapping

        private readonly Dictionary<string, StorageSetMapping> m_associationSetMappings =
            new Dictionary<string, StorageSetMapping>(StringComparer.Ordinal);

        //A collection of AssociationSetMappings under this EntityContainer mapping        

        private readonly Dictionary<EdmFunction, FunctionImportMapping> m_functionImportMappings =
            new Dictionary<EdmFunction, FunctionImportMapping>();

        private readonly StorageMappingItemCollection m_storageMappingItemCollection;
        private readonly Memoizer<InputForComputingCellGroups, OutputFromComputeCellGroups> m_memoizedCellGroupEvaluator;

        public StorageMappingItemCollection StorageMappingItemCollection
        {
            get { return m_storageMappingItemCollection; }
        }

        /// <summary>
        ///     Gets the type kind for this item
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.MetadataItem; }
        }

        /// <summary>
        ///     The Entity Container Metadata object on the C-side
        ///     for which the mapping is being represented.
        /// </summary>
        internal override MetadataItem EdmItem
        {
            get { return m_entityContainer; }
        }

        internal override string Identity
        {
            get { return identity; }
        }

        /// <summary>
        ///     Indicates whether there are no Set mappings
        ///     in the container mapping.
        /// </summary>
        internal bool IsEmpty
        {
            get
            {
                return ((m_entitySetMappings.Count == 0)
                        && (m_associationSetMappings.Count == 0));
            }
        }

        /// <summary>
        ///     Determine whether the container includes any views.
        ///     Returns true if there is at least one query or update view specified by the mapping.
        /// </summary>
        internal bool HasViews
        {
            get
            {
                return HasMappingFragments()
                       || AllSetMaps.Any((StorageSetMapping setMap) => setMap.QueryView != null);
            }
        }

        internal string SourceLocation { get; set; }

        /// <summary>
        ///     The Entity Container Metadata object on the C-side
        ///     for which the mapping is being represented.
        /// </summary>
        public EntityContainer EdmEntityContainer
        {
            get { return m_entityContainer; }
        }

        /// <summary>
        ///     The Entity Container Metadata object on the C-side
        ///     for which the mapping is being represented.
        /// </summary>
        public EntityContainer StorageEntityContainer
        {
            get { return m_storageEntityContainer; }
        }

        /// <summary>
        ///     a list of all the  entity set maps under this
        ///     container. In CS mapping, the mapping is done
        ///     at the extent level as opposed to the type level.
        /// </summary>
        public ReadOnlyCollection<StorageSetMapping> EntitySetMaps
        {
            get { return new List<StorageSetMapping>(m_entitySetMappings.Values).AsReadOnly(); }
        }

        public virtual IEnumerable<StorageEntitySetMapping> EntitySetMappings
        {
            get { return EntitySetMaps.OfType<StorageEntitySetMapping>(); }
        }

        public virtual IEnumerable<StorageAssociationSetMapping> AssociationSetMappings
        {
            get { return RelationshipSetMaps.OfType<StorageAssociationSetMapping>(); }
        }

        /// <summary>
        ///     a list of all the  entity set maps under this
        ///     container. In CS mapping, the mapping is done
        ///     at the extent level as opposed to the type level.
        ///     RelationshipSetMaps will be CompositionSetMaps and
        ///     AssociationSetMaps put together.
        /// </summary>
        /// <remarks>
        ///     The reason we have RelationshipSetMaps is to be consistent with CDM metadata
        ///     which treats both associations and compositions as Relationships.
        /// </remarks>
        public ReadOnlyCollection<StorageSetMapping> RelationshipSetMaps
        {
            get { return new List<StorageSetMapping>(m_associationSetMappings.Values).AsReadOnly(); }
        }

        /// <summary>
        ///     a list of all the  set maps under this
        ///     container.
        /// </summary>
        public IEnumerable<StorageSetMapping> AllSetMaps
        {
            get { return m_entitySetMappings.Values.Concat(m_associationSetMappings.Values); }
        }

        /// <summary>
        ///     Line Number in MSL file where the EntityContainer Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLineNumber { get; set; }

        /// <summary>
        ///     Line Position in MSL file where the EntityContainer Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLinePosition { get; set; }

        /// <summary>
        ///     Indicates whether to validate the mapping or not.
        /// </summary>
        internal bool Validate
        {
            get { return m_validate; }
        }

        /// <summary>
        ///     Indicates whether to generate the update views or not.
        /// </summary>
        internal bool GenerateUpdateViews
        {
            get { return m_generateUpdateViews; }
        }

        /// <summary>
        ///     get an EntitySet mapping based upon the name of the entity set.
        /// </summary>
        /// ///
        /// <param name="entitySetName"> the name of the entity set </param>
        internal StorageSetMapping GetEntitySetMapping(String entitySetName)
        {
            DebugCheck.NotNull(entitySetName);
            //Key for EntitySetMapping should be EntitySet name and Entoty type name
            StorageSetMapping setMapping = null;
            m_entitySetMappings.TryGetValue(entitySetName, out setMapping);
            return setMapping;
        }

        /// <summary>
        ///     Get a RelationShip set mapping based upon the name of the relationship set
        /// </summary>
        /// <param name="relationshipSetName"> the name of the relationship set </param>
        /// <returns> the mapping for the entity set if it exists, null if it does not exist </returns>
        internal StorageSetMapping GetRelationshipSetMapping(string relationshipSetName)
        {
            DebugCheck.NotNull(relationshipSetName);
            StorageSetMapping setMapping = null;
            m_associationSetMappings.TryGetValue(relationshipSetName, out setMapping);
            return setMapping;
        }

        /// <summary>
        ///     Get a RelationShipSet mapping that has the passed in EntitySet as one of the ends and is mapped to the
        ///     table.
        /// </summary>
        internal IEnumerable<StorageAssociationSetMapping> GetRelationshipSetMappingsFor(
            EntitySetBase edmEntitySet, EntitySetBase storeEntitySet)
        {
            //First select the association set maps that are mapped to this table
            var associationSetMappings =
                m_associationSetMappings.Values.Cast<StorageAssociationSetMapping>().Where(
                    w => ((w.StoreEntitySet != null) && (w.StoreEntitySet == storeEntitySet)));
            //From this again filter the ones that have the specified EntitySet on atleast one end
            associationSetMappings =
                associationSetMappings.Where(
                    associationSetMap =>
                    ((associationSetMap.Set as AssociationSet).AssociationSetEnds.Any(
                        associationSetEnd => associationSetEnd.EntitySet == edmEntitySet)));
            return associationSetMappings;
        }

        /// <summary>
        ///     Get a set mapping based upon the name of the set
        /// </summary>
        /// <param name="setName"> </param>
        /// <returns> </returns>
        internal StorageSetMapping GetSetMapping(string setName)
        {
            var setMap = GetEntitySetMapping(setName);
            if (setMap == null)
            {
                setMap = GetRelationshipSetMapping(setName);
            }
            return setMap;
        }

        /// <summary>
        ///     Adds an entity set mapping to the list of EntitySetMaps
        ///     under this entity container mapping. The method will be called
        ///     by the Mapping loader.
        /// </summary>
        public void AddEntitySetMapping(StorageSetMapping setMapping)
        {
            if (!m_entitySetMappings.ContainsKey(setMapping.Set.Name))
            {
                m_entitySetMappings.Add(setMapping.Set.Name, setMapping);
            }
        }

        /// <summary>
        ///     Adds a association set mapping to the list of AssociationSetMaps
        ///     under this entity container mapping. The method will be called
        ///     by the Mapping loader.
        /// </summary>
        public void AddAssociationSetMapping(StorageSetMapping setMapping)
        {
            m_associationSetMappings.Add(setMapping.Set.Name, setMapping);
        }

        /// <summary>
        ///     check whether the EntityContainerMapping contains
        ///     the map for the given AssociationSet
        /// </summary>
        /// <param name="associationSet"> </param>
        /// <returns> </returns>
        internal bool ContainsAssociationSetMapping(AssociationSet associationSet)
        {
            return m_associationSetMappings.ContainsKey(associationSet.Name);
        }

        /// <summary>
        ///     Returns whether the Set Map for the given set has a query view or not
        /// </summary>
        /// <param name="setName"> </param>
        /// <returns> </returns>
        internal bool HasQueryViewForSetMap(string setName)
        {
            var set = GetSetMapping(setName);
            if (set != null)
            {
                return (set.QueryView != null);
            }
            return false;
        }

        internal bool HasMappingFragments()
        {
            foreach (var extentMap in AllSetMaps)
            {
                foreach (var typeMap in extentMap.TypeMappings)
                {
                    if (typeMap.MappingFragments.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Methods to modify and access function imports, which association a "functionImport" declared
        // in the model entity container with a targetFunction declared in the target
        internal void AddFunctionImportMapping(EdmFunction functionImport, FunctionImportMapping mapping)
        {
            m_functionImportMappings.Add(functionImport, mapping);
        }

        internal virtual bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping mapping)
        {
            return m_functionImportMappings.TryGetValue(functionImport, out mapping);
        }

        internal OutputFromComputeCellGroups GetCellgroups(InputForComputingCellGroups args)
        {
            Debug.Assert(ReferenceEquals(this, args.ContainerMapping));
            return m_memoizedCellGroupEvaluator.Evaluate(args);
        }

        private OutputFromComputeCellGroups ComputeCellGroups(InputForComputingCellGroups args)
        {
            var result = new OutputFromComputeCellGroups();
            result.Success = true;

            var cellCreator = new CellCreator(args.ContainerMapping);
            result.Cells = cellCreator.GenerateCells();
            result.Identifiers = cellCreator.Identifiers;

            if (result.Cells.Count <= 0)
            {
                //When type-specific QVs are asked for but not defined in the MSL we should return without generating
                // Query pipeline will handle this appropriately by asking for UNION ALL view.
                result.Success = false;
                return result;
            }

            result.ForeignKeyConstraints = ForeignConstraint.GetForeignConstraints(args.ContainerMapping.StorageEntityContainer);

            // Go through each table and determine their foreign key constraints
            var partitioner = new CellPartitioner(result.Cells, result.ForeignKeyConstraints);
            var cellGroups = partitioner.GroupRelatedCells();

            //Clone cell groups- i.e, List<Set<Cell>> - upto cell before storing it in the cache because viewgen modified the Cell structure
            result.CellGroups = cellGroups.Select(setOfcells => new CellGroup(setOfcells.Select(cell => new Cell(cell)))).ToList();

            return result;
        }
    }

    internal struct InputForComputingCellGroups : IEquatable<InputForComputingCellGroups>, IEqualityComparer<InputForComputingCellGroups>
    {
        internal readonly StorageEntityContainerMapping ContainerMapping;
        internal readonly ConfigViewGenerator Config;

        internal InputForComputingCellGroups(StorageEntityContainerMapping containerMapping, ConfigViewGenerator config)
        {
            ContainerMapping = containerMapping;
            Config = config;
        }

        public bool Equals(InputForComputingCellGroups other)
        {
            // Isn't this funny? We are not using Memoizer for function memoization. Args Entity and Config don't matter!
            // If I were to compare Entity this would not use the cache for cases when I supply different entity set. However,
            // the cell groups belong to ALL entity sets.
            return (ContainerMapping.Equals(other.ContainerMapping)
                    && Config.Equals(other.Config));
        }

        public bool Equals(InputForComputingCellGroups one, InputForComputingCellGroups two)
        {
            if (ReferenceEquals(one, two))
            {
                return true;
            }
            if (ReferenceEquals(one, null)
                || ReferenceEquals(two, null))
            {
                return false;
            }

            return one.Equals(two);
        }

        public int GetHashCode(InputForComputingCellGroups value)
        {
            if (value == null)
            {
                return 0;
            }

            return value.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ContainerMapping.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is InputForComputingCellGroups)
            {
                return Equals((InputForComputingCellGroups)obj);
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(InputForComputingCellGroups input1, InputForComputingCellGroups input2)
        {
            if (ReferenceEquals(input1, input2))
            {
                return true;
            }
            return input1.Equals(input2);
        }

        public static bool operator !=(InputForComputingCellGroups input1, InputForComputingCellGroups input2)
        {
            return !(input1 == input2);
        }
    }

    internal struct OutputFromComputeCellGroups
    {
        internal List<Cell> Cells;
        internal CqlIdentifiers Identifiers;
        internal List<CellGroup> CellGroups;
        internal List<ForeignConstraint> ForeignKeyConstraints;
        internal bool Success;
    }
}
