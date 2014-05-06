// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Diagnostics;
    using System.Linq;

    internal class EdmMetadataRepository : RepositoryBase
    {
        private readonly DbTransaction _existingTransaction;

        public EdmMetadataRepository(InternalContext usersContext, string connectionString, DbProviderFactory providerFactory)
            : base(usersContext, connectionString, providerFactory)
        {
            _existingTransaction = usersContext.TryGetCurrentStoreTransaction();
        }

        public virtual string QueryForModelHash(Func<DbConnection, EdmMetadataContext> createContext)
        {
            var connection = CreateConnection();
            try
            {
                using (var metadataContext = createContext(connection))
                {
                    if (_existingTransaction != null)
                    {
                        Debug.Assert(_existingTransaction.Connection == connection);

                        if (_existingTransaction.Connection == connection)
                        {
                            metadataContext.Database.UseTransaction(_existingTransaction);
                        }
                    }

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
            finally
            {
                DisposeConnection(connection);
            }
        }
    }
}
