// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    // Public version of writable original values record that is to be returned to the user for setting original values directly.
    // Although this class is actually internal, it is the version that implements the writeable original values functionality returned through the public surface.
    // This version must maintain information about the index of the top-level entity property that corresponds to this record, because the record
    // may represent a complex type somewhere in an entity hierarchy and this is the only way we know which entity property it is associated with.
    // This version also does minimal necessary validation on the values that the user is trying to set.
    internal sealed class ObjectStateEntryOriginalDbUpdatableDataRecord_Public : ObjectStateEntryOriginalDbUpdatableDataRecord_Internal
    {
        // Will be EntityEntry.s_EntityRoot for entities and for complex types will be the index of the top-level entity property related to this complex type
        private readonly int _parentEntityPropertyIndex;

        internal ObjectStateEntryOriginalDbUpdatableDataRecord_Public(
            EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject, int parentEntityPropertyIndex)
            : base(cacheEntry, metadata, userObject)
        {
            _parentEntityPropertyIndex = parentEntityPropertyIndex;
        }

        protected override object GetRecordValue(int ordinal)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            return (_cacheEntry as EntityEntry).GetOriginalEntityValue(
                _metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatablePublic, GetPropertyIndex(ordinal));
        }

        protected override void SetRecordValue(int ordinal, object value)
        {
            var member = _metadata.Member(ordinal);

            // We do not allow setting complex properties through writeable original values.
            // Instead individual scalar properties can be set on a data record that represents the complex type.
            if (member.IsComplex)
            {
                throw new InvalidOperationException(Strings.ObjectStateEntry_SetOriginalComplexProperties(member.CLayerName));
            }

            // Null values are represented in data records as DBNull.Value, so translate appropriately
            var fieldValue = value ?? DBNull.Value;

            var entry = _cacheEntry as EntityEntry;
            var oldState = entry.State;

            // Only update the original values if the new value is different from the value currently set on the entity
            if (entry.HasRecordValueChanged(this, ordinal, fieldValue))
            {
                // Since the original value is going to be set, validate that is doesn't violate any restrictions

                // Throw if trying to change the original value of the primary key
                if (member.IsPartOfKey)
                {
                    throw new InvalidOperationException(Strings.ObjectStateEntry_SetOriginalPrimaryKey(member.CLayerName));
                }

                // Verify non-nullable EDM members are not being set to null
                // Need to continue allowing CLR reference types to be set to null for backwards compatibility
                var memberClrType = member.ClrType;
                if (DBNull.Value == fieldValue
                    &&
                    memberClrType.IsValueType
                    &&
                    !member.CdmMetadata.Nullable)
                {
                    // Throw if the underlying CLR type of this property is not nullable, and it is being set to null
                    throw new InvalidOperationException(
                        Strings.ObjectStateEntry_NullOriginalValueForNonNullableProperty(
                            member.CLayerName, member.ClrMetadata.Name, member.ClrMetadata.DeclaringType.FullName));
                }

                base.SetRecordValue(ordinal, value);

                // Update the state of the ObjectStateEntry if it has been marked as Modified
                if (oldState == EntityState.Unchanged
                    && entry.State == EntityState.Modified)
                {
                    entry.ObjectStateManager.ChangeState(entry, oldState, EntityState.Modified);
                }

                // Set the individual property to modified
                entry.SetModifiedPropertyInternal(GetPropertyIndex(ordinal));
            }
        }

        // For entities the property index is the specified ordinal, but otherwise it's the top-level entity property index that we have saved
        private int GetPropertyIndex(int ordinal)
        {
            return _parentEntityPropertyIndex == EntityEntry.s_EntityRoot ? ordinal : _parentEntityPropertyIndex;
        }
    }
}
