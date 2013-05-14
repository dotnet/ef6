// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     A subclass of the TypeInfo class above that only represents information
    ///     about "root" types
    /// </summary>
    internal class RootTypeInfo : TypeInfo
    {
        #region private state

        private readonly List<PropertyRef> m_propertyRefList;
        private readonly Dictionary<PropertyRef, EdmProperty> m_propertyMap;
        private EdmProperty m_nullSentinelProperty;
        private EdmProperty m_typeIdProperty;
        private readonly ExplicitDiscriminatorMap m_discriminatorMap;
        private EdmProperty m_entitySetIdProperty;
        private RowType m_flattenedType;
        private TypeUsage m_flattenedTypeUsage;

        #endregion

        #region Constructor

        /// <summary>
        ///     Constructor for a root type
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal RootTypeInfo(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap)
            : base(type, null)
        {
            PlanCompiler.Assert(type.EdmType.BaseType == null, "only root types allowed here");

            m_propertyMap = new Dictionary<PropertyRef, EdmProperty>();
            m_propertyRefList = new List<PropertyRef>();
            m_discriminatorMap = discriminatorMap;
            TypeIdKind = TypeIdKind.Generated;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        ///     Kind of the typeid column (if any)
        /// </summary>
        internal TypeIdKind TypeIdKind { get; set; }

        /// <summary>
        ///     Datatype of the typeid column (if any)
        /// </summary>
        internal TypeUsage TypeIdType { get; set; }

        /// <summary>
        ///     Add a mapping from the propertyRef (of the old type) to the
        ///     corresponding property in the new type.
        ///     NOTE: Only to be used by StructuredTypeInfo
        /// </summary>
        internal void AddPropertyMapping(PropertyRef propertyRef, EdmProperty newProperty)
        {
            m_propertyMap[propertyRef] = newProperty;
            if (propertyRef is TypeIdPropertyRef)
            {
                m_typeIdProperty = newProperty;
            }
            else if (propertyRef is EntitySetIdPropertyRef)
            {
                m_entitySetIdProperty = newProperty;
            }
            else if (propertyRef is NullSentinelPropertyRef)
            {
                m_nullSentinelProperty = newProperty;
            }
        }

        /// <summary>
        ///     Adds a new property reference to the list of desired properties
        ///     NOTE: Only to be used by StructuredTypeInfo
        /// </summary>
        internal void AddPropertyRef(PropertyRef propertyRef)
        {
            m_propertyRefList.Add(propertyRef);
        }

        /// <summary>
        ///     Flattened record version of the type
        /// </summary>
        internal new RowType FlattenedType
        {
            get { return m_flattenedType; }
            set
            {
                m_flattenedType = value;
                m_flattenedTypeUsage = TypeUsage.Create(value);
            }
        }

        /// <summary>
        ///     TypeUsage that encloses the Flattened record version of the type
        /// </summary>
        internal new TypeUsage FlattenedTypeUsage
        {
            get { return m_flattenedTypeUsage; }
        }

        /// <summary>
        ///     Gets map information for types mapped using simple discriminator pattern.
        /// </summary>
        internal ExplicitDiscriminatorMap DiscriminatorMap
        {
            get { return m_discriminatorMap; }
        }

        /// <summary>
        ///     Get the property describing the entityset (if any)
        /// </summary>
        internal new EdmProperty EntitySetIdProperty
        {
            get { return m_entitySetIdProperty; }
        }

        internal new EdmProperty NullSentinelProperty
        {
            get { return m_nullSentinelProperty; }
        }

        /// <summary>
        ///     Get the list of property refs for this type
        /// </summary>
        internal new IEnumerable<PropertyRef> PropertyRefList
        {
            get { return m_propertyRefList; }
        }

        /// <summary>
        ///     Determines the offset for structured types in Flattened type. For instance, if the original type is of the form:
        ///     { int X, ComplexType Y }
        ///     and the flattened type is of the form:
        ///     { int X, Y_ComplexType_Prop1, Y_ComplexType_Prop2 }
        ///     GetNestedStructureOffset(Y) returns 1
        /// </summary>
        /// <param name="property"> Complex property. </param>
        /// <returns> Offset. </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TypeInfo")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal int GetNestedStructureOffset(PropertyRef property)
        {
            // m_propertyRefList contains every element of the flattened type
            for (var i = 0; i < m_propertyRefList.Count; i++)
            {
                var nestedPropertyRef = m_propertyRefList[i] as NestedPropertyRef;

                // match offset of the first element of the complex type property
                if (null != nestedPropertyRef
                    && nestedPropertyRef.InnerProperty.Equals(property))
                {
                    return i;
                }
            }
            PlanCompiler.Assert(false, "no complex structure " + property + " found in TypeInfo");
            // return something so that the compiler doesn't complain
            return default(int);
        }

        /// <summary>
        ///     Try get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef"> property reference (on the old type) </param>
        /// <param name="throwIfMissing"> throw if the property is not found </param>
        /// <param name="property"> the corresponding property on the new type </param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal new bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out EdmProperty property)
        {
            var result = m_propertyMap.TryGetValue(propertyRef, out property);
            if (throwIfMissing && !result)
            {
                {
                    PlanCompiler.Assert(false, "Unable to find property " + propertyRef + " in type " + Type.EdmType.Identity);
                }
            }
            return result;
        }

        /// <summary>
        ///     The typeid property in the flattened type - applies only to nominal types
        ///     this will be used as the type discriminator column.
        /// </summary>
        internal new EdmProperty TypeIdProperty
        {
            get { return m_typeIdProperty; }
        }

        #endregion
    }
}
