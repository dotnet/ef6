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
    /// Represents the Mapping metadata for an AssociationSet in CS space.
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
    /// This class represents the metadata for the AssociationSetMapping elements in the
    /// above example. And it is possible to access the AssociationTypeMap underneath it.
    /// There will be only one TypeMap under AssociationSetMap.
    /// </example>
    public class AssociationSetMapping : EntitySetBaseMapping
    {
        private readonly AssociationSet _associationSet;
        private AssociationTypeMapping _associationTypeMapping;
        private AssociationSetModificationFunctionMapping _modificationFunctionMapping;

        /// <summary>
        /// Initializes a new AssociationSetMapping instance.
        /// </summary>
        /// <param name="associationSet">The association set to be mapped.</param>
        /// <param name="storeEntitySet">The store entity set to be mapped.</param>
        /// <param name="containerMapping">The parent container mapping.</param>
        public AssociationSetMapping(AssociationSet associationSet, EntitySet storeEntitySet, EntityContainerMapping containerMapping)
            : base(containerMapping)
        {
            Check.NotNull(associationSet, "associationSet");
            Check.NotNull(storeEntitySet, "storeEntitySet");

            _associationSet = associationSet;
            _associationTypeMapping = new AssociationTypeMapping(associationSet.ElementType, this);
            _associationTypeMapping.MappingFragment
                = new MappingFragment(storeEntitySet, _associationTypeMapping, false);
        }

        // Used for testing only.
        internal AssociationSetMapping(AssociationSet associationSet, EntitySet storeEntitySet)
            : this(associationSet, storeEntitySet, null)
        {
        }

        internal AssociationSetMapping(AssociationSet associationSet, EntityContainerMapping containerMapping)
            : base(containerMapping)
        {
            _associationSet = associationSet;
        }

        /// <summary>
        /// Gets the association set that is mapped.
        /// </summary>
        public AssociationSet AssociationSet
        {
            get { return _associationSet; }
        }

        internal override EntitySetBase Set
        {
            get { return AssociationSet; }
        }

        /// <summary>
        /// Gets the contained association type mapping.
        /// </summary>
        public AssociationTypeMapping AssociationTypeMapping 
        {
            get { return _associationTypeMapping; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(_associationTypeMapping == null);
                Debug.Assert(!IsReadOnly);

                _associationTypeMapping = value;
            }
        }

        internal override IEnumerable<TypeMapping> TypeMappings
        {
            get { yield return _associationTypeMapping; }
        }

        /// <summary>
        /// Gets or sets the corresponding function mapping. Can be null.
        /// </summary>
        public AssociationSetModificationFunctionMapping ModificationFunctionMapping
        {
            get { return _modificationFunctionMapping;  }

            set
            {
                ThrowIfReadOnly();

                _modificationFunctionMapping = value; 
                
            }
        }

        /// <summary>
        /// Gets the store entity set that is mapped.
        /// </summary>
        public EntitySet StoreEntitySet
        {
            get { return (SingleFragment != null) ? SingleFragment.StoreEntitySet : null; }

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(SingleFragment != null);
                Debug.Assert(!IsReadOnly);

                SingleFragment.StoreEntitySet = value;
            }
        }

        internal EntityType Table
        {
            get { return (StoreEntitySet != null ? StoreEntitySet.ElementType : null); }
        }

        /// <summary>
        /// Gets or sets the source end property mapping.
        /// </summary>
        public EndPropertyMapping SourceEndMapping
        {
            get
            {
                return 
                    (SingleFragment != null)
                        ? SingleFragment.Properties.OfType<EndPropertyMapping>().FirstOrDefault()
                        : null;
            }

            set
            {
                Check.NotNull(value, "value");
                ThrowIfReadOnly();

                DebugCheck.NotNull(SingleFragment);
                Debug.Assert(SingleFragment.Properties.Count == 0);

                SingleFragment.AddProperty(value);
            }
        }

        /// <summary>
        /// Gets or sets the target end property mapping.
        /// </summary>
        public EndPropertyMapping TargetEndMapping
        {
            get
            {
                return (SingleFragment != null)
                           ? SingleFragment.Properties.OfType<EndPropertyMapping>().ElementAtOrDefault(1)
                           : null;
            }

            set
            {
                Check.NotNull(value, "value");
                ThrowIfReadOnly();

                DebugCheck.NotNull(SingleFragment);
                Debug.Assert(SingleFragment.Properties.Count == 1);

                SingleFragment.AddProperty(value);
            }
        }

        /// <summary>
        /// Gets the property mapping conditions.
        /// </summary>
        public ReadOnlyCollection<ConditionPropertyMapping> Conditions
        {
            get
            {
                return 
                    (SingleFragment != null)
                        ? SingleFragment.Conditions
                        : new ReadOnlyCollection<ConditionPropertyMapping>(
                            new List<ConditionPropertyMapping>());
            }
        }

        private MappingFragment SingleFragment
        {
            get
            {
                return (_associationTypeMapping != null)
                           ? _associationTypeMapping.MappingFragment
                           : null;
            }
        }

        /// <summary>
        /// Adds a property mapping condition.
        /// </summary>
        /// <param name="condition">The condition to add.</param>
        public void AddCondition(ConditionPropertyMapping condition)
        {
            Check.NotNull(condition, "condition");
            ThrowIfReadOnly();

            if (SingleFragment != null)
            {
                SingleFragment.AddCondition(condition);
            }
        }

        /// <summary>
        /// Removes a property mapping condition.
        /// </summary>
        /// <param name="condition">The property mapping condition to remove.</param>
        public void RemoveCondition(ConditionPropertyMapping condition)
        {
            Check.NotNull(condition, "condition");
            ThrowIfReadOnly();

            if (SingleFragment != null)
            {
                SingleFragment.RemoveCondition(condition);
            }
        }
    }
}
