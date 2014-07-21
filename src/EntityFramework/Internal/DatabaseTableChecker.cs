// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Transactions;

    internal class DatabaseTableChecker
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public DatabaseExistenceState AnyModelTableExists(InternalContext internalContext)
        {
            var exists = internalContext.DatabaseOperations.Exists(
                internalContext.Connection,
                internalContext.CommandTimeout,
                new Lazy<StoreItemCollection>(() => CreateStoreItemCollection(internalContext)));

            if (!exists)
            {
                return DatabaseExistenceState.DoesNotExist;
            }

            using (var clonedObjectContext = internalContext.CreateObjectContextForDdlOps())
            {
                try
                {
                    if (internalContext.CodeFirstModel == null)
                    {
                        // If not Code First, then assume tables created in some other way
                        return DatabaseExistenceState.Exists;
                    }

                    var provider = DbConfiguration.DependencyResolver.GetService<TableExistenceChecker>(internalContext.ProviderName);

                    if (provider == null)
                    {
                        // If we can't check for tables, then assume they exist as we did in older versions
                        return DatabaseExistenceState.Exists;
                    }

                    var modelTables = GetModelTables(internalContext).ToList();

                    if (!modelTables.Any())
                    {
                        // If this is an empty model, then all tables that can exist (0) do exist
                        return DatabaseExistenceState.Exists;
                    }

                    if (QueryForTableExistence(provider, clonedObjectContext, modelTables))
                    {
                        // If any table exists, then assume that this is a non-empty database
                        return DatabaseExistenceState.Exists;
                    }

                    // At this point we know no model tables exist. If the history table exists and has an entry
                    // for this context, then treat this as a non-empty database, otherwise treat is as existing
                    // but empty.
                    return internalContext.HasHistoryTableEntry()
                        ? DatabaseExistenceState.Exists
                        : DatabaseExistenceState.ExistsConsideredEmpty;
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message, ex.ToString());

                    // Revert to previous behavior on error
                    return DatabaseExistenceState.Exists;
                }
            }
        }

        private static StoreItemCollection CreateStoreItemCollection(InternalContext internalContext)
        {
            using (var clonedObjectContext = internalContext.CreateObjectContextForDdlOps())
            {
                var entityConnection = ((EntityConnection)clonedObjectContext.ObjectContext.Connection);
                return (StoreItemCollection)entityConnection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
            }
        }

        public virtual bool QueryForTableExistence(
            TableExistenceChecker provider, ClonedObjectContext clonedObjectContext, List<EntitySet> modelTables)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                if (provider.AnyModelTableExistsInDatabase(
                    clonedObjectContext.ObjectContext,
                    clonedObjectContext.Connection,
                    modelTables,
                    EdmMetadataContext.TableName))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual IEnumerable<EntitySet> GetModelTables(InternalContext internalContext)
        {
            return internalContext.ObjectContext.MetadataWorkspace
                .GetItemCollection(DataSpace.SSpace)
                .GetItems<EntityContainer>()
                .Single()
                .BaseEntitySets
                .OfType<EntitySet>()
                .Where(
                    s => !s.MetadataProperties.Contains("Type")
                         || (string)s.MetadataProperties["Type"].Value == "Tables");
        }
    }
}
