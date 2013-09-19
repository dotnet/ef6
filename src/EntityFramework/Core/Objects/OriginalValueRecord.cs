// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// The original values of the properties of an entity when it was retrieved from the database.
    /// </summary>
    public abstract class OriginalValueRecord : DbUpdatableDataRecord
    {
        internal OriginalValueRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            :
                base(cacheEntry, metadata, userObject)
        {
        }
    }
}
