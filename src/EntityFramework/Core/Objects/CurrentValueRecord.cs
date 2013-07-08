// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// The values currently assigned to the properties of an entity.
    /// </summary>
    public abstract class CurrentValueRecord : DbUpdatableDataRecord
    {
        internal CurrentValueRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            :
                base(cacheEntry, metadata, userObject)
        {
        }

        internal CurrentValueRecord(ObjectStateEntry cacheEntry)
            :
                base(cacheEntry)
        {
        }
    }
}
