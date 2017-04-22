// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Represents the structure of an <see cref="EntitySet"/>. In the conceptual-model this represents the shape and structure 
    /// of an entity. In the store model this represents the structure of a table. To change the Schema and Table name use EntitySet.  
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EntityType : EntityTypeBase
    {
        private ReadOnlyMetadataCollection<EdmProperty> _properties;

        // <summary>
        // Initializes a new instance of Entity Type
        // </summary>
        // <param name="name"> name of the entity type </param>
        // <param name="namespaceName"> namespace of the entity type </param>
        // <param name="dataSpace"> dataspace in which the EntityType belongs to </param>
        // <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }

        // <param name="name"> name of the entity type </param>
        // <param name="namespaceName"> namespace of the entity type </param>
        // <param name="dataSpace"> dataspace in which the EntityType belongs to </param>
        // <param name="keyMemberNames"> key members for the type </param>
        // <param name="members"> members of the entity type [property and navigational property] </param>
        // <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityType(
            string name,
            string namespaceName,
            DataSpace dataSpace,
            IEnumerable<string> keyMemberNames,
            IEnumerable<EdmMember> members)
            : base(name, namespaceName, dataSpace)
        {
            //--- first add the properties 
            if (null != members)
            {
                CheckAndAddMembers(members, this);
            }
            //--- second add the key members
            if (null != keyMemberNames)
            {
                //Validation should make sure that base type of this type does not have keymembers when this type has keymembers. 
                CheckAndAddKeyMembers(keyMemberNames);
            }
        }

        // <summary>
        // cached dynamic method to construct a CLR instance
        // </summary>
        private RefType _referenceType;

        private RowType _keyRow;

        private readonly List<ForeignKeyBuilder> _foreignKeyBuilders = new List<ForeignKeyBuilder>();

        internal IEnumerable<ForeignKeyBuilder> ForeignKeyBuilders
        {
            get { return _foreignKeyBuilders; }
        }

        internal void RemoveForeignKey(ForeignKeyBuilder foreignKeyBuilder)
        {
            DebugCheck.NotNull(foreignKeyBuilder);
            Util.ThrowIfReadOnly(this);

            foreignKeyBuilder.SetOwner(null);

            _foreignKeyBuilders.Remove(foreignKeyBuilder);
        }

        internal void AddForeignKey(ForeignKeyBuilder foreignKeyBuilder)
        {
            DebugCheck.NotNull(foreignKeyBuilder);
            Util.ThrowIfReadOnly(this);

            foreignKeyBuilder.SetOwner(this);

            _foreignKeyBuilders.Add(foreignKeyBuilder);
        }

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntityType; }
        }

        // <summary>
        // Validates a EdmMember object to determine if it can be added to this type's
        // Members collection. If this method returns without throwing, it is assumed
        // the member is valid.
        // </summary>
        // <param name="member"> The member to validate </param>
        // <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                Helper.IsEdmProperty(member) || Helper.IsNavigationProperty(member),
                "Only members of type Property may be added to Entity types.");
        }

        /// <summary>Gets the declared navigation properties associated with the entity type.</summary>
        /// <returns>The declared navigation properties associated with the entity type.</returns>
        public ReadOnlyMetadataCollection<NavigationProperty> DeclaredNavigationProperties
        {
            get { return GetDeclaredOnlyMembers<NavigationProperty>(); }
        }

        private readonly object _navigationPropertiesCacheLock = new object();
        private ReadOnlyMetadataCollection<NavigationProperty> _navigationPropertiesCache;

        /// <summary>
        /// Gets the navigation properties of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of navigation properties on this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// .
        /// </returns>
        public ReadOnlyMetadataCollection<NavigationProperty> NavigationProperties
        {
            get
            {
                // PERF: this code written this way since it's part of a hotpath, consider its performance when refactoring
                var navigationProperties = _navigationPropertiesCache;
                if (navigationProperties == null)
                {
                    lock (_navigationPropertiesCacheLock)
                    {
                        if (_navigationPropertiesCache == null)
                        {
                            Members.SourceAccessed += ResetNavigationProperties;
                            _navigationPropertiesCache = new FilteredReadOnlyMetadataCollection
                                <NavigationProperty, EdmMember>(
                                Members, Helper.IsNavigationProperty);
                        }
                        navigationProperties = _navigationPropertiesCache;
                    }
                }
                return navigationProperties;
            }
        }

        private void ResetNavigationProperties(object sender, EventArgs e)
        {
            if (_navigationPropertiesCache != null)
            {
                lock (_navigationPropertiesCacheLock)
                {
                    if (_navigationPropertiesCache != null)
                    {
                        _navigationPropertiesCache = null;
                        Members.SourceAccessed -= ResetNavigationProperties;
                    }
                }
            }
        }

        /// <summary>Gets the list of declared properties for the entity type.</summary>
        /// <returns>The declared properties for the entity type.</returns>
        public ReadOnlyMetadataCollection<EdmProperty> DeclaredProperties
        {
            get { return GetDeclaredOnlyMembers<EdmProperty>(); }
        }

        /// <summary>Gets the collection of declared members for the entity type.</summary>
        /// <returns>The collection of declared members for the entity type.</returns>
        public ReadOnlyMetadataCollection<EdmMember> DeclaredMembers
        {
            get { return GetDeclaredOnlyMembers<EdmMember>(); }
        }

        /// <summary>
        /// Gets the list of properties for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of properties for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// .
        /// </returns>
        public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                if (!IsReadOnly)
                {
                    return new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(Members, Helper.IsEdmProperty);
                }

                if (_properties == null)
                {
                    Interlocked.CompareExchange(
                        ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            Members, Helper.IsEdmProperty), null);
                }

                return _properties;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" /> object that references this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RefType" /> object that references this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// .
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public RefType GetReferenceType()
        {
            if (_referenceType == null)
            {
                Interlocked.CompareExchange(ref _referenceType, new RefType(this), null);
            }
            return _referenceType;
        }

        internal RowType GetKeyRowType()
        {
            if (_keyRow == null)
            {
                var keyProperties = new List<EdmProperty>(KeyMembers.Count);
                keyProperties.AddRange(KeyMembers.Select(keyMember => new EdmProperty(keyMember.Name, Helper.GetModelTypeUsage(keyMember))));
                Interlocked.CompareExchange(ref _keyRow, new RowType(keyProperties), null);
            }
            return _keyRow;
        }

        // <summary>
        // Attempts to get the property name for the assoication between the two given end
        // names.  Note that this property may not exist if a navigation property is defined
        // in one direction but not in the other.
        // </summary>
        // <param name="relationshipType"> the relationship for which a nav property is required </param>
        // <param name="fromName"> the 'from' end of the association </param>
        // <param name="toName"> the 'to' end of the association </param>
        // <param name="navigationProperty"> the property name, or null if none was found </param>
        // <returns> true if a property was found, false otherwise </returns>
        internal bool TryGetNavigationProperty(
            string relationshipType, string fromName, string toName, out NavigationProperty navigationProperty)
        {
            // This is a linear search but it's probably okay because the number of entries
            // is generally small and this method is only called to generate code during lighweight
            // code gen.
            foreach (var navProperty in NavigationProperties)
            {
                if (navProperty.RelationshipType.FullName == relationshipType
                    &&
                    navProperty.FromEndMember.Name == fromName
                    &&
                    navProperty.ToEndMember.Name == toName)
                {
                    navigationProperty = navProperty;
                    return true;
                }
            }
            navigationProperty = null;
            return false;
        }

        /// <summary>
        /// The factory method for constructing the EntityType object.
        /// </summary>
        /// <param name="name">The name of the entity type.</param>
        /// <param name="namespaceName">The namespace of the entity type.</param>
        /// <param name="dataSpace">The dataspace in which the EntityType belongs to.</param>
        /// <param name="keyMemberNames">Name of key members for the type.</param>
        /// <param name="members">Members of the entity type (primitive and navigation properties).</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The EntityType object.</returns>
        /// <exception cref="System.ArgumentException">Thrown if either name, namespace arguments are null.</exception>
        /// <remarks>The newly created EntityType will be read only.</remarks>
        public static EntityType Create(
            string name,
            string namespaceName,
            DataSpace dataSpace,
            IEnumerable<string> keyMemberNames,
            IEnumerable<EdmMember> members,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");

            var entity = new EntityType(name, namespaceName, dataSpace, keyMemberNames, members);

            if (metadataProperties != null)
            {
                entity.AddMetadataProperties(metadataProperties);
            }

            entity.SetReadOnly();
            return entity;
        }

        /// <summary>
        /// The factory method for constructing the EntityType object.
        /// </summary>
        /// <param name="name">The name of the entity type.</param>
        /// <param name="namespaceName">The namespace of the entity type.</param>
        /// <param name="dataSpace">The dataspace in which the EntityType belongs to.</param>
        /// <param name="baseType">The base type.</param>
        /// <param name="keyMemberNames">Name of key members for the type.</param>
        /// <param name="members">Members of the entity type (primitive and navigation properties).</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The EntityType object.</returns>
        /// <exception cref="System.ArgumentException">Thrown if either name, namespace arguments are null.</exception>
        /// <remarks>The newly created EntityType will be read only.</remarks>
        public static EntityType Create(
            string name,
            string namespaceName,
            DataSpace dataSpace,
            EntityType baseType,
            IEnumerable<string> keyMemberNames,
            IEnumerable<EdmMember> members,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");
            Check.NotNull(baseType, "baseType");

            var entity = new EntityType(name, namespaceName, dataSpace, keyMemberNames, members) { BaseType = baseType };

            if (metadataProperties != null)
            {
                entity.AddMetadataProperties(metadataProperties);
            }

            entity.SetReadOnly();
            return entity;
        }

        /// <summary>
        /// Adds the specified navigation property to the members of this type.
        /// The navigation property is added regardless of the read-only flag.
        /// </summary>
        /// <param name="property">The navigation property to be added.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void AddNavigationProperty(NavigationProperty property)
        {
            Check.NotNull(property, "property");

            AddMember(property, true);
        }
    }
}
