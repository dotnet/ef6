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
