// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents the Entity Type
    /// </summary>
    public abstract class EntityTypeBase : StructuralType
    {
        private readonly ReadOnlyMetadataCollection<EdmMember> _keyMembers;
        private string[] _keyMemberNames;

        // <summary>
        // Initializes a new instance of Entity Type
        // </summary>
        // <param name="name"> name of the entity type </param>
        // <param name="namespaceName"> namespace of the entity type </param>
        // <param name="dataSpace"> dataSpace in which this edmtype belongs to </param>
        // <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityTypeBase(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _keyMembers = new ReadOnlyMetadataCollection<EdmMember>(new MetadataCollection<EdmMember>());
        }

        /// <summary>Gets the list of all the key members for the current entity or relationship type.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> object that represents the list of key members for the current entity or relationship type.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EdmMember, true)]
        public virtual ReadOnlyMetadataCollection<EdmMember> KeyMembers
        {
            get
            {
                // Since we allow entity types with no keys, we should first check if there are 
                // keys defined on the base class. If yes, then return the keys otherwise, return
                // the keys defined on this class
                if (BaseType != null
                    && ((EntityTypeBase)BaseType).KeyMembers.Count != 0)
                {
                    Debug.Assert(_keyMembers.Count == 0, "Since the base type have keys, current type cannot have keys defined");

                    return ((EntityTypeBase)BaseType).KeyMembers;
                }

                return _keyMembers;
            }
        }

        /// <summary>Gets the list of all the key properties for this entity type.</summary>
        /// <returns>The list of all the key properties for this entity type.</returns>
        public virtual ReadOnlyMetadataCollection<EdmProperty> KeyProperties
        {
            get { return new ReadOnlyMetadataCollection<EdmProperty>(KeyMembers.Cast<EdmProperty>().ToList()); }
        }

        // <summary>
        // Returns the list of the property names that form the key for this entity type
        // Perf Bug #529294: To cache the list of member names that form the key for the entity type
        // </summary>
        internal virtual string[] KeyMemberNames
        {
            get
            {
                var keyNames = _keyMemberNames;

                if (keyNames == null)
                {
                    keyNames = new string[KeyMembers.Count];
                    for (var i = 0; i < keyNames.Length; i++)
                    {
                        keyNames[i] = KeyMembers[i].Name;
                    }
                    _keyMemberNames = keyNames;
                }

                Debug.Assert(
                    _keyMemberNames.Length == KeyMembers.Count,
                    "This list is out of sync with the key members count. This property was called before all the keymembers were added");

                return _keyMemberNames;
            }
        }

        /// <summary>
        /// Adds the specified property to the list of keys for the current entity.  
        /// </summary>
        /// <exception cref="System.ArgumentNullException">if member argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the EntityType has a base type of another EntityTypeBase. In this case KeyMembers should be added to the base type</exception>
        /// <exception cref="System.InvalidOperationException">If the EntityType instance is in ReadOnly state</exception>
        public void AddKeyMember(EdmMember member)
        {
            Check.NotNull(member, "member");
            Util.ThrowIfReadOnly(this);
            Debug.Assert(
                BaseType == null || ((EntityTypeBase)BaseType).KeyMembers.Count == 0,
                "Key cannot be added if there is a basetype with keys");

            if (!Members.Contains(member))
            {
                AddMember(member);
            }

            _keyMembers.Source.Add(member);
        }

        // <summary>
        // Makes this property readonly
        // </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                _keyMembers.Source.SetReadOnly();
                base.SetReadOnly();
            }
        }

        // <summary>
        // Checks for each property to be non-null and then adds it to the member collection
        // </summary>
        // <param name="members"> members for this type </param>
        // <param name="entityType"> the membersCollection to which the members should be added </param>
        internal static void CheckAndAddMembers(
            IEnumerable<EdmMember> members,
            EntityType entityType)
        {
            foreach (var member in members)
            {
                // Check for each property to be non-null
                if (null == member)
                {
                    throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("members"));
                }

                // Add the property to the member collection
                entityType.AddMember(member);
            }
        }

        // <summary>
        // Checks for each key member to be non-null
        // also check for it to be present in the members collection
        // and then adds it to the KeyMembers collection.
        // Throw if the key member is not already in the members
        // collection. Cannot do much other than that as the
        // Key members is just an Ienumerable of the names
        // of the members.
        // </summary>
        // <param name="keyMembers"> the list of keys (member names) to be added for the given type </param>
        internal void CheckAndAddKeyMembers(IEnumerable<String> keyMembers)
        {
            foreach (var keyMember in keyMembers)
            {
                // Check for each keymember to be non-null
                if (null == keyMember)
                {
                    throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("keyMembers"));
                }
                // Check for whether the key exists in the members collection
                EdmMember member;
                if (!Members.TryGetValue(keyMember, false, out member))
                {
                    throw new ArgumentException(Strings.InvalidKeyMember(keyMember));
                    //--- to do, identify the right exception to throw here
                }
                // Add the key member to the key member collection 
                AddKeyMember(member);
            }
        }

        /// <summary>Removes the specified key member from the collection.</summary>
        /// <param name="member">The key member to remove.</param>
        public override void RemoveMember(EdmMember member)
        {
            Check.NotNull(member, "member");
            Util.ThrowIfReadOnly(this);

            if (_keyMembers.Contains(member))
            {
                _keyMembers.Source.Remove(member);
            }

            base.RemoveMember(member);
        }

        internal override void NotifyItemIdentityChanged()
        {
            base.NotifyItemIdentityChanged();

            _keyMembers.Source.InvalidateCache();
        }
    }
}
