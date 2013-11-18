// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// An implementation of IDatabaseInitializer that will recreate and optionally re-seed the
    /// database only if the database does not exist.
    /// To seed the database, create a derived class and override the Seed method.
    /// </summary>
    /// <typeparam name="TContext"> The type of the context. </typeparam>
    public class CreateDatabaseIfNotExists<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Data.Entity.CreateDatabaseIfNotExists`1" /> class.</summary>
        public CreateDatabaseIfNotExists()
        {
        }

        #region Strategy implementation

        static CreateDatabaseIfNotExists()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        /// Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name="context"> The context. </param>
        public void InitializeDatabase(TContext context)
        {
            Check.NotNull(context, "context");

            var existence = new DatabaseTableChecker().AnyModelTableExists(context.InternalContext);

            if (existence == DatabaseExistenceState.Exists)
            {
                // If there is no metadata either in the model or in the database, then
                // we assume that the database matches the model because the common cases for
                // these scenarios are database/model first and/or an existing database.
                if (!context.Database.CompatibleWithModel(throwIfNoMetadata: false, existenceState: existence))
                {
                    throw Error.DatabaseInitializationStrategy_ModelMismatch(context.GetType().Name);
                }
            }
            else
            {
                // Either the database doesn't exist, or exists and is considered empty
                context.Database.Create(existence);
                Seed(context);
                context.SaveChanges();
            }
        }

        #endregion

        #region Seeding methods

        /// <summary>
        /// A method that should be overridden to actually add data to the context for seeding.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="context"> The context to seed. </param>
        protected virtual void Seed(TContext context)
        {
        }

        #endregion
    }
}
