// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Transactions;

    /// <summary>
    ///     An implementation of IDatabaseInitializer that will recreate and optionally re-seed the
    ///     database only if the database does not exist.
    ///     To seed the database, create a derived class and override the Seed method.
    /// </summary>
    /// <typeparam name = "TContext">The type of the context.</typeparam>
    public class CreateDatabaseIfNotExists<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        #region Strategy implementation

        static CreateDatabaseIfNotExists()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        ///     Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name = "context">The context.</param>
        public void InitializeDatabase(TContext context)
        {
            bool databaseExists;
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                databaseExists = context.Database.Exists();
            }

            if (databaseExists && new DatabaseTableChecker().AnyModelTableExists(context))
            {
                // If there is no metadata either in the model or in the databaase, then
                // we assume that the database matches the model because the common cases for
                // these scenarios are database/model first and/or an existing database.
                if (!context.Database.CompatibleWithModel(throwIfNoMetadata: false))
                {
                    throw Error.DatabaseInitializationStrategy_ModelMismatch(context.GetType().Name);
                }
            }
            else
            {
                context.Database.Create(skipExistsCheck: true);
                Seed(context);
                context.SaveChanges();
            }
        }

        #endregion

        #region Seeding methods

        /// <summary>
        ///     A method that should be overridden to actually add data to the context for seeding. 
        ///     The default implementation does nothing.
        /// </summary>
        /// <param name = "context">The context to seed.</param>
        protected virtual void Seed(TContext context)
        {
        }

        #endregion
    }
}
