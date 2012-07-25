// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    // Internal version of writeable original values record is used by all internal operations that need to set original values, such as PreserveChanges queries
    // This version should never be returned to the user, because it doesn't enforce any necessary restrictions.
    // See ObjectStateEntryOriginalDbUpdatableDataRecord_Public for user scenarios.
    internal class ObjectStateEntryOriginalDbUpdatableDataRecord_Internal : OriginalValueRecord
    {
        internal ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(
            EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            : base(cacheEntry, metadata, userObject)
        {
            Contract.Requires(cacheEntry != null);
            Contract.Requires(userObject != null);
            Contract.Requires(metadata != null);
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryOriginalDbUpdatableDataRecord_Internal for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    break;
                default:
                    Debug.Assert(false, "An OriginalValueRecord cannot be created for an object in an added or detached state.");
                    break;
            }
        }

        protected override object GetRecordValue(int ordinal)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            return (_cacheEntry as EntityEntry).GetOriginalEntityValue(
                _metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatableInternal);
        }

        protected override void SetRecordValue(int ordinal, object value)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            (_cacheEntry as EntityEntry).SetOriginalEntityValue(_metadata, ordinal, _userObject, value);
        }
    }
}
