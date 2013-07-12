// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;
    using System.Threading.Tasks;

    /// <summary>
    ///     Represents contextual information associated with calls into <see cref="IDbCommandInterceptor" />
    ///     implementations.
    /// </summary>
    /// <remarks>
    ///     An instance of this class is passed to the dispatch methods of <see cref="DbCommandDispatcher"/>
    ///     and does not contain mutable information such as the result of the operation. This mutable information
    ///     is obtained from the <see cref="DbCommandInterceptionContext{TResult}"/> that is passed to the interceptors.
    ///     Instances of this class are publicly immutable. To add contextual information use one of the
    ///     With... or As... methods to create a new interception context containing the new information.
    /// </remarks>
    public class DbCommandBaseInterceptionContext : DbInterceptionContext
    {
        private CommandBehavior _commandBehavior = CommandBehavior.Default;

        /// <summary>
        ///     Constructs a new <see cref="DbCommandBaseInterceptionContext" /> with no state.
        /// </summary>
        public DbCommandBaseInterceptionContext()
        {
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandBaseInterceptionContext" /> by copying state from the given
        ///     interception context. Also see <see cref="DbInterceptionContext.Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbCommandBaseInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            var asThisType = copyFrom as DbCommandBaseInterceptionContext;
            if (asThisType != null)
            {
                _commandBehavior = asThisType._commandBehavior;
            }
        }

        /// <summary>
        ///     The <see cref="CommandBehavior" /> that will be used or has been used to execute the command with a
        ///     <see cref="DbDataReader" />. This property is only used for <see cref="DbCommand.ExecuteReader(CommandBehavior)" />
        ///     and its async counterparts.
        /// </summary>
        public CommandBehavior CommandBehavior
        {
            get { return _commandBehavior; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandBaseInterceptionContext" /> that contains all the contextual information in this
        ///     interception context together with the given <see cref="CommandBehavior" />.
        /// </summary>
        /// <param name="commandBehavior">The command behavior to associate.</param>
        /// <returns>A new interception context associated with the given command behavior.</returns>
        public DbCommandBaseInterceptionContext WithCommandBehavior(CommandBehavior commandBehavior)
        {
            var copy = TypedClone();
            copy._commandBehavior = commandBehavior;
            return copy;
        }

        private DbCommandBaseInterceptionContext TypedClone()
        {
            return (DbCommandBaseInterceptionContext)Clone();
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbCommandBaseInterceptionContext(this);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandBaseInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandBaseInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandBaseInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandBaseInterceptionContext" /> that contains all the contextual information in this
        ///     interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbCommandBaseInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbCommandBaseInterceptionContext)base.WithObjectContext(context);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCommandBaseInterceptionContext" /> that contains all the contextual information in this
        ///     interception context the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbCommandBaseInterceptionContext AsAsync()
        {
            return (DbCommandBaseInterceptionContext)base.AsAsync();
        }
    }
}
