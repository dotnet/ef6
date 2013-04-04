// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     An <see cref="EagerInternalContext" /> is an <see cref="InternalContext" /> where the <see cref="ObjectContext" />
    ///     instance that it wraps is set immediately at construction time rather than being created lazily. In this case
    ///     the internal context may or may not own the <see cref="ObjectContext" /> instance but will only dispose it
    ///     if it does own it.
    /// </summary>
    internal class EagerInternalContext : InternalContext
    {
        #region Fields and constructors

        // The underlying ObjectContext.
        private readonly ObjectContext _objectContext;
        private readonly bool _objectContextOwned;
        private readonly string _originalConnectionString;

        /// <summary>
        ///     For mocking.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public EagerInternalContext(DbContext owner)
            : base(owner, null)
        {
        }

        /// <summary>
        ///     Constructs an <see cref="EagerInternalContext" /> for an already existing <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="owner">
        ///     The owner <see cref="DbContext" /> .
        /// </param>
        /// <param name="objectContext">
        ///     The existing <see cref="ObjectContext" /> .
        /// </param>
        public EagerInternalContext(
            DbContext owner,
            ObjectContext objectContext,
            bool objectContextOwned,
            Interception interception = null)
            : base(owner, interception)
        {
            DebugCheck.NotNull(objectContext);

            _objectContext = objectContext;
            _objectContextOwned = objectContextOwned;
            _originalConnectionString = InternalConnection.GetStoreConnectionString(_objectContext.Connection);

            InitializeEntitySetMappings();
        }

        #endregion

        #region ObjectContext and model

        /// <summary>
        ///     Returns the underlying <see cref="ObjectContext" />.
        /// </summary>
        public override ObjectContext ObjectContext
        {
            get
            {
                Initialize();
                return ObjectContextInUse;
            }
        }

        /// <summary>
        ///     Returns the underlying <see cref="ObjectContext" /> without causing the underlying database to be created
        ///     or the database initialization strategy to be executed.
        ///     This is used to get a context that can then be used for database creation/initialization.
        /// </summary>
        public override ObjectContext GetObjectContextWithoutDatabaseInitialization()
        {
            InitializeContext();
            return ObjectContextInUse;
        }

        /// <summary>
        ///     The <see cref="ObjectContext" /> actually being used, which may be the
        ///     temp context for initialization or the real context.
        /// </summary>
        private ObjectContext ObjectContextInUse
        {
            get { return TempObjectContext ?? _objectContext; }
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     Does nothing, since the <see cref="ObjectContext" /> already exists.
        /// </summary>
        protected override void InitializeContext()
        {
            CheckContextNotDisposed();
        }

        /// <summary>
        ///     Does nothing since the database is always considered initialized if the <see cref="DbContext" /> was created
        ///     from an existing <see cref="ObjectContext" />.
        /// </summary>
        public override void MarkDatabaseNotInitialized()
        {
        }

        /// <summary>
        ///     Does nothing since the database is always considered initialized if the <see cref="DbContext" /> was created
        ///     from an existing <see cref="ObjectContext" />.
        /// </summary>
        public override void MarkDatabaseInitialized()
        {
        }

        /// <summary>
        ///     Does nothing since the database is always considered initialized if the <see cref="DbContext" /> was created
        ///     from an existing <see cref="ObjectContext" />.
        /// </summary>
        protected override void InitializeDatabase()
        {
        }

        /// <summary>
        ///     Gets the default database initializer to use for this context if no other has been registered.
        ///     For code first this property returns a <see cref="CreateDatabaseIfNotExists{TContext}" /> instance.
        ///     For database/model first, this property returns null.
        /// </summary>
        /// <value> The default initializer. </value>
        public override IDatabaseInitializer<DbContext> DefaultInitializer
        {
            get { return null; }
        }

        #endregion

        #region Dispose

        /// <summary>
        ///     Disposes the context. The underlying <see cref="ObjectContext" /> is also disposed if it is owned.
        /// </summary>
        public override void DisposeContext()
        {
            base.DisposeContext();

            if (_objectContextOwned && !IsDisposed)
            {
                _objectContext.Dispose();
            }
        }

        #endregion

        #region Connection access

        /// <summary>
        ///     The connection underlying this context.
        /// </summary>
        public override DbConnection Connection
        {
            get
            {
                CheckContextNotDisposed();
                return ((EntityConnection)_objectContext.Connection).StoreConnection;
            }
        }

        /// <summary>
        ///     The connection string as originally applied to the context. This is used to perform operations
        ///     that need the connection string in a non-mutated form, such as with security info still intact.
        /// </summary>
        public override string OriginalConnectionString
        {
            get { return _originalConnectionString; }
        }

        /// <summary>
        ///     Returns the origin of the underlying connection string.
        /// </summary>
        public override DbConnectionStringOrigin ConnectionStringOrigin
        {
            get { return DbConnectionStringOrigin.UserCode; }
        }

        /// <inheritdoc />
        public override void OverrideConnection(IInternalConnection connection)
        {
            DebugCheck.NotNull(connection);

            throw Error.EagerInternalContext_CannotSetConnectionInfo();
        }

        #endregion

        #region Lazy Loading

        /// <summary>
        ///     Gets or sets a value indicating whether lazy loading is enabled.  This is just a wrapper
        ///     over the same flag in the underlying <see cref="ObjectContext" />.
        /// </summary>
        public override bool LazyLoadingEnabled
        {
            get { return ObjectContextInUse.ContextOptions.LazyLoadingEnabled; }
            set { ObjectContextInUse.ContextOptions.LazyLoadingEnabled = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether proxy creation is enabled.  This is just a wrapper
        ///     over the same flag in the underlying ObjectContext.
        /// </summary>
        public override bool ProxyCreationEnabled
        {
            get { return ObjectContextInUse.ContextOptions.ProxyCreationEnabled; }
            set { ObjectContextInUse.ContextOptions.ProxyCreationEnabled = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether C# null comparison behavior is enabled.  This is just a wrapper
        ///     over the same flag in the underlying ObjectContext.
        /// </summary>
        public override bool UseDatabaseNullSemantics
        {
            get { return !ObjectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior; }
            set { ObjectContextInUse.ContextOptions.UseCSharpNullComparisonBehavior = !value; }
        }

        public override int? CommandTimeout
        {
            get { return ObjectContextInUse.CommandTimeout; }
            set { ObjectContextInUse.CommandTimeout = value; }
        }

        #endregion
    }
}
