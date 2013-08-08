// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents the metadata for mapping fragment.
    /// A set of mapping fragments makes up the Set mappings( EntitySet, AssociationSet or CompositionSet )
    /// Each MappingFragment provides mapping for those properties of a type that map to a single table.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping
    /// --EntityContainerMapping ( CNorthwind-->SNorthwind )
    /// --EntitySetMapping
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EntityTypeMapping
    /// --MappingFragment
    /// --EntityKey
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ComplexPropertyMap
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    /// --ComplexTypeMapping
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --AssociationSetMapping
    /// --AssociationTypeMapping
    /// --MappingFragment
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    /// --EndPropertyMap
    /// --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the mapping fragment elements in the
    /// above example. Users can access all the top level constructs of
    /// MappingFragment element like EntityKey map, Property Maps, Discriminator
    /// property through this mapping fragment class.
    /// </example>
    internal class StorageMappingFragment : StructuralTypeMapping
    {
        private readonly List<ColumnMappingBuilder> _columnMappings = new List<ColumnMappingBuilder>();
        private readonly List<MetadataProperty> _annotationsList = new List<MetadataProperty>();

        /// <summary>
        /// Construct a new Mapping Fragment object
        /// </summary>
        public StorageMappingFragment(EntitySet tableExtent, StorageTypeMapping typeMapping, bool isSQueryDistinct)
        {
            Check.NotNull(tableExtent, "tableExtent");
            Check.NotNull(typeMapping, "typeMapping");

            m_tableExtent = tableExtent;
            m_typeMapping = typeMapping;
            m_isSQueryDistinct = isSQueryDistinct;
        }

        public IEnumerable<ColumnMappingBuilder> ColumnMappings
        {
            get { return _columnMappings; }
        }

        public void AddColumnMapping(ColumnMappingBuilder columnMappingBuilder)
        {
            Check.NotNull(columnMappingBuilder, "columnMappingBuilder");
            if (!columnMappingBuilder.PropertyPath.Any()
                || _columnMappings.Contains(columnMappingBuilder))
            {
                throw new ArgumentException(Strings.InvalidColumnBuilderArgument("columnBuilderMapping"));
            }

            DebugCheck.NotNull(columnMappingBuilder.ColumnProperty);

            _columnMappings.Add(columnMappingBuilder);

            StructuralTypeMapping structuralTypeMapping = this;
            EdmProperty property;

            // Turn the property path into a mapping fragment nested tree structure.

            var i = 0;
            for (; i < columnMappingBuilder.PropertyPath.Count - 1; i++)
            {
                // The first n-1 properties are complex so we just need to build
                // a corresponding tree of complex type mappings.

                property = columnMappingBuilder.PropertyPath[i];

                var complexPropertyMapping
                    = structuralTypeMapping
                        .Properties
                        .OfType<StorageComplexPropertyMapping>()
                        .SingleOrDefault(pm => ReferenceEquals(pm.EdmProperty, property));

                StorageComplexTypeMapping complexTypeMapping = null;

                if (complexPropertyMapping == null)
                {
                    complexTypeMapping = new StorageComplexTypeMapping(false);
                    complexTypeMapping.AddType(property.ComplexType);

                    complexPropertyMapping = new StorageComplexPropertyMapping(property);
                    complexPropertyMapping.AddTypeMapping(complexTypeMapping);

                    structuralTypeMapping.AddProperty(complexPropertyMapping);
                }

                structuralTypeMapping
                    = complexTypeMapping
                      ?? complexPropertyMapping.TypeMappings.Single();
            }

            // The last property has to be a scalar mapping to the target column.
            // Extract it and create the scalar mapping leaf node, ensuring that we 
            // set the target column.

            property = columnMappingBuilder.PropertyPath[i];

            var scalarPropertyMapping
                = structuralTypeMapping
                    .Properties
                    .OfType<StorageScalarPropertyMapping>()
                    .SingleOrDefault(pm => ReferenceEquals(pm.EdmProperty, property));

            if (scalarPropertyMapping == null)
            {
                scalarPropertyMapping
                    = new StorageScalarPropertyMapping(property, columnMappingBuilder.ColumnProperty);

                structuralTypeMapping.AddProperty(scalarPropertyMapping);

                columnMappingBuilder.SetTarget(scalarPropertyMapping);
            }
            else
            {
                scalarPropertyMapping.ColumnProperty = columnMappingBuilder.ColumnProperty;
            }
        }

        internal void RemoveColumnMapping(ColumnMappingBuilder columnMappingBuilder)
        {
            DebugCheck.NotNull(columnMappingBuilder);
            DebugCheck.NotNull(columnMappingBuilder.ColumnProperty);
            Debug.Assert(columnMappingBuilder.PropertyPath.Any());
            Debug.Assert(_columnMappings.Contains(columnMappingBuilder));

            _columnMappings.Remove(columnMappingBuilder);

            RemoveColumnMapping(this, columnMappingBuilder.PropertyPath);
        }

        private static void RemoveColumnMapping(StructuralTypeMapping structuralTypeMapping, IEnumerable<EdmProperty> propertyPath)
        {
            DebugCheck.NotNull(structuralTypeMapping);
            DebugCheck.NotNull(propertyPath);

            // Remove the target column mapping by walking down the mapping fragment
            // tree corresponding to the passed-in property path until we reach the scalar
            // mapping leaf node. On the way out remove any empty mappings.

            var propertyMapping
                = structuralTypeMapping
                    .Properties
                    .Single(pm => ReferenceEquals(pm.EdmProperty, propertyPath.First()));

            if (propertyMapping is StorageScalarPropertyMapping)
            {
                structuralTypeMapping.RemoveProperty(propertyMapping);
            }
            else
            {
                var complexPropertyMapping = ((StorageComplexPropertyMapping)propertyMapping);
                var complexTypeMapping = complexPropertyMapping.TypeMappings.Single();

                RemoveColumnMapping(complexTypeMapping, propertyPath.Skip(1));

                if (!complexTypeMapping.Properties.Any())
                {
                    structuralTypeMapping.RemoveProperty(complexPropertyMapping);
                }
            }
        }

        internal IList<MetadataProperty> Annotations
        {
            get { return _annotationsList; }
        }

        /// <summary>
        /// Table extent from which the properties are mapped under this fragment.
        /// </summary>
        private EntitySet m_tableExtent;

        /// <summary>
        /// Type mapping under which this mapping fragment exists.
        /// </summary>
        private readonly StorageTypeMapping m_typeMapping;

        /// <summary>
        /// Condition property mappings for this mapping fragment.
        /// </summary>
        private readonly Dictionary<EdmProperty, StorageConditionPropertyMapping> m_conditionProperties =
            new Dictionary<EdmProperty, StorageConditionPropertyMapping>(EqualityComparer<EdmProperty>.Default);

        /// <summary>
        /// All the other properties .
        /// </summary>
        private readonly List<StoragePropertyMapping> m_properties = new List<StoragePropertyMapping>();

        private readonly bool m_isSQueryDistinct;

        /// <summary>
        /// The table from which the properties are mapped in this fragment
        /// </summary>
        public EntitySet TableSet
        {
            get { return m_tableExtent; }
            internal set
            {
                DebugCheck.NotNull(value);

                m_tableExtent = value;
            }
        }

        public EntityType Table
        {
            get { return m_tableExtent.ElementType; }
        }

        internal bool IsSQueryDistinct
        {
            get { return m_isSQueryDistinct; }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        public ReadOnlyCollection<StoragePropertyMapping> AllProperties
        {
            get
            {
                var properties = new List<StoragePropertyMapping>();
                properties.AddRange(m_properties);
                properties.AddRange(m_conditionProperties.Values);
                return properties.AsReadOnly();
            }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        public override ReadOnlyCollection<StoragePropertyMapping> Properties
        {
            get { return m_properties.AsReadOnly(); }
        }

        public IEnumerable<ColumnMappingBuilder> FlattenedProperties
        {
            get { return GetFlattenedProperties(m_properties, new List<EdmProperty>()); }
        }

        private static IEnumerable<ColumnMappingBuilder> GetFlattenedProperties(
            IEnumerable<StoragePropertyMapping> propertyMappings, List<EdmProperty> propertyPath)
        {
            DebugCheck.NotNull(propertyMappings);
            DebugCheck.NotNull(propertyPath);

            foreach (var propertyMapping in propertyMappings)
            {
                propertyPath.Add(propertyMapping.EdmProperty);

                var storageComplexPropertyMapping
                    = propertyMapping as StorageComplexPropertyMapping;

                if (storageComplexPropertyMapping != null)
                {
                    foreach (var columnMappingBuilder
                        in GetFlattenedProperties(
                            storageComplexPropertyMapping.TypeMappings.Single().Properties,
                            propertyPath))
                    {
                        yield return columnMappingBuilder;
                    }
                }
                else
                {
                    var storageScalarPropertyMapping
                        = propertyMapping as StorageScalarPropertyMapping;

                    if (storageScalarPropertyMapping != null)
                    {
                        yield return new ColumnMappingBuilder(
                            storageScalarPropertyMapping.ColumnProperty,
                            propertyPath.ToList());
                    }
                }

                propertyPath.Remove(propertyMapping.EdmProperty);
            }
        }

        public IEnumerable<StorageConditionPropertyMapping> ColumnConditions
        {
            get { return m_conditionProperties.Values; }
        }

        /// <summary>
        /// Line Number in MSL file where the Mapping Fragment Element's Start Tag is present.
        /// </summary>
        internal int StartLineNumber { get; set; }

        /// <summary>
        /// Line Position in MSL file where the Mapping Fragment Element's Start Tag is present.
        /// </summary>
        internal int StartLinePosition { get; set; }

        /// <summary>
        /// File URI of the MSL file
        /// </summary>
        //This should not be stored on the Fragment. Probably it should go on schema.
        //But this requires some thinking before we can finally decide where it should go.
        internal string SourceLocation
        {
            get { return m_typeMapping.SetMapping.EntityContainerMapping.SourceLocation; }
        }

        /// <summary>
        /// Add a property mapping as a child of this mapping fragment
        /// </summary>
        /// <param name="propertyMapping"> child property mapping to be added </param>
        internal override void AddProperty(StoragePropertyMapping propertyMapping)
        {
            m_properties.Add(propertyMapping);
        }

        internal override void RemoveProperty(StoragePropertyMapping prop)
        {
            m_properties.Remove(prop);
        }

        internal void ClearConditions()
        {
            m_conditionProperties.Clear();
        }

        internal void RemoveConditionProperty(StorageConditionPropertyMapping condition)
        {
            DebugCheck.NotNull(condition);

            var conditionMember = condition.EdmProperty ?? condition.ColumnProperty;

            m_conditionProperties.Remove(conditionMember);
        }

        internal void AddConditionProperty(StorageConditionPropertyMapping conditionPropertyMap)
        {
            DebugCheck.NotNull(conditionPropertyMap);

            AddConditionProperty(conditionPropertyMap, _ => { });
        }

        /// <summary>
        /// Add a condition property mapping as a child of this complex property mapping
        /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
        /// </summary>
        /// <param name="conditionPropertyMap"> The mapping that needs to be added </param>
        internal void AddConditionProperty(
            StorageConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
        {
            //Same Member can not have more than one Condition with in the 
            //same Mapping Fragment.
            var conditionMember = conditionPropertyMap.EdmProperty ?? conditionPropertyMap.ColumnProperty;

            Debug.Assert(conditionMember != null);

            if (!m_conditionProperties.ContainsKey(conditionMember))
            {
                m_conditionProperties.Add(conditionMember, conditionPropertyMap);
            }
            else
            {
                duplicateMemberConditionError(conditionMember);
            }
        }
    }
}
