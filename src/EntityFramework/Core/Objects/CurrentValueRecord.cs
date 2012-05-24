namespace System.Data.Entity.Core.Objects
{
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
