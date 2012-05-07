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
