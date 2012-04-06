namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
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
