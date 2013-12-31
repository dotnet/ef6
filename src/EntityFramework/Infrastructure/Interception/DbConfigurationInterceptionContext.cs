// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents contextual information associated with calls into <see cref="IDbConfigurationInterceptor" />
    /// implementations.
    /// </summary>
    /// <remarks>
    /// Instances of this class are publicly immutable for contextual information. To add
    /// contextual information use one of the With... or As... methods to create a new
    /// interception context containing the new information.
    /// </remarks>
    public class DbConfigurationInterceptionContext :
        DbInterceptionContext
    {
        /// <summary>
        /// Constructs a new <see cref="DbConfigurationInterceptionContext" /> with no state.
        /// </summary>
        public DbConfigurationInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DbConfigurationInterceptionContext" /> by copying state from the given
        /// interception context. Also see <see cref="DbConfigurationInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbConfigurationInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbConfigurationInterceptionContext(this);
        }

        /// <summary>
        /// Creates a new <see cref="DbConfigurationInterceptionContext" /> that contains all the contextual information in
        /// this interception context with the addition of the given <see cref="DbContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConfigurationInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbConfigurationInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="DbConfigurationInterceptionContext" /> that contains all the contextual information in
        /// this interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConfigurationInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbConfigurationInterceptionContext)base.WithObjectContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="DbConfigurationInterceptionContext" /> that contains all the contextual information in
        /// this interception context the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbConfigurationInterceptionContext AsAsync()
        {
            return (DbConfigurationInterceptionContext)base.AsAsync();
        }

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
    }
}
