namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.History;
    using System.Diagnostics.Contracts;

    internal sealed class DefaultModelCacheKeyFactory : IDbModelCacheKeyFactory
    {
        public IDbModelCacheKey Create(DbContext context)
        {
            Contract.Requires(context != null);

            string defaultSchema = null;

            var historyContext = context as HistoryContext;

            if (historyContext != null)
            {
                defaultSchema = historyContext.DefaultSchema;
            }

            return new DefaultModelCacheKey(context.GetType(), context.InternalContext.ProviderName, defaultSchema);
        }
    }
}
