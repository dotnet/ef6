// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Returned by the ChangeTracker method of <see cref="DbContext" /> to provide access to features of
    ///     the context that are related to change tracking of entities.
    /// </summary>
    public class DbChangeTracker
    {
        #region Construction and fields

        private readonly InternalContext _internalContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbChangeTracker" /> class.
        /// </summary>
        /// <param name="internalContext"> The internal context. </param>
        internal DbChangeTracker(InternalContext internalContext)
        {
            DebugCheck.NotNull(internalContext);

            _internalContext = internalContext;
        }

        #endregion

        #region Entity entries

        /// <summary>
        ///     Gets <see cref="DbEntityEntry" /> objects for all the entities tracked by this context.
        /// </summary>
        /// <returns> The entries. </returns>
        public IEnumerable<DbEntityEntry> Entries()
        {
            return
                _internalContext.GetStateEntries().Select(
                    e => new DbEntityEntry(new InternalEntityEntry(_internalContext, e)));
        }

        /// <summary>
        ///     Gets <see cref="DbEntityEntry" /> objects for all the entities of the given type
        ///     tracked by this context.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <returns> The entries. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<DbEntityEntry<TEntity>> Entries<TEntity>() where TEntity : class
        {
            return
                _internalContext.GetStateEntries<TEntity>().Select(
                    e => new DbEntityEntry<TEntity>(new InternalEntityEntry(_internalContext, e)));
        }

        #endregion

        #region DetectChanges

        /// <summary>
        ///     Check if the <see cref="DbContext" /> contain changes, added, modified or deleted entities and relationships of POCO entities. 
        ///     This method call first to detect changes on the underlying <see cref="DbContext" />.
        /// </summary>
        /// <returns>True if underlying <see cref="DbContext" /> have changes, else false.</returns>
        public bool HasChanges()
        {
            _internalContext.DetectChanges();

            var objectStateManager = _internalContext
               .ObjectContext.ObjectStateManager;

            DebugCheck.NotNull(objectStateManager);

            var entriesAffected =
              objectStateManager.GetObjectStateEntriesCount(EntityState.Added | EntityState.Deleted | EntityState.Modified);


            return entriesAffected > 0;
        }

        /// <summary>
        ///     Detects changes made to the properties and relationships of POCO entities.  Note that some types of
        ///     entity (such as change tracking proxies and entities that derive from
        ///     <see
        ///         cref="System.Data.Entity.Core.Objects.DataClasses.EntityObject" />
        ///     )
        ///     report changes automatically and a call to DetectChanges is not normally needed for these types of entities.
        ///     Also note that normally DetectChanges is called automatically by many of the methods of <see cref="DbContext" />
        ///     and its related classes such that it is rare that this method will need to be called explicitly.
        ///     However, it may be desirable, usually for performance reasons, to turn off this automatic calling of
        ///     DetectChanges using the AutoDetectChangesEnabled flag from <see cref="DbContext.Configuration" />.
        /// </summary>
        public void DetectChanges()
        {
            _internalContext.DetectChanges(force: true);
        }

        #endregion

        #region Hidden Object methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion
    }
}
