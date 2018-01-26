namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Collections.Concurrent;

    internal class StaticQueryCacheManager
    {
        private readonly ConcurrentDictionary<LinqQueryCacheKey, string> staticCommandCache = new ConcurrentDictionary<LinqQueryCacheKey, string>();

        public static StaticQueryCacheManager Instance { get; } = new StaticQueryCacheManager();

        public string GetStaticQueryPlan(LinqQueryCacheKey key)
        {
            string cachedStaticQueryPlan = null;
            if (this.staticCommandCache.TryGetValue(key, out cachedStaticQueryPlan))
            {
                return cachedStaticQueryPlan;

            }

            return null;
        }
    }
}
