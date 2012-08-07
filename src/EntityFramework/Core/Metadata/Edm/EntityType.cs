// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     concrete Representation the Entity Type
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EntityType : EntityTypeBase
    {
        #region Constructors

        internal EntityType()
        {
        }

        /// <summary>
        ///     Initializes a new instance of Entity Type
        /// </summary>
        /// <param name="name"> name of the entity type </param>
        /// <param name="namespaceName"> namespace of the entity type </param>
        /// <param name="version"> version of the entity type </param>
        /// <param name="dataSpace"> dataspace in which the EntityType belongs to </param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }

        /// <param name="name"> name of the entity type </param>
        /// <param name="namespaceName"> namespace of the entity type </param>
        /// <param name="version"> version of the entity type </param>
        /// <param name="dataSpace"> dataspace in which the EntityType belongs to </param>
        /// <param name="members"> members of the entity type [property and navigational property] </param>
        /// <param name="keyMemberNames"> key members for the type </param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
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

        #endregion

        #region Fields

        /// <summary>
        ///     cached dynamic method to construct a CLR instance
        /// </summary>
        private RefType _referenceType;

        private ReadOnlyMetadataCollection<EdmProperty> _properties;
        private RowType _keyRow;

        #endregion

        #region Methods

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntityType; }
        }

        /// <summary>
        ///     Validates a EdmMember object to determine if it can be added to this type's 
        ///     Members collection. If this method returns without throwing, it is assumed
        ///     the member is valid.
        /// </summary>
        /// <param name="member"> The member to validate </param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                Helper.IsEdmProperty(member) || Helper.IsNavigationProperty(member),
                "Only members of type Property may be added to Entity types.");
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Returns the list of Navigation Properties for this entity type
        /// </summary>
        public ReadOnlyMetadataCollection<NavigationProperty> NavigationProperties
        {
            get
            {
                return new FilteredReadOnlyMetadataCollection<NavigationProperty, EdmMember>(
                    (Members), Helper.IsNavigationProperty);
            }
        }

        /// <summary>
        ///     Returns just the properties from the collection
        ///     of members on this type
        /// </summary>
        public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _properties)
                {
                    Interlocked.CompareExchange(
                        ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            Members, Helper.IsEdmProperty), null);
                }
                return _properties;
            }
        }

        #endregion // Properties

        /// <summary>
        ///     Returns the Reference type pointing to this entity type
        /// </summary>
        /// <returns> </returns>
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
                foreach (var keyMember in KeyMembers)
                {
                    keyProperties.Add(new EdmProperty(keyMember.Name, Helper.GetModelTypeUsage(keyMember)));
                }
                Interlocked.CompareExchange(ref _keyRow, new RowType(keyProperties), null);
            }
            return _keyRow;
        }

        /// <summary>
        ///     Attempts to get the property name for the assoication between the two given end
        ///     names.  Note that this property may not exist if a navigation property is defined
        ///     in one direction but not in the other.
        /// </summary>
        /// <param name="relationshipType"> the relationship for which a nav property is required </param>
        /// <param name="fromName"> the 'from' end of the association </param>
        /// <param name="toName"> the 'to' end of the association </param>
        /// <param name="navigationProperty"> the property name, or null if none was found </param>
        /// <returns> true if a property was found, false otherwise </returns>
        internal bool TryGetNavigationProperty(
            string relationshipType, string fromName, string toName, out NavigationProperty navigationProperty)
        {
            // This is a linear search but it's probably okay because the number of entries
            // is generally small and this method is only called to generate code during lighweight
            // code gen.
            foreach (var navProperty in NavigationProperties)
            {
                if (navProperty.RelationshipType.FullName == relationshipType &&
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
    }
}
