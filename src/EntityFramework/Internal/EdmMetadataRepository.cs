// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Linq;

    internal class EdmMetadataRepository : RepositoryBase
    {
        public EdmMetadataRepository(string connectionString, DbProviderFactory providerFactory)
            : base(connectionString, providerFactory)
        {
        }

        public virtual string QueryForModelHash(Func<DbConnection, EdmMetadataContext> createContext)
        {
            using (var metadataContext = createContext(CreateConnection()))
            {
                try
                {
                    var edmMetadata =
                        metadataContext.Metadata.AsNoTracking().OrderByDescending(m => m.Id).FirstOrDefault();
                    return edmMetadata != null ? edmMetadata.ModelHash : null;
                }
                catch (EntityCommandExecutionException)
                {
                    return null;
                }
            }
        }
    }
}
