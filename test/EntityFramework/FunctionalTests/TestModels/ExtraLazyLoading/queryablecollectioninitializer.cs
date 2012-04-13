namespace LazyUnicorns
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;

    public class QueryableCollectionInitializer : CachingCollectionInitializer
    {
        public override object CreateCollection<TElement>(DbCollectionEntry collectionEntry)
        {
            return new QueryableCollection<TElement>(
                (ICollection<TElement>)collectionEntry.CurrentValue,
                collectionEntry.Query().Cast<TElement>());
        }
    }
}
