// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Represents the Mapping metadata for an AssociationSet in CS space.
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
    ///     This class represents the metadata for the AssociationSetMapping elements in the
    ///     above example. And it is possible to access the AssociationTypeMap underneath it.
    ///     There will be only one TypeMap under AssociationSetMap.
    /// </example>
    public class StorageAssociationSetMapping : StorageSetMapping
    {
        private readonly List<DataModelAnnotation> _annotationsList = new List<DataModelAnnotation>();

        /// <summary>
        ///     Construct a new AssociationSetMapping object
        /// </summary>
        /// <param name="extent"> Represents the Association Set Metadata object. Will change this to Extent instead of MemberMetadata. </param>
        /// <param name="entityContainerMapping"> The entityContainerMapping mapping that contains this Set mapping </param>
        internal StorageAssociationSetMapping(AssociationSet extent, StorageEntityContainerMapping entityContainerMapping)
            : base(extent, entityContainerMapping)
        {
        }

        internal StorageAssociationSetMapping(AssociationSet extent, EntitySet entitySet)
            : base(extent, null)
        {
            DebugCheck.NotNull(entitySet);

            var associationTypeMapping
                = new StorageAssociationTypeMapping(extent.ElementType, this);

            var mappingFragment
                = new StorageMappingFragment(entitySet, associationTypeMapping, false);

            associationTypeMapping.AddFragment(mappingFragment);

            AddTypeMapping(associationTypeMapping);
        }

        public virtual AssociationSet AssociationSet
        {
            get { return (AssociationSet)Set; }
        }

        /// <summary>
        ///     Gets or sets function mapping information for this association set. May be null.
        /// </summary>
        internal StorageAssociationSetModificationFunctionMapping ModificationFunctionMapping { get; set; }

        public EntitySet StoreEntitySet
        {
            get { return (SingleFragment != null) ? SingleFragment.TableSet : null; }
            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(SingleFragment != null);

                SingleFragment.TableSet = value;
            }
        }

        public EntityType Table
        {
            get { return (StoreEntitySet != null ? StoreEntitySet.ElementType : null); }
        }

        public StorageEndPropertyMapping SourceEndMapping
        {
            get
            {
                return (SingleFragment != null)
                           ? SingleFragment.Properties.OfType<StorageEndPropertyMapping>().FirstOrDefault()
                           : null;
            }
            internal set
            {
                DebugCheck.NotNull(value);
                DebugCheck.NotNull(SingleFragment);
                Debug.Assert(SingleFragment.Properties.Count == 0);

                SingleFragment.AddProperty(value);
            }
        }

        public StorageEndPropertyMapping TargetEndMapping
        {
            get
            {
                return (SingleFragment != null)
                           ? SingleFragment.Properties.OfType<StorageEndPropertyMapping>().ElementAtOrDefault(1)
                           : null;
            }
            internal set
            {
                DebugCheck.NotNull(value);
                DebugCheck.NotNull(SingleFragment);
                Debug.Assert(SingleFragment.Properties.Count == 1);

                SingleFragment.AddProperty(value);
            }
        }

        internal virtual IList<DataModelAnnotation> Annotations
        {
            get { return _annotationsList; }
        }

        public virtual IEnumerable<StorageConditionPropertyMapping> ColumnConditions
        {
            get
            {
                return (SingleFragment != null)
                           ? SingleFragment.ColumnConditions
                           : Enumerable.Empty<StorageConditionPropertyMapping>();
            }
        }

        internal void AddColumnCondition(StorageConditionPropertyMapping storageConditionPropertyMapping)
        {
            if (SingleFragment != null)
            {
                SingleFragment.AddConditionProperty(storageConditionPropertyMapping);
            }
        }

        private StorageMappingFragment SingleFragment
        {
            get
            {
                var typeMapping = TypeMappings.SingleOrDefault();

                return (typeMapping != null)
                           ? typeMapping.MappingFragments.SingleOrDefault()
                           : null;
            }
        }
    }
}
