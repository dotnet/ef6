// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // An instance of this internal class is created whenever an instance of the public <see cref="DbSet{TEntity}" />
    // class is needed. This allows the public surface to be non-generic, while the runtime type created
    // still implements <see cref="IQueryable{T}" />.
    // </summary>
    // <typeparam name="TEntity"> The type of the entity. </typeparam>
    internal class InternalDbSet<TEntity> : DbSet, IQueryable<TEntity>
#if !NET40
, IDbAsyncEnumerable<TEntity>
#endif
 where TEntity : class
    {
        #region Fields and constructors

        private readonly IInternalSet<TEntity> _internalSet;

        // <summary>
        // Creates a new set that will be backed by the given internal set.
        // </summary>
        // <param name="internalSet"> The internal set. </param>
        public InternalDbSet(IInternalSet<TEntity> internalSet)
        {
            DebugCheck.NotNull(internalSet);

            _internalSet = internalSet;
        }

        // <summary>
        // Creates an instance of this class.  This method is used with CreateDelegate to cache a delegate
        // that can create a generic instance without calling MakeGenericType every time.
        // </summary>
        // <param name="internalSet"> The internal set to wrap, or null if a new internal set should be created. </param>
        // <returns> The set. </returns>
        public static InternalDbSet<TEntity> Create(InternalContext internalContext, IInternalSet internalSet)
        {
            return
                new InternalDbSet<TEntity>(
                    (IInternalSet<TEntity>)internalSet ?? new InternalSet<TEntity>(internalContext));
        }

        #endregion

        #region Implementation of abstract methods defined on DbSet and DbQuery

        // <inheritdoc />
        internal override IInternalQuery InternalQuery
        {
            get { return _internalSet; }
        }

        // <inheritdoc />
        internal override IInternalSet InternalSet
        {
            get { return _internalSet; }
        }

        // <inheritdoc />
        public override DbQuery Include(string path)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on DbSet.
            Check.NotEmpty(path, "path");

            return new InternalDbQuery<TEntity>(_internalSet.Include(path));
        }

        // <inheritdoc />
        public override DbQuery AsNoTracking()
        {
            return new InternalDbQuery<TEntity>(_internalSet.AsNoTracking());
        }

        // <inheritdoc />
        [Obsolete("Queries are now streaming by default unless a retrying ExecutionStrategy is used. Calling this method will have no effect.")]
        public override DbQuery AsStreaming()
        {
            return new InternalDbQuery<TEntity>(_internalSet.AsStreaming());
        }

        internal override DbQuery WithExecutionStrategy(IDbExecutionStrategy executionStrategy)
        {
            return new InternalDbQuery<TEntity>(_internalSet.WithExecutionStrategy(executionStrategy));
        }

        // <inheritdoc />
        public override object Find(params object[] keyValues)
        {
            return _internalSet.Find(keyValues);
        }

        internal override IInternalQuery GetInternalQueryWithCheck(string memberName)
        {
            return _internalSet;
        }

        internal override IInternalSet GetInternalSetWithCheck(string memberName)
        {
            return _internalSet;
        }

#if !NET40

        // <inheritdoc />
        public override async Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _internalSet.FindAsync(cancellationToken, keyValues).WithCurrentCulture();
        }

#endif

        // <inheritdoc />
        public override IList Local
        {
            get { return _internalSet.Local; }
        }

        // <inheritdoc />
        public override object Create()
        {
            return _internalSet.Create();
        }

        // <inheritdoc />
        public override object Create(Type derivedEntityType)
        {
            Check.NotNull(derivedEntityType, "derivedEntityType");

            return _internalSet.Create(derivedEntityType);
        }

        #endregion

        #region GetEnumerator

        // <summary>
        // Returns an <see cref="IEnumerator{TEntity}" /> which when enumerated will execute the backing query against the database.
        // </summary>
        // <returns> The query results. </returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _internalSet.GetEnumerator();
        }

        #endregion

        #region IDbAsyncEnumerable

#if !NET40

        // <summary>
        // Returns an <see cref="IDbAsyncEnumerator{TEntity}" /> which when enumerated will execute the backing query against the database.
        // </summary>
        // <returns> The query results. </returns>
        public IDbAsyncEnumerator<TEntity> GetAsyncEnumerator()
        {
            return _internalSet.GetAsyncEnumerator();
        }

#endif

        #endregion
    }
}
