// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Class for representing an entity container
    /// </summary>
    public class EntityContainer : GlobalItem
    {
        private string _name;
        private readonly ReadOnlyMetadataCollection<EntitySetBase> _baseEntitySets;
        private readonly ReadOnlyMetadataCollection<EdmFunction> _functionImports;

        internal EntityContainer()
        {
            // mocking only
        }

        /// <summary>
        /// Creates an entity container with the specified name and data space.
        /// </summary>
        /// <param name="name">The entity container name.</param>
        /// <param name="dataSpace">The entity container data space.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the name argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the name argument is empty string.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityContainer(string name, DataSpace dataSpace)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            DataSpace = dataSpace;
            _baseEntitySets = new ReadOnlyMetadataCollection<EntitySetBase>(new EntitySetBaseCollection(this));
            _functionImports = new ReadOnlyMetadataCollection<EdmFunction>(new MetadataCollection<EdmFunction>());
        }

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntityContainer; }
        }

        // <summary>
        // Gets the identity for this item as a string
        // </summary>
        internal override string Identity
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets the name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </summary>
        /// <returns>
        /// The name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </returns>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public virtual String Name
        {
            get { return _name; }
            set
            {
                Check.NotEmpty(value, "value");
                Util.ThrowIfReadOnly(this);

                _name = value;
            }
        }

        /// <summary>
        /// Gets a list of entity sets and association sets that this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />
        /// includes.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> object that contains a list of entity sets and association sets that this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />
        /// includes.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EntitySetBase, true)]
        public ReadOnlyMetadataCollection<EntitySetBase> BaseEntitySets
        {
            get { return _baseEntitySets; }
        }

        private readonly object _baseEntitySetsLock = new object();
        private ReadOnlyMetadataCollection<AssociationSet> _associationSetsCache;

        /// <summary> Gets the association sets for this entity container. </summary>
        /// <returns> The association sets for this entity container .</returns>
        public ReadOnlyMetadataCollection<AssociationSet> AssociationSets
        {
            get
            {
                // PERF: this code written this way since it's part of a hotpath, consider its performance when refactoring
                var assiationSets = _associationSetsCache;
                if (assiationSets == null)
                {
                    lock (_baseEntitySetsLock)
                    {
                        if (_associationSetsCache == null)
                        {
                            _baseEntitySets.SourceAccessed += ResetAssociationSetsCache;
                            _associationSetsCache = new FilteredReadOnlyMetadataCollection<AssociationSet, EntitySetBase>(
                                _baseEntitySets, Helper.IsAssociationSet);
                        }
                        assiationSets = _associationSetsCache;
                    }
                }
                return assiationSets;
            }
        }

        private void ResetAssociationSetsCache(object sender, EventArgs e)
        {
            if (_associationSetsCache != null)
            {
                lock (_baseEntitySetsLock)
                {
                    if (_associationSetsCache != null)
                    {
                        _associationSetsCache = null;
                        _baseEntitySets.SourceAccessed -= ResetAssociationSetsCache;
                    }
                }
            }
        }

        private ReadOnlyMetadataCollection<EntitySet> _entitySetsCache;

        /// <summary> Gets the entity sets for this entity container. </summary>
        /// <returns> The entity sets for this entity container .</returns>
        public ReadOnlyMetadataCollection<EntitySet> EntitySets
        {
            get
            {
                // PERF: this code written this way since it's part of a hotpath, consider its performance when refactoring
                var entitySets = _entitySetsCache;
                if (entitySets == null)
                {
                    lock (_baseEntitySetsLock)
                    {
                        if (_entitySetsCache == null)
                        {
                            _baseEntitySets.SourceAccessed += ResetEntitySetsCache;
                            _entitySetsCache = new FilteredReadOnlyMetadataCollection<EntitySet, EntitySetBase>(
                                _baseEntitySets, Helper.IsEntitySet);
                        }
                        entitySets = _entitySetsCache;
                    }
                }
                return entitySets;
            }
        }

        private void ResetEntitySetsCache(object sender, EventArgs e)
        {
            if (_entitySetsCache != null)
            {
                lock (_baseEntitySetsLock)
                {
                    if (_entitySetsCache != null)
                    {
                        _entitySetsCache = null;
                        _baseEntitySets.SourceAccessed -= ResetEntitySetsCache;
                    }
                }
            }
        }

        /// <summary>
        /// Specifies a collection of <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmFunction" /> elements. Each function contains the details of a stored procedure that exists in the database or equivalent CommandText that is mapped to an entity and its properties.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmFunction" />
        /// elements.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EdmFunction, true)]
        public ReadOnlyMetadataCollection<EdmFunction> FunctionImports
        {
            get { return _functionImports; }
        }

        // <summary>
        // Sets this item to be readonly, once this is set, the item will never be writable again.
        // </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                BaseEntitySets.Source.SetReadOnly();
                FunctionImports.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object by using the specified name for the entity set.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object that represents the entity set that has the specified name.
        /// </returns>
        /// <param name="name">The name of the entity set that is searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        public EntitySet GetEntitySetByName(string name, bool ignoreCase)
        {
            var entitySet = (BaseEntitySets.GetValue(name, ignoreCase) as EntitySet);
            if (null != entitySet)
            {
                return entitySet;
            }
            throw new ArgumentException(Strings.InvalidEntitySetName(name));
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object by using the specified name for the entity set.
        /// </summary>
        /// <returns>true if there is an entity set that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the entity set that is searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="entitySet">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object. If there is no entity set, this output parameter contains null.
        /// </param>
        public bool TryGetEntitySetByName(string name, bool ignoreCase, out EntitySet entitySet)
        {
            Check.NotNull(name, "name");
            EntitySetBase baseEntitySet = null;
            entitySet = null;
            if (BaseEntitySets.TryGetValue(name, ignoreCase, out baseEntitySet))
            {
                if (Helper.IsEntitySet(baseEntitySet))
                {
                    entitySet = (EntitySet)baseEntitySet;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" /> object by using the specified name for the relationship set.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" /> object that represents the relationship set that has the specified name.
        /// </returns>
        /// <param name="name">The name of the relationship set that is searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        public RelationshipSet GetRelationshipSetByName(string name, bool ignoreCase)
        {
            RelationshipSet relationshipSet;
            if (!TryGetRelationshipSetByName(name, ignoreCase, out relationshipSet))
            {
                throw new ArgumentException(Strings.InvalidRelationshipSetName(name));
            }
            return relationshipSet;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" /> object by using the specified name for the relationship set.
        /// </summary>
        /// <returns>true if there is a relationship set that matches the search criteria; otherwise, false. </returns>
        /// <param name="name">The name of the relationship set that is searched for.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="relationshipSet">
        /// When this method returns, contains a <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipSet" /> object.
        /// </param>
        public bool TryGetRelationshipSetByName(string name, bool ignoreCase, out RelationshipSet relationshipSet)
        {
            Check.NotNull(name, "name");
            EntitySetBase baseEntitySet = null;
            relationshipSet = null;
            if (BaseEntitySets.TryGetValue(name, ignoreCase, out baseEntitySet))
            {
                if (Helper.IsRelationshipSet(baseEntitySet))
                {
                    relationshipSet = (RelationshipSet)baseEntitySet;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </summary>
        /// <returns>
        /// The name of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" />.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Adds the specified entity set to the container.
        /// </summary>
        /// <param name="entitySetBase">The entity set to add.</param>
        public void AddEntitySetBase(EntitySetBase entitySetBase)
        {
            Check.NotNull(entitySetBase, "entitySetBase");
            Util.ThrowIfReadOnly(this);

            _baseEntitySets.Source.Add(entitySetBase);
            entitySetBase.ChangeEntityContainerWithoutCollectionFixup(this);
        }

        /// <summary>Removes a specific entity set from the container.</summary>
        /// <param name="entitySetBase">The entity set to remove.</param>
        public void RemoveEntitySetBase(EntitySetBase entitySetBase)
        {
            Check.NotNull(entitySetBase, "entitySetBase");
            Util.ThrowIfReadOnly(this);

            _baseEntitySets.Source.Remove(entitySetBase);
            entitySetBase.ChangeEntityContainerWithoutCollectionFixup(null);
        }

        /// <summary>
        /// Adds a function import to the container.
        /// </summary>
        /// <param name="function">The function import to add.</param>
        public void AddFunctionImport(EdmFunction function)
        {
            Check.NotNull(function, "function");
            Util.ThrowIfReadOnly(this);
            if (!function.IsFunctionImport)
            {
                throw new ArgumentException(Strings.OnlyFunctionImportsCanBeAddedToEntityContainer(function.Name));
            }

            _functionImports.Source.Add(function);
        }

        /// <summary>
        /// The factory method for constructing the EntityContainer object.
        /// </summary>
        /// <param name="name">The name of the entity container to be created.</param>
        /// <param name="dataSpace">DataSpace in which this entity container belongs to.</param>
        /// <param name="entitySets">Entity sets that will be included in the new container. Can be null.</param>
        /// <param name="functionImports">Functions that will be included in the new container. Can be null.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The EntityContainer object.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the name argument is null or empty string.</exception>
        /// <remarks>The newly created EntityContainer will be read only.</remarks>
        public static EntityContainer Create(
            string name, DataSpace dataSpace, IEnumerable<EntitySetBase> entitySets,
            IEnumerable<EdmFunction> functionImports, IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            var entityContainer = new EntityContainer(name, dataSpace);

            if (entitySets != null)
            {
                foreach (var entitySet in entitySets)
                {
                    entityContainer.AddEntitySetBase(entitySet);
                }
            }

            if (functionImports != null)
            {
                foreach (var function in functionImports)
                {
                    if (!function.IsFunctionImport)
                    {
                        throw new ArgumentException(Strings.OnlyFunctionImportsCanBeAddedToEntityContainer(function.Name));
                    }
                    entityContainer.AddFunctionImport(function);
                }
            }

            if (metadataProperties != null)
            {
                entityContainer.AddMetadataProperties(metadataProperties.ToList());
            }

            entityContainer.SetReadOnly();

            return entityContainer;
        }

        internal virtual void NotifyItemIdentityChanged(EntitySetBase item, string initialIdentity)
        {
            _baseEntitySets.Source.HandleIdentityChange(item, initialIdentity);
        }
    }
}
