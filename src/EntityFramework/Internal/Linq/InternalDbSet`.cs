namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     An instance of this internal class is created whenever an instance of the public <see cref = "DbSet{TEntity}" />
    ///     class is needed. This allows the public surface to be non-generic, while the runtime type created
    ///     still implements <see cref = "IQueryable{T}" />.
    /// </summary>
    /// <typeparam name = "TEntity">The type of the entity.</typeparam>
    internal class InternalDbSet<TEntity> : DbSet, IQueryable<TEntity>
        where TEntity : class
    {
        #region Fields and constructors

        private readonly IInternalSet<TEntity> _internalSet;

        /// <summary>
        ///     Creates a new set that will be backed by the given internal set.
        /// </summary>
        /// <param name = "internalSet">The internal set.</param>
        public InternalDbSet(IInternalSet<TEntity> internalSet)
        {
            Contract.Requires(internalSet != null);

            _internalSet = internalSet;
        }

        /// <summary>
        ///     Creates an instance of this class.  This method is used with CreateDelegate to cache a delegate
        ///     that can create a generic instance without calling MakeGenericType every time.
        /// </summary>
        /// <param name = "internalContext"></param>
        /// <param name = "internalSet">The internal set to wrap, or null if a new internal set should be created.</param>
        /// <returns>The set.</returns>
        public static InternalDbSet<TEntity> Create(InternalContext internalContext, IInternalSet internalSet)
        {
            return new InternalDbSet<TEntity>((IInternalSet<TEntity>)internalSet ?? new InternalSet<TEntity>(internalContext));
        }

        #endregion

        #region Implementation of abstract methods defined on DbSet and DbQuery

        /// <summary>
        ///     Gets the underlying internal query object.
        /// </summary>
        /// <value>The internal query.</value>
        internal override IInternalQuery InternalQuery
        {
            get { return _internalSet; }
        }

        /// <summary>
        ///     Gets the underlying internal set.
        /// </summary>
        /// <value>The internal set.</value>
        internal override IInternalSet InternalSet
        {
            get { return _internalSet; }
        }

        /// <summary>
        ///     See comments in <see cref = "DbQuery" />.
        /// </summary>
        public override DbQuery Include(string path)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on DbSet.
            DbHelpers.ThrowIfNullOrWhitespace(path, "path");

            return new InternalDbQuery<TEntity>(_internalSet.Include(path));
        }

        /// <summary>
        ///     See comments in <see cref = "DbQuery" />.
        /// </summary>
        public override DbQuery AsNoTracking()
        {
            return new InternalDbQuery<TEntity>(_internalSet.AsNoTracking());
        }

        /// <summary>
        ///     See comments in <see cref = "DbSet{TEntity}" />.
        /// </summary>
        public override object Find(params object[] keyValues)
        {
            return _internalSet.Find(keyValues);
        }

        /// <summary>
        ///     See comments in <see cref = "DbSet{TEntity}" />.
        /// </summary>
        public override IList Local
        {
            get { return _internalSet.Local; }
        }

        /// <summary>
        ///     See comments in <see cref = "DbSet{TEntity}" />.
        /// </summary>
        public override object Create()
        {
            return _internalSet.Create();
        }

        /// <summary>
        ///     See comments in <see cref = "DbSet{TEntity}" />.
        /// </summary>
        public override object Create(Type derivedEntityType)
        {
            // We need this because the Code Contract gets compiled out in the release build even though
            // this method is effectively on the public surface because it overrides the abstract method on DbSet.
            DbHelpers.ThrowIfNull(derivedEntityType, "derivedEntityType");

            return _internalSet.Create(derivedEntityType);
        }

        #endregion

        #region GetEnumerator

        /// <summary>
        ///     Gets the enumeration of this query causing it to be executed against the store.
        /// </summary>
        /// <returns>An enumerator for the query</returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _internalSet.GetEnumerator();
        }

        #endregion
    }
}