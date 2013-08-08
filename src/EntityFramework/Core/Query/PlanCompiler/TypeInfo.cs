// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using md = System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The TypeInfo class encapsulates various pieces of information about a type.
    /// The most important of these include the "flattened" record type - corresponding
    /// to the type, and the TypeId field for nominal types
    /// </summary>
    internal class TypeInfo
    {
        #region private state

        private readonly md.TypeUsage m_type; // the type
        private readonly List<TypeInfo> m_immediateSubTypes; // the list of children below this type in it's type hierarchy.
        private readonly TypeInfo m_superType; // the type one level up in this types type hierarchy -- the base type.
        private readonly RootTypeInfo m_rootType; // the top-most type in this types type hierarchy

        #endregion

        #region Constructors and factory methods

        /// <summary>
        /// Creates type information for a type
        /// </summary>
        internal static TypeInfo Create(md.TypeUsage type, TypeInfo superTypeInfo, ExplicitDiscriminatorMap discriminatorMap)
        {
            TypeInfo result;
            if (superTypeInfo == null)
            {
                result = new RootTypeInfo(type, discriminatorMap);
            }
            else
            {
                result = new TypeInfo(type, superTypeInfo);
            }
            return result;
        }

        protected TypeInfo(md.TypeUsage type, TypeInfo superType)
        {
            m_type = type;
            m_immediateSubTypes = new List<TypeInfo>();
            m_superType = superType;
            if (superType != null)
            {
                // Add myself to my supertype's list of subtypes
                superType.m_immediateSubTypes.Add(this);
                // my supertype's root type is mine as well
                m_rootType = superType.RootType;
            }
        }

        #endregion

        #region "public" properties for all types

        /// <summary>
        /// Is this the root type?
        /// True for entity, complex types and ref types, if this is the root of the
        /// hierarchy.
        /// Always true for Record types
        /// </summary>
        internal bool IsRootType
        {
            get { return m_rootType == null; }
        }

        /// <summary>
        /// the types that derive from this type
        /// </summary>
        internal List<TypeInfo> ImmediateSubTypes
        {
            get { return m_immediateSubTypes; }
        }

        /// <summary>
        /// the immediate parent type of this type.
        /// </summary>
        internal TypeInfo SuperType
        {
            get { return m_superType; }
        }

        /// <summary>
        /// the top most type in the hierarchy.
        /// </summary>
        internal RootTypeInfo RootType
        {
            get { return m_rootType ?? (RootTypeInfo)this; }
        }

        /// <summary>
        /// The metadata type
        /// </summary>
        internal md.TypeUsage Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// The typeid value for this type - only applies to nominal types
        /// </summary>
        internal object TypeId { get; set; }

        #endregion

        #region "public" properties for root types

        // These properties are actually stored on the RootType but we let
        // let folks use the TypeInfo class as the proxy to get to them.
        // Essentially, they are mostly sugar to simplify coding.
        //
        // For example:
        //
        // You could either write:
        //
        //      typeinfo.RootType.FlattenedType
        //
        // or you can write:
        //
        //      typeinfo.FlattenedType
        //

        /// <summary>
        /// Flattened record version of the type
        /// </summary>
        internal virtual md.RowType FlattenedType
        {
            get { return RootType.FlattenedType; }
        }

        /// <summary>
        /// TypeUsage that encloses the Flattened record version of the type
        /// </summary>
        internal virtual md.TypeUsage FlattenedTypeUsage
        {
            get { return RootType.FlattenedTypeUsage; }
        }

        /// <summary>
        /// Get the property describing the entityset (if any)
        /// </summary>
        internal virtual md.EdmProperty EntitySetIdProperty
        {
            get { return RootType.EntitySetIdProperty; }
        }

        /// <summary>
        /// Does this type have an entitySetId property
        /// </summary>
        internal bool HasEntitySetIdProperty
        {
            get { return RootType.EntitySetIdProperty != null; }
        }

        /// <summary>
        /// Get the nullSentinel property (if any)
        /// </summary>
        internal virtual md.EdmProperty NullSentinelProperty
        {
            get { return RootType.NullSentinelProperty; }
        }

        /// <summary>
        /// Does this type have a nullSentinel property?
        /// </summary>
        internal bool HasNullSentinelProperty
        {
            get { return RootType.NullSentinelProperty != null; }
        }

        /// <summary>
        /// The typeid property in the flattened type - applies only to nominal types
        /// this will be used as the type discriminator column.
        /// </summary>
        internal virtual md.EdmProperty TypeIdProperty
        {
            get { return RootType.TypeIdProperty; }
        }

        /// <summary>
        /// Does this type need a typeid property? (Needed for complex types and entity types in general)
        /// </summary>
        internal bool HasTypeIdProperty
        {
            get { return RootType.TypeIdProperty != null; }
        }

        /// <summary>
        /// All the properties of this type.
        /// </summary>
        internal virtual IEnumerable<PropertyRef> PropertyRefList
        {
            get { return RootType.PropertyRefList; }
        }

        /// <summary>
        /// Get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef"> property reference (on the old type) </param>
        internal md.EdmProperty GetNewProperty(PropertyRef propertyRef)
        {
            md.EdmProperty property;
            var result = TryGetNewProperty(propertyRef, true, out property);
            Debug.Assert(result, "Should have thrown if the property was not found");
            return property;
        }

        /// <summary>
        /// Try get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef"> property reference (on the old type) </param>
        /// <param name="throwIfMissing"> throw if the property is not found </param>
        /// <param name="newProperty"> the corresponding property on the new type </param>
        internal bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out md.EdmProperty newProperty)
        {
            return RootType.TryGetNewProperty(propertyRef, throwIfMissing, out newProperty);
        }

        /// <summary>
        /// Get the list of "key" properties (in the flattened type)
        /// </summary>
        /// <returns> the key property equivalents in the flattened type </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Non-EdmProperty")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal IEnumerable<PropertyRef> GetKeyPropertyRefs()
        {
            md.EntityTypeBase entityType = null;
            md.RefType refType = null;
            if (TypeHelpers.TryGetEdmType(m_type, out refType))
            {
                entityType = refType.ElementType;
            }
            else
            {
                entityType = TypeHelpers.GetEdmType<md.EntityTypeBase>(m_type);
            }

            // Walk through the list of keys of the entity type, and find their analogs in the
            // "flattened" type
            foreach (var p in entityType.KeyMembers)
            {
                // Eventually this could be RelationshipEndMember, but currently only properties are suppported as key members
                PlanCompiler.Assert(p is md.EdmProperty, "Non-EdmProperty key members are not supported");
                var spr = new SimplePropertyRef(p);
                yield return spr;
            }
        }

        /// <summary>
        /// Get the list of "identity" properties in the flattened type.
        /// The identity properties include the entitysetid property, followed by the
        /// key properties
        /// </summary>
        /// <returns> List of identity properties </returns>
        internal IEnumerable<PropertyRef> GetIdentityPropertyRefs()
        {
            if (HasEntitySetIdProperty)
            {
                yield return EntitySetIdPropertyRef.Instance;
            }
            foreach (var p in GetKeyPropertyRefs())
            {
                yield return p;
            }
        }

        /// <summary>
        /// Get the list of all properties in the flattened type
        /// </summary>
        internal IEnumerable<PropertyRef> GetAllPropertyRefs()
        {
            foreach (var p in PropertyRefList)
            {
                yield return p;
            }
        }

        /// <summary>
        /// Get the list of all properties in the flattened type
        /// </summary>
        internal IEnumerable<md.EdmProperty> GetAllProperties()
        {
            foreach (var m in FlattenedType.Properties)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Gets all types in the hierarchy rooted at this.
        /// </summary>
        internal List<TypeInfo> GetTypeHierarchy()
        {
            var result = new List<TypeInfo>();
            GetTypeHierarchy(result);
            return result;
        }

        /// <summary>
        /// Adds all types in the hierarchy to the given list.
        /// </summary>
        private void GetTypeHierarchy(List<TypeInfo> result)
        {
            result.Add(this);
            foreach (var subType in ImmediateSubTypes)
            {
                subType.GetTypeHierarchy(result);
            }
        }

        #endregion
    }
}
