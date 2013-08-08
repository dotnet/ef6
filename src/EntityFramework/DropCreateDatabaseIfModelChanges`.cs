// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// An implementation of IDatabaseInitializer that will <b>DELETE</b>, recreate, and optionally re-seed the
    /// database only if the model has changed since the database was created.
    /// </summary>
    /// <remarks>
    /// Whether or not the model has changed is determined by the <see cref="Database.CompatibleWithModel(bool)" />
    /// method.
    /// To seed the database create a derived class and override the Seed method.
    /// </remarks>
    public class DropCreateDatabaseIfModelChanges<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        private readonly MigrationsChecker _migrationsChecker;

        /// <summary>Initializes a new instance of the <see cref="T:System.Data.Entity.DropCreateDatabaseIfModelChanges`1" /> class.</summary>
        public DropCreateDatabaseIfModelChanges()
            : this(null)
        {
        }

        internal DropCreateDatabaseIfModelChanges(MigrationsChecker migrationsChecker)
        {
            _migrationsChecker = migrationsChecker ?? new MigrationsChecker();
        }

        #region Strategy implementation

        static DropCreateDatabaseIfModelChanges()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        /// Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="context" />
        /// is
        /// <c>null</c>
        /// .
        /// </exception>
        public void InitializeDatabase(TContext context)
        {
            Check.NotNull(context, "context");

            var exists = new DatabaseTableChecker().AnyModelTableExists(context.InternalContext);

            if (_migrationsChecker.IsMigrationsConfigured(
                context.InternalContext,
                () =>
                {
                    if (exists && !context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                    {
                        throw Error.DatabaseInitializationStrategy_ModelMismatch(context.GetType().Name);
                    }

                    return exists;
                }))
            {
                return;
            }

            if (exists)
            {
                if (context.Database.CompatibleWithModel(throwIfNoMetadata: true))
                {
                    return;
                }

                context.Database.Delete();
            }

            // Database didn't exist or we deleted it, so we now create it again.
            context.Database.Create(skipExistsCheck: true);

            Seed(context);
            context.SaveChanges();
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
