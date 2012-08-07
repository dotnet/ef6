// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.History;

    internal sealed class DefaultModelCacheKeyFactory : IDbModelCacheKeyFactory
    {
        public IDbModelCacheKey Create(DbContext context)
        {
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
