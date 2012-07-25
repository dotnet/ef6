// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    public abstract class OriginalValueRecord : DbUpdatableDataRecord
    {
        internal OriginalValueRecord(ObjectStateEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            :
                base(cacheEntry, metadata, userObject)
        {
        }
    }
}
