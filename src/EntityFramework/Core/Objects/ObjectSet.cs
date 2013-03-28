// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ObjectSet<TEntity> : ObjectQuery<TEntity>, IObjectSet<TEntity>
        where TEntity : class
    {
        private readonly EntitySet _entitySet;

        #region Internal Constructors

        /// <summary>
        ///     Creates a new ObjectSet that has a base ObjectQuery with the CommandText that represents
        ///     all of the entities in the specified EntitySet.
        ///     Sets the query's command text to the fully-qualified, quoted, EntitySet name, i.e. [EntityContainerName].[EntitySetName]
        ///     Explicitly set MergeOption to AppendOnly in order to mirror CreateQuery behavior
        /// </summary>
        /// <param name="entitySet"> Metadata EntitySet on which to base the ObjectSet. </param>
        /// <param name="context"> ObjectContext to be used for the query and data modification operations. </param>
        internal ObjectSet(EntitySet entitySet, ObjectContext context)
            : base(entitySet, context, MergeOption.AppendOnly)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(context);
            _entitySet = entitySet;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the metadata of the entity set represented by this <see cref="T:System.Data.Entity.Core.Objects.ObjectSet`1" /> instance.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object.
        /// </returns>
        public EntitySet EntitySet
        {
            get { return _entitySet; }
        }

        #endregion

        #region Public Methods

        /// <summary>Adds an object to the object context in the current entity set. </summary>
        /// <param name="entity">The object to add.</param>
        public void AddObject(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.AddObject -- see devnote at the top of this class
            Context.AddObject(FullyQualifiedEntitySetName, entity);
        }

        /// <summary>Attaches an object or object graph to the object context in the current entity set. </summary>
        /// <param name="entity">The object to attach.</param>
        public void Attach(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.AttachTo -- see devnote at the top of this class
            Context.AttachTo(FullyQualifiedEntitySetName, entity);
        }

        /// <summary>Marks an object for deletion. </summary>
        /// <param name="entity">
        ///     An object that represents the entity to delete. The object can be in any state except
        ///     <see
        ///         cref="F:System.Data.Entity.EntityState.Detached" />
        ///     .
        /// </param>
        public void DeleteObject(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.DeleteObject -- see devnote at the top of this class
            // Note that in this case we use an internal DeleteObject overload so we can have the context validate
            // the EntitySet after it verifies that the specified object is in the context at all.
            Context.DeleteObject(entity, EntitySet);
        }

        /// <summary>Removes the object from the object context.</summary>
        /// <param name="entity">
        ///     Object to be detached. Only the  entity  is removed; if there are any related objects that are being tracked by the same
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.ObjectStateManager" />
        ///     , those will not be detached automatically.
        /// </param>
        public void Detach(TEntity entity)
        {
            // this method is expected to behave exactly like ObjectContext.Detach -- see devnote at the top of this class
            // Note that in this case we use an internal Detach overload so we can have the context validate
            // the EntitySet after it verifies that the specified object is in the context at all.
            Context.Detach(entity, EntitySet);
        }

        /// <summary>
        ///     Copies the scalar values from the supplied object into the object in the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        ///     that has the same key.
        /// </summary>
        /// <returns>The updated object.</returns>
        /// <param name="currentEntity">
        ///     The detached object that has property updates to apply to the original object. The entity key of  currentEntity  must match the
        ///     <see
        ///         cref="P:System.Data.Entity.Core.Objects.ObjectStateEntry.EntityKey" />
        ///     property of an entry in the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        ///     .
        /// </param>
        public TEntity ApplyCurrentValues(TEntity currentEntity)
        {
            // this method is expected to behave exactly like ObjectContext.ApplyCurrentValues -- see devnote at the top of this class
            return Context.ApplyCurrentValues(FullyQualifiedEntitySetName, currentEntity);
        }

        /// <summary>
        ///     Sets the <see cref="P:System.Data.Entity.Core.Objects.ObjectStateEntry.OriginalValues" /> property of an
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.ObjectStateEntry" />
        ///     to match the property values of a supplied object.
        /// </summary>
        /// <returns>The updated object.</returns>
        /// <param name="originalEntity">
        ///     The detached object that has property updates to apply to the original object. The entity key of  originalEntity  must match the
        ///     <see
        ///         cref="P:System.Data.Entity.Core.Objects.ObjectStateEntry.EntityKey" />
        ///     property of an entry in the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.ObjectContext" />
        ///     .
        /// </param>
        public TEntity ApplyOriginalValues(TEntity originalEntity)
        {
            // this method is expected to behave exactly like ObjectContext.ApplyOriginalValues -- see devnote at the top of this class
            return Context.ApplyOriginalValues(FullyQualifiedEntitySetName, originalEntity);
        }

        /// <summary>Creates a new entity type object.</summary>
        /// <returns>The new entity type object, or an instance of a proxy type that corresponds to the entity type.</returns>
        public TEntity CreateObject()
        {
            return Context.CreateObject<TEntity>();
        }

        /// <summary>Creates an instance of the specified type.</summary>
        /// <returns>An instance of the requested type  T , or an instance of a proxy type that corresponds to the type  T .</returns>
        /// <typeparam name="T">Type of object to be returned.</typeparam>
        public T CreateObject<T>() where T : class, TEntity
        {
            return Context.CreateObject<T>();
        }

        #endregion

        #region Private Properties

        // Used
        private string FullyQualifiedEntitySetName
        {
            get
            {
                // Fully-qualified name is used to ensure the ObjectContext can always resolve the EntitySet name
                // The identifiers used here should not be escaped with brackets ("[]") because the ObjectContext does not allow escaping for the EntitySet name
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", _entitySet.EntityContainer.Name, _entitySet.Name);
            }
        }

        #endregion
    }
}
