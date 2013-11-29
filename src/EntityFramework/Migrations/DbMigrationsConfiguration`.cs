// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Configuration relating to the use of migrations for a given model.
    /// </summary>
    /// <typeparam name="TContext"> The context representing the model that this configuration applies to. </typeparam>
    public class DbMigrationsConfiguration<TContext> : DbMigrationsConfiguration
        where TContext : DbContext
    {
        static DbMigrationsConfiguration()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        /// Initializes a new instance of the DbMigrationsConfiguration class.
        /// </summary>
        public DbMigrationsConfiguration()
        {
            ContextType = typeof(TContext);
            MigrationsAssembly = GetType().Assembly();
            MigrationsNamespace = GetType().Namespace;
        }

        /// <summary>
        /// Runs after upgrading to the latest migration to allow seed data to be updated.
        /// </summary>
        /// <remarks>
        /// Note that the database may already contain seed data when this method runs. This means that
        /// implementations of this method must check whether or not seed data is present and/or up-to-date
        /// and then only make changes if necessary and in a non-destructive way. The 
        /// <see cref="DbSetMigrationsExtensions.AddOrUpdate{TEntity}(System.Data.Entity.IDbSet{TEntity},TEntity[])"/>
        /// can be used to help with this, but for seeding large amounts of data it may be necessary to do less
        /// granular checks if performance is an issue.
        /// If the <see cref="MigrateDatabaseToLatestVersion{TContext,TMigrationsConfiguration}"/> database 
        /// initializer is being used, then this method will be called each time that the initializer runs.
        /// If one of the <see cref="DropCreateDatabaseAlways{TContext}"/>, <see cref="DropCreateDatabaseIfModelChanges{TContext}"/>,
        /// or <see cref="CreateDatabaseIfNotExists{TContext}"/> initializers is being used, then this method will not be
        /// called and the Seed method defined in the initializer should be used instead.
        /// </remarks>
        /// <param name="context"> Context to be used for updating seed data. </param>
        protected virtual void Seed(TContext context)
        {
            Check.NotNull(context, "context");
        }

        internal override void OnSeed(DbContext context)
        {
            Seed((TContext)context);
        }

        #region Hide object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected new object MemberwiseClone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}
