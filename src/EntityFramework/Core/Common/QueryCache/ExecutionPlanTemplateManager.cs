namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Collections.Concurrent;

    internal class ExecutionPlanTemplateManager
    {
        private readonly ConcurrentDictionary<LinqQueryCacheKey, string> templates = new ConcurrentDictionary<LinqQueryCacheKey, string>();

        public static ExecutionPlanTemplateManager Instance { get; } = new ExecutionPlanTemplateManager();

        public string GetExecutionPlanTemplate(LinqQueryCacheKey key)
        {
            string cachedTemplate = null;
            if (this.templates.TryGetValue(key, out cachedTemplate))
            {
                return cachedTemplate;
            }

            return null;
        }

        public void AddExecutionPlanTemplate(LinqQueryCacheKey key, string template)
        {
            this.templates.TryAdd(key, template);
        }
    }
}
