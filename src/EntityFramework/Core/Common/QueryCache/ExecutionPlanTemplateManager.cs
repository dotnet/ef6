namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Collections.Concurrent;
    using System.Collections;
    using System.Collections.Generic;

    internal class ExecutionPlanTemplateManager
    {
        private readonly ConcurrentDictionary<LinqQueryCacheKey, ExecutionPlanTemplate> templates = new ConcurrentDictionary<LinqQueryCacheKey, ExecutionPlanTemplate>();

        public static ExecutionPlanTemplateManager Instance { get; } = new ExecutionPlanTemplateManager();

        public ExecutionPlanTemplate GetExecutionPlanTemplate(LinqQueryCacheKey key)
        {
            ExecutionPlanTemplate cachedTemplate = null;
            if (this.templates.TryGetValue(key, out cachedTemplate))
            {
                return cachedTemplate;
            }

            return null;
        }

        public void AddExecutionPlanTemplate(LinqQueryCacheKey key, ExecutionPlanTemplate template)
        {
            this.templates.AddOrUpdate(key, template, (oldKey, oldValue) => template);
        }
    }
}
