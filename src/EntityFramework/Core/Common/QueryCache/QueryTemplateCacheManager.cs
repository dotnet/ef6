namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Collections.Concurrent;

    internal class QueryTemplateCacheManager
    {
        private readonly ConcurrentDictionary<LinqQueryCacheKey, string> staticCommandCache = new ConcurrentDictionary<LinqQueryCacheKey, string>();

        public static QueryTemplateCacheManager Instance { get; } = new QueryTemplateCacheManager();

        public string GetExecutionPlanTemplate(LinqQueryCacheKey key)
        {
            string cachedStaticQueryPlan = null;
            if (this.staticCommandCache.TryGetValue(key, out cachedStaticQueryPlan))
            {
                return cachedStaticQueryPlan;
            }

            return null;
        }

        public void AddExecutionPlanTemplate(LinqQueryCacheKey key, string queryTemplate)
        {
            this.staticCommandCache.TryAdd(key, queryTemplate);
        }
    }
}
