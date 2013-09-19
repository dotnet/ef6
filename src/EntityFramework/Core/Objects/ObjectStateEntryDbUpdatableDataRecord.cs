// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal sealed class ObjectStateEntryDbUpdatableDataRecord : CurrentValueRecord
    {
        internal ObjectStateEntryDbUpdatableDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            : base(cacheEntry, metadata, userObject)
        {
            DebugCheck.NotNull(cacheEntry);
            DebugCheck.NotNull(userObject);
            DebugCheck.NotNull(metadata);
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbUpdatableDataRecord for a key entry");

            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    Debug.Assert(
                        false, "A CurrentValueRecord cannot be created for an entity object that is in a deleted or detached state.");
                    break;
            }
        }

        internal ObjectStateEntryDbUpdatableDataRecord(RelationshipEntry cacheEntry)
            : base(cacheEntry)
        {
            DebugCheck.NotNull(cacheEntry);

            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    Debug.Assert(
                        false, "A CurrentValueRecord cannot be created for an entity object that is in a deleted or detached state.");
                    break;
            }
        }

        protected override object GetRecordValue(int ordinal)
        {
            if (_cacheEntry.IsRelationship)
            {
                return (_cacheEntry as RelationshipEntry).GetCurrentRelationValue(ordinal);
            }
            else
            {
                return (_cacheEntry as EntityEntry).GetCurrentEntityValue(
                    _metadata, ordinal, _userObject, ObjectStateValueRecord.CurrentUpdatable);
            }
        }

        protected override void SetRecordValue(int ordinal, object value)
        {
            if (_cacheEntry.IsRelationship)
            {
                // Cannot modify relation values from the public API
                throw new InvalidOperationException(Strings.ObjectStateEntry_CantModifyRelationValues);
            }
            else
            {
                (_cacheEntry as EntityEntry).SetCurrentEntityValue(_metadata, ordinal, _userObject, value);
            }
        }
    }
}
