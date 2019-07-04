// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// An identifier for an entity.
    /// </summary>
    [DebuggerDisplay("{ConcatKeyValue()}")]
    [Serializable]
    [DataContract(IsReference = true)]
    public sealed class EntityKey : IEquatable<EntityKey>
    {
        // The implementation of EntityKey is optimized for the following common cases:
        //      1) Keys constructed internally rather by the user - in particular, keys 
        //         created by the bridge on the round-trip from query.
        //      2) Single-valued (as opposed to composite) keys.
        // We accomplish this by maintaining two variables, at most one of which is non-null.
        // The first is of type object and in the case of a singleton key, is set to the
        // single key value.  The second is an object array and in the case of 
        // a composite key, is set to the list of key values.  If both variables are null,
        // the EntityKey is a temporary key.  Note that the key field names
        // are not stored - for composite keys, the values are stored in the order in which
        // metadata reports the corresponding key members.

        // The following 5 fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private string _entitySetName;
        private string _entityContainerName;
        private object _singletonKeyValue; // non-null for singleton keys
        private object[] _compositeKeyValues; // non-null for composite keys
        private string[] _keyNames; // key names that correspond to the key values
        private readonly bool _isLocked; // determines if this key is lock from writing

        // Determines whether the key includes a byte[].
        // Not serialized for backwards compatibility.
        // This value is computed along with the _hashCode, which is also not serialized.
        [NonSerialized]
        private bool _containsByteArray;

        [NonSerialized]
        private EntityKeyMember[] _deserializedMembers;

        // The hash code is not serialized since it can be computed differently on the deserialized system.
        [NonSerialized]
        private int _hashCode; // computed as needed

        // <summary>
        // A singleton EntityKey by which a read-only entity is identified.
        // </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        private static readonly EntityKey _noEntitySetKey = new EntityKey("NoEntitySetKey.NoEntitySetKey");

        // <summary>
        // Returns a singleton EntityKey identifying an entity resulted from a failed TREAT.
        // </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        private static readonly EntityKey _entityNotValidKey = new EntityKey("EntityNotValidKey.EntityNotValidKey");

        // <summary>
        // A dictionary of names so that singleton instances of names can be used
        // </summary>
        private static readonly ConcurrentDictionary<string, string> NameLookup = new ConcurrentDictionary<string, string>();

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey" /> class.
        /// </summary>
        public EntityKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey" /> class with an entity set name and a generic
        /// <see
        ///     cref="T:System.Collections.Generic.KeyValuePair" />
        /// collection.
        /// </summary>
        /// <param name="qualifiedEntitySetName">
        /// A <see cref="T:System.String" /> that is the entity set name qualified by the entity container name.
        /// </param>
        /// <param name="entityKeyValues">
        /// A generic <see cref="T:System.Collections.Generic.KeyValuePair" /> collection.Each key/value pair has a property name as the key and the value of that property as the value. There should be one pair for each property that is part of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// . The order of the key/value pairs is not important, but each key property should be included. The property names are simple names that are not qualified with an entity type name or the schema name.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public EntityKey(string qualifiedEntitySetName, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
        {
            Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
            Check.NotNull(entityKeyValues, "entityKeyValues");

            InitializeEntitySetName(qualifiedEntitySetName);
            InitializeKeyValues(entityKeyValues);

            AssertCorrectState(null, false);
            _isLocked = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey" /> class with an entity set name and an
        /// <see
        ///     cref="T:System.Collections.Generic.IEnumerable`1" />
        /// collection of
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKeyMember" />
        /// objects.
        /// </summary>
        /// <param name="qualifiedEntitySetName">
        /// A <see cref="T:System.String" /> that is the entity set name qualified by the entity container name.
        /// </param>
        /// <param name="entityKeyValues">
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKeyMember" />
        /// objects with which to initialize the key.
        /// </param>
        public EntityKey(string qualifiedEntitySetName, IEnumerable<EntityKeyMember> entityKeyValues)
        {
            Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
            Check.NotNull(entityKeyValues, "entityKeyValues");

            InitializeEntitySetName(qualifiedEntitySetName);
            InitializeKeyValues(new KeyValueReader(entityKeyValues));

            AssertCorrectState(null, false);
            _isLocked = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKey" /> class with an entity set name and specific entity key pair.
        /// </summary>
        /// <param name="qualifiedEntitySetName">
        /// A <see cref="T:System.String" /> that is the entity set name qualified by the entity container name.
        /// </param>
        /// <param name="keyName">
        /// A <see cref="T:System.String" /> that is the name of the key.
        /// </param>
        /// <param name="keyValue">
        /// An <see cref="T:System.Object" /> that is the key value.
        /// </param>
        public EntityKey(string qualifiedEntitySetName, string keyName, object keyValue)
        {
            Check.NotEmpty(qualifiedEntitySetName, "qualifiedEntitySetName");
            Check.NotEmpty(keyName, "keyName");
            Check.NotNull(keyValue, "keyValue");

            InitializeEntitySetName(qualifiedEntitySetName);

            ValidateName(keyName);

            _keyNames = new[] { keyName };
            _singletonKeyValue = keyValue;

            AssertCorrectState(null, false);
            _isLocked = true;
        }

        #endregion

        #region Internal Constructors

        // <summary>
        // Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        // </summary>
        // <param name="entitySet"> EntitySet of the entity </param>
        // <param name="record"> an IExtendedDataRecord that represents the entity </param>
        internal EntityKey(EntitySet entitySet, IExtendedDataRecord record)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entitySet.Name);
            DebugCheck.NotNull(entitySet.EntityContainer);
            DebugCheck.NotNull(entitySet.EntityContainer.Name);
            DebugCheck.NotNull(record);

            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;

            InitializeKeyValues(entitySet, record);

            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        // <summary>
        // Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        // </summary>
        // <param name="qualifiedEntitySetName"> EntitySet of the entity </param>
        internal EntityKey(string qualifiedEntitySetName)
        {
            DebugCheck.NotEmpty(qualifiedEntitySetName);

            InitializeEntitySetName(qualifiedEntitySetName);

            _isLocked = true;
        }

        // <summary>
        // Constructs a temporary EntityKey with the given EntitySet.
        // Temporary keys do not store key field names
        // </summary>
        // <param name="entitySet"> EntitySet of the entity </param>
        internal EntityKey(EntitySetBase entitySet)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entitySet.EntityContainer);

            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;

            AssertCorrectState(entitySet, true);
            _isLocked = true;
        }

        // <summary>
        // Constructor optimized for a singleton key.
        // SQLBUDT 478655: Performance optimization: Does no integrity checking on the key value.
        // SQLBUDT 523554: Performance optimization: Does no validate type of key members.
        // </summary>
        // <param name="entitySet"> EntitySet of the entity </param>
        // <param name="singletonKeyValue"> The single value that composes the entity's key, assumed to contain the correct type. </param>
        internal EntityKey(EntitySetBase entitySet, object singletonKeyValue)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entitySet.EntityContainer);
            DebugCheck.NotNull(singletonKeyValue);

            _singletonKeyValue = singletonKeyValue;
            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;
            _keyNames = entitySet.ElementType.KeyMemberNames; // using EntitySetBase avoids an (EntityType) cast that EntitySet incurs

            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        // <summary>
        // Constructor optimized for a composite key.
        // SQLBUDT 478655: Performance optimization: Does no integrity checking on the key values.
        // SQLBUDT 523554: Performance optimization: Does no validate type of key members.
        // </summary>
        // <param name="entitySet"> EntitySet of the entity </param>
        // <param name="compositeKeyValues"> A list of the values (at least 2) that compose the entity's key, assumed to contain correct types. </param>
        internal EntityKey(EntitySetBase entitySet, object[] compositeKeyValues)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entitySet.EntityContainer);
            DebugCheck.NotNull(compositeKeyValues);

            _compositeKeyValues = compositeKeyValues;
            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;
            _keyNames = entitySet.ElementType.KeyMemberNames; // using EntitySetBase avoids an (EntityType) cast that EntitySet incurs

            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        #endregion

        /// <summary>
        /// Gets a singleton EntityKey by which a read-only entity is identified.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static EntityKey NoEntitySetKey
        {
            get { return _noEntitySetKey; }
        }

        /// <summary>
        /// Gets a singleton EntityKey identifying an entity resulted from a failed TREAT.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static EntityKey EntityNotValidKey
        {
            get { return _entityNotValidKey; }
        }

        /// <summary>Gets or sets the name of the entity set.</summary>
        /// <returns>
        /// A <see cref="T:System.String" /> value that is the name of the entity set for the entity to which the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// belongs.
        /// </returns>
        [DataMember]
        public string EntitySetName
        {
            get { return _entitySetName; }
            set
            {
                ValidateWritable(_entitySetName);
                _entitySetName = LookupSingletonName(value);
            }
        }

        /// <summary>Gets or sets the name of the entity container.</summary>
        /// <returns>
        /// A <see cref="T:System.String" /> value that is the name of the entity container for the entity to which the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// belongs.
        /// </returns>
        [DataMember]
        public string EntityContainerName
        {
            get { return _entityContainerName; }
            set
            {
                ValidateWritable(_entityContainerName);
                _entityContainerName = LookupSingletonName(value);
            }
        }

        /// <summary>
        /// Gets or sets the key values associated with this <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of key values for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// .
        /// </returns>
        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required for this feature")]
        public EntityKeyMember[] EntityKeyValues
        {
            get
            {
                if (!IsTemporary)
                {
                    EntityKeyMember[] keyValues;
                    if (_singletonKeyValue != null)
                    {
                        keyValues = new[]
                            {
                                new EntityKeyMember(_keyNames[0], _singletonKeyValue)
                            };
                    }
                    else
                    {
                        keyValues = new EntityKeyMember[_compositeKeyValues.Length];
                        for (var i = 0; i < _compositeKeyValues.Length; ++i)
                        {
                            keyValues[i] = new EntityKeyMember(_keyNames[i], _compositeKeyValues[i]);
                        }
                    }
                    return keyValues;
                }
                return null;
            }
            set
            {
                ValidateWritable(_keyNames);
                if (value != null)
                {
                    if (
                        !InitializeKeyValues(
                            new KeyValueReader(value), allowNullKeys: true, tokenizeStrings: true))
                    {
                        // If we did not retrieve values from the setter (i.e. encoded settings), we need to keep track of the 
                        // array instance because the array members will be set next.
                        _deserializedMembers = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="T:System.Data.Entity.Core.EntityKey" /> is temporary.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Entity.Core.EntityKey" /> is temporary; otherwise, false.
        /// </returns>
        public bool IsTemporary
        {
            get { return (SingletonKeyValue == null) && (CompositeKeyValues == null); }
        }

        private object SingletonKeyValue
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _singletonKeyValue;
            }
        }

        private object[] CompositeKeyValues
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _compositeKeyValues;
            }
        }

        /// <summary>Gets the entity set for this entity key from the given metadata workspace.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> for the entity key.
        /// </returns>
        /// <param name="metadataWorkspace">The metadata workspace that contains the entity.</param>
        /// <exception cref="T:System.ArgumentException">The entity set could not be located in the specified metadata workspace.</exception>
        public EntitySet GetEntitySet(MetadataWorkspace metadataWorkspace)
        {
            Check.NotNull(metadataWorkspace, "metadataWorkspace");
            if (String.IsNullOrEmpty(_entityContainerName)
                || String.IsNullOrEmpty(_entitySetName))
            {
                throw new InvalidOperationException(Strings.EntityKey_MissingEntitySetName);
            }

            // GetEntityContainer will throw if it cannot find the container

            // SQLBUDT 479443:  If this entity key was initially created using an entity set 
            // from a different workspace, look up the entity set in the new workspace.
            // Metadata will throw an ArgumentException if the entity set could not be found.

            return metadataWorkspace
                .GetEntityContainer(_entityContainerName, DataSpace.CSpace)
                .GetEntitySetByName(_entitySetName, false);
        }

        #region Equality/Hashing

        /// <summary>Returns a value that indicates whether this instance is equal to a specified object. </summary>
        /// <returns>true if this instance and  obj  have equal values; otherwise, false. </returns>
        /// <param name="obj">
        /// An <see cref="T:System.Object" /> to compare with this instance.
        /// </param>
        public override bool Equals(object obj)
        {
            return InternalEquals(this, obj as EntityKey, compareEntitySets: true);
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityKey" />
        /// .
        /// </summary>
        /// <returns>true if this instance and  other  have equal values; otherwise, false. </returns>
        /// <param name="other">
        /// An <see cref="T:System.Data.Entity.Core.EntityKey" /> object to compare with this instance.
        /// </param>
        public bool Equals(EntityKey other)
        {
            return InternalEquals(this, other, compareEntitySets: true);
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="T:System.Data.Entity.Core.EntityKey" /> object.
        /// <see
        ///     cref="M:System.Data.Entity.Core.EntityKey.GetHashCode" />
        /// is suitable for hashing algorithms and data structures such as a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = _hashCode;
            if (0 == hashCode)
            {
                _containsByteArray = false;

                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }

                if (_entitySetName != null)
                {
                    hashCode = _entitySetName.GetHashCode();
                }
                if (_entityContainerName != null)
                {
                    hashCode ^= _entityContainerName.GetHashCode();
                }

                // If the key is not temporary, determine a hash code based on the value(s) within the key.
                if (null != _singletonKeyValue)
                {
                    hashCode = AddHashValue(hashCode, _singletonKeyValue);
                }
                else if (null != _compositeKeyValues)
                {
                    for (int i = 0, n = _compositeKeyValues.Length; i < n; i++)
                    {
                        hashCode = AddHashValue(hashCode, _compositeKeyValues[i]);
                    }
                }
                else
                {
                    // If the key is temporary, use default hash code
                    hashCode = base.GetHashCode();
                }

                // cache the hash code if we are a locked or fully specified EntityKey
                if (_isLocked || (!String.IsNullOrEmpty(_entitySetName) &&
                                  !String.IsNullOrEmpty(_entityContainerName) &&
                                  (_singletonKeyValue != null || _compositeKeyValues != null)))
                {
                    _hashCode = hashCode;
                }
            }
            return hashCode;
        }

        private int AddHashValue(int hashCode, object keyValue)
        {
            var byteArrayValue = keyValue as byte[];
            if (null != byteArrayValue)
            {
                hashCode ^= ByValueEqualityComparer.ComputeBinaryHashCode(byteArrayValue);
                _containsByteArray = true;
                return hashCode;
            }
            else
            {
                return hashCode ^ keyValue.GetHashCode();
            }
        }

        /// <summary>
        /// Compares two <see cref="T:System.Data.Entity.Core.EntityKey" /> objects.
        /// </summary>
        /// <returns>true if the  key1  and  key2  values are equal; otherwise, false.</returns>
        /// <param name="key1">
        /// A <see cref="T:System.Data.Entity.Core.EntityKey" /> to compare.
        /// </param>
        /// <param name="key2">
        /// A <see cref="T:System.Data.Entity.Core.EntityKey" /> to compare.
        /// </param>
        public static bool operator ==(EntityKey key1, EntityKey key2)
        {
            return InternalEquals(key1, key2, compareEntitySets: true);
        }

        /// <summary>
        /// Compares two <see cref="T:System.Data.Entity.Core.EntityKey" /> objects.
        /// </summary>
        /// <returns>true if the  key1  and  key2  values are not equal; otherwise, false.</returns>
        /// <param name="key1">
        /// A <see cref="T:System.Data.Entity.Core.EntityKey" /> to compare.
        /// </param>
        /// <param name="key2">
        /// A <see cref="T:System.Data.Entity.Core.EntityKey" /> to compare.
        /// </param>
        public static bool operator !=(EntityKey key1, EntityKey key2)
        {
            return !InternalEquals(key1, key2, compareEntitySets: true);
        }

        // <summary>
        // Internal function to compare two keys by their values.
        // </summary>
        // <param name="key1"> a key to compare </param>
        // <param name="key2"> a key to compare </param>
        // <param name="compareEntitySets"> Entity sets are not significant for conceptual null keys </param>
        // <returns> true if the two keys are equal, false otherwise </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool InternalEquals(EntityKey key1, EntityKey key2, bool compareEntitySets)
        {
            // If both are null or refer to the same object, they're equal.
            if (ReferenceEquals(key1, key2))
            {
                return true;
            }

            // If exactly one is null (avoid calling EntityKey == operator overload), they're not equal.
            if (ReferenceEquals(key1, null)
                || ReferenceEquals(key2, null))
            {
                return false;
            }

            // We know they are not both special keys, so if either key is special then the keys are not equal.
            if (ReferenceEquals(NoEntitySetKey, key1)
                || ReferenceEquals(EntityNotValidKey, key1)
                || ReferenceEquals(NoEntitySetKey, key2)
                || ReferenceEquals(EntityNotValidKey, key2))
            {
                return false;
            }

            // If the hash codes differ, the keys are not equal.  Note that 
            // a key's hash code is cached after being computed for the first time, 
            // so this check will only incur the cost of computing a hash code 
            // at most once for a given key.

            // The primary caller is Dictionary<EntityKey,ObjectStateEntry>
            // at which point Equals is only called after HashCode was determined to be equal
            if ((key1.GetHashCode() != key2.GetHashCode() && compareEntitySets)
                ||
                key1._containsByteArray != key2._containsByteArray)
            {
                return false;
            }

            if (null != key1._singletonKeyValue)
            {
                if (key1._containsByteArray)
                {
                    // Compare the single value (if the second is null, false should be returned)
                    if (null == key2._singletonKeyValue)
                    {
                        return false;
                    }

                    // they are both byte[] because they have the same _containsByteArray value of true, and only a single value
                    if (!ByValueEqualityComparer.CompareBinaryValues((byte[])key1._singletonKeyValue, (byte[])key2._singletonKeyValue))
                    {
                        return false;
                    }
                }
                else
                {
                    // not a byte array
                    if (!key1._singletonKeyValue.Equals(key2._singletonKeyValue))
                    {
                        return false;
                    }
                }

                // Check key names
                if (!String.Equals(key1._keyNames[0], key2._keyNames[0]))
                {
                    return false;
                }
            }
            else
            {
                // If either key is temporary, they're not equal.  This is because
                // temporary keys are compared by CLR reference, and we've already
                // checked reference equality.
                // If the first key is a composite key and the second one isn't, they're not equal.
                if (null != key1._compositeKeyValues
                    && null != key2._compositeKeyValues
                    && key1._compositeKeyValues.Length == key2._compositeKeyValues.Length)
                {
                    if (key1._containsByteArray)
                    {
                        if (!CompositeValuesWithBinaryEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!CompositeValuesEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            if (compareEntitySets)
            {
                // Check metadata.
                if (!String.Equals(key1._entitySetName, key2._entitySetName)
                    ||
                    !String.Equals(key1._entityContainerName, key2._entityContainerName))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CompositeValuesWithBinaryEqual(EntityKey key1, EntityKey key2)
        {
            for (var i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!ByValueEqualityComparer.Default.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesWithBinaryEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesWithBinaryEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (var i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return ByValueEqualityComparer.Default.Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        private static bool CompositeValuesEqual(EntityKey key1, EntityKey key2)
        {
            for (var i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (var i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        #endregion

        // <summary>
        // Returns an array of string/<see cref="DbExpression" /> pairs, one for each key value in this EntityKey,
        // where the string is the key member name and the DbExpression is the value in this EntityKey
        // for that key member, represented as a <see cref="DbConstantExpression" /> with the same result
        // type as the key member.
        // </summary>
        // <param name="entitySet"> The entity set to which this EntityKey refers; used to verify that this key has the required key members </param>
        // <returns> The name -> expression mappings for the key member values represented by this EntityKey </returns>
        internal KeyValuePair<string, DbExpression>[] GetKeyValueExpressions(EntitySet entitySet)
        {
            Debug.Assert(!IsTemporary, "GetKeyValueExpressions doesn't make sense for temporary keys - they have no values.");
            DebugCheck.NotNull(entitySet);
            Debug.Assert(entitySet.Name == _entitySetName, "EntitySet returned from GetEntitySet has incorrect name.");
            var numKeyMembers = 0;
            if (!IsTemporary)
            {
                if (_singletonKeyValue != null)
                {
                    numKeyMembers = 1;
                }
                else
                {
                    numKeyMembers = _compositeKeyValues.Length;
                }
            }
            if (((EntitySetBase)entitySet).ElementType.KeyMembers.Count != numKeyMembers)
            {
                // If we found an entity set by name that's a different CLR reference 
                // than the one contained by this EntityKey, the two entity sets could
                // be incompatible.  The only error case we need to handle here is the
                // one where the number of key members differs; other error cases
                // will be handled by the command tree builder methods.

                // FUTURE_FEATURE SQLPT 300003053:  When there exists a method to do
                // structural equivalent of metadata types, this error check should be changed to an 
                // assert.
                throw new ArgumentException(
                    Strings.EntityKey_EntitySetDoesNotMatch(TypeHelpers.GetFullName(entitySet.EntityContainer.Name, entitySet.Name)),
                    "entitySet");
            }

            // Iterate over the internal collection of string->object
            // key value pairs and create a list of string->constant
            // expression key value pairs.
            KeyValuePair<string, DbExpression>[] keyColumns;
            if (_singletonKeyValue != null)
            {
                var singletonKeyMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[0];
                Debug.Assert(singletonKeyMember != null, "Metadata for singleton key member shouldn't be null.");
                keyColumns =
                    new[]
                        {
                            Helper.GetModelTypeUsage(singletonKeyMember).Constant(_singletonKeyValue)
                                  .As(singletonKeyMember.Name)
                        };
            }
            else
            {
                keyColumns = new KeyValuePair<string, DbExpression>[_compositeKeyValues.Length];
                for (var i = 0; i < _compositeKeyValues.Length; ++i)
                {
                    Debug.Assert(_compositeKeyValues[i] != null, "Values within key-value pairs cannot be null.");

                    var keyMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
                    Debug.Assert(keyMember != null, "Metadata for key members shouldn't be null.");
                    keyColumns[i] = Helper.GetModelTypeUsage(keyMember).Constant(_compositeKeyValues[i]).As(keyMember.Name);
                }
            }

            return keyColumns;
        }

        // <summary>
        // Returns a string representation of this EntityKey, for use in debugging.
        // Note that the returned string contains potentially sensitive information
        // (i.e., key values), and thus shouldn't be publicly exposed.
        // </summary>
        internal string ConcatKeyValue()
        {
            var builder = new StringBuilder();
            builder.Append("EntitySet=").Append(_entitySetName);
            if (!IsTemporary)
            {
                foreach (var pair in EntityKeyValues)
                {
                    builder.Append(';');
                    builder.Append(pair.Key).Append("=").Append(pair.Value);
                }
            }
            return builder.ToString();
        }

        // <summary>
        // Returns the appropriate value for the given key name.
        // </summary>
        internal object FindValueByName(string keyName)
        {
            Debug.Assert(!IsTemporary, "FindValueByName should not be called for temporary keys.");
            if (SingletonKeyValue != null)
            {
                Debug.Assert(_keyNames[0] == keyName, "For a singleton key, the given keyName must match.");
                return _singletonKeyValue;
            }
            else
            {
                var compositeKeyValues = CompositeKeyValues;
                for (var i = 0; i < compositeKeyValues.Length; i++)
                {
                    if (keyName == _keyNames[i])
                    {
                        return compositeKeyValues[i];
                    }
                }
                throw new ArgumentOutOfRangeException("keyName");
            }
        }

        internal void InitializeEntitySetName(string qualifiedEntitySetName)
        {
            DebugCheck.NotEmpty(qualifiedEntitySetName);

            var result = qualifiedEntitySetName.Split('.');
            if (result.Length != 2
                || string.IsNullOrWhiteSpace(result[0])
                || string.IsNullOrWhiteSpace(result[1]))
            {
                throw new ArgumentException(Strings.EntityKey_InvalidQualifiedEntitySetName, "qualifiedEntitySetName");
            }

            _entityContainerName = result[0];
            _entitySetName = result[1];

            ValidateName(_entityContainerName);
            ValidateName(_entitySetName);
        }

        private static void ValidateName(string name)
        {
            if (!name.IsValidUndottedName())
            {
                throw new ArgumentException(Strings.EntityKey_InvalidName(name));
            }
        }

        #region Key Value Assignment and Validation

        internal bool InitializeKeyValues(
            IEnumerable<KeyValuePair<string, object>> entityKeyValues,
            bool allowNullKeys = false,
            bool tokenizeStrings = false)
        {
            DebugCheck.NotNull(entityKeyValues);

            var numExpectedKeyValues = entityKeyValues.Count();
            if (numExpectedKeyValues == 1)
            {
                _keyNames = new string[1];

                var keyValuePair = entityKeyValues.Single();
                InitializeKeyValue(keyValuePair, 0, tokenizeStrings);
                _singletonKeyValue = keyValuePair.Value;
            }
            else if (numExpectedKeyValues > 1)
            {
                _keyNames = new string[numExpectedKeyValues];
                _compositeKeyValues = new object[numExpectedKeyValues];

                var i = 0;
                foreach (var keyValuePair in entityKeyValues)
                {
                    InitializeKeyValue(keyValuePair, i, tokenizeStrings);
                    _compositeKeyValues[i] = keyValuePair.Value;
                    i++;
                }
            }
            else if (!allowNullKeys)
            {
                throw new ArgumentException(Strings.EntityKey_EntityKeyMustHaveValues, "entityKeyValues");
            }

            return numExpectedKeyValues > 0;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private void InitializeKeyValue(KeyValuePair<string, object> keyValuePair, int i, bool tokenizeStrings)
        {
            if (EntityUtil.IsNull(keyValuePair.Value)
                || string.IsNullOrWhiteSpace(keyValuePair.Key))
            {
                throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "entityKeyValues");
            }
            ValidateName(keyValuePair.Key);
            _keyNames[i] = tokenizeStrings ? LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
        }

        // <summary>
        // Validates the record parameter passed to the EntityKey constructor,
        // and converts the data into the form required by EntityKey.  For singleton keys,
        // this is a single object.  For composite keys, this is an object array.
        // </summary>
        // <param name="entitySet"> the entity set metadata object which this key refers to </param>
        // <param name="record"> the parameter to validate </param>
        private void InitializeKeyValues(EntitySet entitySet, IExtendedDataRecord record)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(record);

            // Note that this method is only called when constructing keys from internal code
            // paths such as the materializer and therefore uses Asserts to check conditions
            // that will be compiled out of the retail build.

            var numExpectedKeyValues = entitySet.ElementType.KeyMembers.Count;
            Debug.Assert(numExpectedKeyValues > 0);

            _keyNames = entitySet.ElementType.KeyMemberNames;

            Debug.Assert(record.DataRecordInfo.RecordType.EdmType is EntityType);
            var entityType = (EntityType)record.DataRecordInfo.RecordType.EdmType;

            Debug.Assert(entitySet.ElementType.IsAssignableFrom(entityType));

            if (numExpectedKeyValues == 1)
            {
                // Optimize for key with just one property.
                _singletonKeyValue = record[entityType.KeyMembers[0].Name];

                // We have to throw here rather than asserting because elsewhere in the stack we depend on catching this exception.
                if (EntityUtil.IsNull(_singletonKeyValue))
                {
                    throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "record");
                }
            }
            else
            {
                _compositeKeyValues = new object[numExpectedKeyValues];

                for (var i = 0; i < numExpectedKeyValues; ++i)
                {
                    _compositeKeyValues[i] = record[entityType.KeyMembers[i].Name];

                    // We have to throw here rather than asserting because elsewhere in the stack we depend on catching this exception.
                    if (EntityUtil.IsNull(_compositeKeyValues[i]))
                    {
                        throw new ArgumentException(Strings.EntityKey_NoNullsAllowedInKeyValuePairs, "record");
                    }
                }
            }
        }

        // <summary>
        // Verify that the types of the objects passed in to be used as keys actually match the types from the model.
        // This error is also caught when the entity is materialized and when the key value is set, at which time it
        // also throws ThrowSetInvalidValue().
        // SQLBUDT 513838. This error is possible and should be caught at run time, not in an assertion.
        // </summary>
        // <param name="workspace"> MetadataWorkspace used to resolve and validate types of enum keys. </param>
        // <param name="entitySet"> The EntitySet to validate against </param>
        internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet)
        {
            ValidateEntityKey(workspace, entitySet, false, null);
        }

        // <summary>
        // Verify that the types of the objects passed in to be used as keys actually match the types from the model.
        // This error is also caught when the entity is materialized and when the key value is set, at which time it
        // also throws ThrowSetInvalidValue().
        // SQLBUDT 513838. This error is possible and should be caught at run time, not in an assertion.
        // </summary>
        // <param name="workspace"> MetadataWorkspace used to resolve and validate types of enum keys. </param>
        // <param name="entitySet"> The EntitySet to validate against </param>
        // <param name="isArgumentException"> Wether to throw ArgumentException or InvalidOperationException. </param>
        // <param name="argumentName"> Name of the argument in case of ArgumentException. </param>
        internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet, bool isArgumentException, string argumentName)
        {
            if (entitySet != null)
            {
                var keyMembers = ((EntitySetBase)entitySet).ElementType.KeyMembers;
                if (_singletonKeyValue != null)
                {
                    // 1. Validate number of keys
                    if (keyMembers.Count != 1)
                    {
                        if (isArgumentException)
                        {
                            throw new ArgumentException(
                                Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, 1),
                                argumentName);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Strings.EntityKey_IncorrectNumberOfKeyValuePairs(entitySet.ElementType.FullName, keyMembers.Count, 1));
                        }
                    }

                    // 2. Validate type of key values
                    ValidateTypeOfKeyValue(workspace, keyMembers[0], _singletonKeyValue, isArgumentException, argumentName);

                    // 3. Validate key names
                    if (_keyNames[0]
                        != keyMembers[0].Name)
                    {
                        if (isArgumentException)
                        {
                            throw new ArgumentException(
                                Strings.EntityKey_MissingKeyValue(keyMembers[0].Name, entitySet.ElementType.FullName), argumentName);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Strings.EntityKey_MissingKeyValue(keyMembers[0].Name, entitySet.ElementType.FullName));
                        }
                    }
                }
                else if (null != _compositeKeyValues)
                {
                    // 1. Validate number of keys
                    if (keyMembers.Count
                        != _compositeKeyValues.Length)
                    {
                        if (isArgumentException)
                        {
                            throw new ArgumentException(
                                Strings.EntityKey_IncorrectNumberOfKeyValuePairs(
                                    entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length), argumentName);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Strings.EntityKey_IncorrectNumberOfKeyValuePairs(
                                    entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length));
                        }
                    }

                    for (var i = 0; i < _compositeKeyValues.Length; ++i)
                    {
                        var keyField = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
                        var foundMember = false;
                        for (var j = 0; j < _compositeKeyValues.Length; ++j)
                        {
                            if (keyField.Name
                                == _keyNames[j])
                            {
                                // 2. Validate type of key values
                                ValidateTypeOfKeyValue(workspace, keyField, _compositeKeyValues[j], isArgumentException, argumentName);

                                foundMember = true;
                                break;
                            }
                        }
                        // 3. Validate Key Name (if we found it or not)
                        if (!foundMember)
                        {
                            if (isArgumentException)
                            {
                                throw new ArgumentException(
                                    Strings.EntityKey_MissingKeyValue(keyField.Name, entitySet.ElementType.FullName), argumentName);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    Strings.EntityKey_MissingKeyValue(keyField.Name, entitySet.ElementType.FullName));
                            }
                        }
                    }
                }
            }
        }

        // <summary>
        // Validates whether type of the key matches the type of the key value.
        // </summary>
        // <param name="workspace"> MetadataWorkspace used to resolve and validate types of enum keys. </param>
        // <param name="keyMember"> Edm key member. </param>
        // <param name="keyValue"> The value of the key. </param>
        // <param name="isArgumentException"> Whether to throw ArgumentException or InvalidOperation exception if validation fails. </param>
        // <param name="argumentName"> Name of the argument to be used for ArgumentExceptions. </param>
        private static void ValidateTypeOfKeyValue(
            MetadataWorkspace workspace, EdmMember keyMember, object keyValue, bool isArgumentException, string argumentName)
        {
            DebugCheck.NotNull(workspace);
            DebugCheck.NotNull(keyMember);
            DebugCheck.NotNull(keyValue);
            Debug.Assert(Helper.IsScalarType(keyMember.TypeUsage.EdmType), "key member must be of a scalar type");

            var keyMemberEdmType = keyMember.TypeUsage.EdmType;

            if (Helper.IsPrimitiveType(keyMemberEdmType))
            {
                var entitySetKeyType = ((PrimitiveType)keyMemberEdmType).ClrEquivalentType;
                if (entitySetKeyType != keyValue.GetType())
                {
                    if (isArgumentException)
                    {
                        throw new ArgumentException(
                            Strings.EntityKey_IncorrectValueType(keyMember.Name, entitySetKeyType.FullName, keyValue.GetType().FullName),
                            argumentName);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            Strings.EntityKey_IncorrectValueType(keyMember.Name, entitySetKeyType.FullName, keyValue.GetType().FullName));
                    }
                }
            }
            else
            {
                Debug.Assert(Helper.IsEnumType(keyMember.TypeUsage.EdmType), "Enum type expected");

                EnumType expectedEnumType;
                if (workspace.TryGetObjectSpaceType((EnumType)keyMemberEdmType, out expectedEnumType))
                {
                    var expectedClrEnumType = (expectedEnumType).ClrType;
                    if (expectedClrEnumType != keyValue.GetType())
                    {
                        if (isArgumentException)
                        {
                            throw new ArgumentException(
                                Strings.EntityKey_IncorrectValueType(
                                    keyMember.Name, expectedClrEnumType.FullName, keyValue.GetType().FullName), argumentName);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Strings.EntityKey_IncorrectValueType(
                                    keyMember.Name, expectedClrEnumType.FullName, keyValue.GetType().FullName));
                        }
                    }
                }
                else
                {
                    if (isArgumentException)
                    {
                        throw new ArgumentException(
                            Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyMember.Name, keyMemberEdmType.FullName),
                            argumentName);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            Strings.EntityKey_NoCorrespondingOSpaceTypeForEnumKeyMember(keyMember.Name, keyMemberEdmType.FullName));
                    }
                }
            }
        }

        // <summary>
        // Asserts that the "state" of the EntityKey is correct, by validating assumptions
        // based on whether the key is a singleton, composite, or temporary.
        // </summary>
        // <param name="isTemporary"> whether we expect this EntityKey to be marked temporary </param>
        [Conditional("DEBUG")]
        private void AssertCorrectState(EntitySetBase entitySetBase, bool isTemporary)
        {
            var entitySet = (EntitySet)entitySetBase;
            if (_singletonKeyValue != null)
            {
                Debug.Assert(!isTemporary);
                Debug.Assert(_compositeKeyValues == null);
                if (entitySetBase != null)
                {
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count == 1);
                }
            }
            else if (_compositeKeyValues != null)
            {
                Debug.Assert(!isTemporary);
                if (entitySetBase != null)
                {
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count > 1);
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count == _compositeKeyValues.Length);
                }
                for (var i = 0; i < _compositeKeyValues.Length; ++i)
                {
                    Debug.Assert(_compositeKeyValues[i] != null);
                }
            }
            else if (!IsTemporary)
            {
                // one of our static keys
                Debug.Assert(EntityKeyValues == null);
                Debug.Assert(EntityContainerName == null);
                Debug.Assert(EntitySetName != null);
            }
            else
            {
                Debug.Assert(EntityKeyValues == null);
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Helper method that is used to deserialize an <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </summary>
        /// <param name="context">Describes the source and destination of a given serialized stream, and provides an additional caller-defined context.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [OnDeserializing]
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        public void OnDeserializing(StreamingContext context)
        {
            if (RequiresDeserialization)
            {
                DeserializeMembers();
            }
        }

        /// <summary>
        /// Helper method that is used to deserialize an <see cref="T:System.Data.Entity.Core.EntityKey" />.
        /// </summary>
        /// <param name="context">Describes the source and destination of a given serialized stream and provides an additional caller-defined context.</param>
        [OnDeserialized]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        public void OnDeserialized(StreamingContext context)
        {
            _entitySetName = LookupSingletonName(_entitySetName);
            _entityContainerName = LookupSingletonName(_entityContainerName);
            if (_keyNames != null)
            {
                for (var i = 0; i < _keyNames.Length; i++)
                {
                    _keyNames[i] = LookupSingletonName(_keyNames[i]);
                }
            }
        }

        // <summary>
        // Dev Note: this must be called from within a _lock block on _nameLookup
        // </summary>
        internal static string LookupSingletonName(string name)
        {
            return string.IsNullOrEmpty(name) ? null : NameLookup.GetOrAdd(name, n => n);
        }

        private void ValidateWritable(object instance)
        {
            if (_isLocked || instance != null)
            {
                throw new InvalidOperationException(Strings.EntityKey_CannotChangeKey);
            }
        }

        private bool RequiresDeserialization
        {
            get { return _deserializedMembers != null; }
        }

        private void DeserializeMembers()
        {
            if (InitializeKeyValues(
                new KeyValueReader(_deserializedMembers), allowNullKeys: true, tokenizeStrings: true))
            {
                // If we received values from the _deserializedMembers, then we do not need to track these any more
                _deserializedMembers = null;
            }
        }

        #endregion

        private class KeyValueReader : IEnumerable<KeyValuePair<string, object>>
        {
            private readonly IEnumerable<EntityKeyMember> _enumerator;

            public KeyValueReader(IEnumerable<EntityKeyMember> enumerator)
            {
                _enumerator = enumerator;
            }

            #region IEnumerable<KeyValuePair<string,object>> Members

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (var pair in _enumerator)
                {
                    if (pair != null)
                    {
                        yield return new KeyValuePair<string, object>(pair.Key, pair.Value);
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
    }
}
