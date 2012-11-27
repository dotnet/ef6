// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     An implementation of IDatabaseInitializer that will always recreate and optionally re-seed the
    ///     database the first time that a context is used in the app domain.
    ///     To seed the database, create a derived class and override the Seed method.
    /// </summary>
    /// <typeparam name="TContext"> The type of the context. </typeparam>
    public class DropCreateDatabaseAlways<TContext> : IDatabaseInitializer<TContext>
        where TContext : DbContext
    {
        #region Strategy implementation

        static DropCreateDatabaseAlways()
        {
            DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        }

        /// <summary>
        ///     Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name="context"> The context. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="context" />
        ///     is
        ///     <c>null</c>
        ///     .
        /// </exception>
        public void InitializeDatabase(TContext context)
        {
            Check.NotNull(context, "context");

            context.Database.Delete();
            context.Database.Create();
            Seed(context);
            context.SaveChanges();
        }

        #endregion

        #region Seeding methods

        /// <summary>
        ///     A method that should be overridden to actually add data to the context for seeding.
        ///     The default implementation does nothing.
        /// </summary>
        /// <param name="context"> The context to seed. </param>
        protected virtual void Seed(TContext context)
        {
        }

        #endregion
    }
}
