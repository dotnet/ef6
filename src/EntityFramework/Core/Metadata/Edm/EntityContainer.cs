// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Class for representing an entity container
    /// </summary>
    public class EntityContainer : GlobalItem
    {
        private string _name;
        private readonly ReadOnlyMetadataCollection<EntitySetBase> _baseEntitySets;
        private readonly ReadOnlyMetadataCollection<EdmFunction> _functionImports;

        internal EntityContainer()
        {
        }

        /// <summary>
        ///     The constructor for constructing the EntityContainer object with the name, namespaceName, and version.
        /// </summary>
        /// <param name="name"> The name of this entity container </param>
        /// <param name="dataSpace"> dataSpace in which this entity container belongs to </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the name argument is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the name argument is empty string</exception>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal EntityContainer(string name, DataSpace dataSpace)
        {
            EntityUtil.CheckStringArgument(name, "name");

            _name = name;
            DataSpace = dataSpace;
            _baseEntitySets = new ReadOnlyMetadataCollection<EntitySetBase>(new EntitySetBaseCollection(this));
            _functionImports = new ReadOnlyMetadataCollection<EdmFunction>(new MetadataCollection<EdmFunction>());
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EntityContainer; }
        }

        /// <summary>
        ///     Gets the identity for this item as a string
        /// </summary>
        internal override string Identity
        {
            get { return Name; }
        }

        /// <summary>
        ///     Get the name of this EntityContainer object
        /// </summary>
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
        ///     Gets the collection of entity sets
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntitySetBase, true)]
        public ReadOnlyMetadataCollection<EntitySetBase> BaseEntitySets
        {
            get { return _baseEntitySets; }
        }

        public ReadOnlyMetadataCollection<AssociationSet> AssociationSets
        {
            get
            {
                return new FilteredReadOnlyMetadataCollection<AssociationSet, EntitySetBase>(
                    _baseEntitySets, Helper.IsAssociationSet);
            }
        }

        public ReadOnlyMetadataCollection<EntitySet> EntitySets
        {
            get
            {
                return new FilteredReadOnlyMetadataCollection<EntitySet, EntitySetBase>(
                    _baseEntitySets, Helper.IsEntitySet);
            }
        }

        /// <summary>
        ///     Gets the collection of function imports for this entity container
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmFunction, true)]
        public ReadOnlyMetadataCollection<EdmFunction> FunctionImports
        {
            get { return _functionImports; }
        }

        /// <summary>
        ///     Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
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
        ///     Get the entity set with the given name
        /// </summary>
        /// <param name="name"> name of the entity set to look up for </param>
        /// <param name="ignoreCase"> true if you want to do a case-insensitive lookup </param>
        /// <returns> </returns>
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
        ///     Get the entity set with the given name or return null if not found
        /// </summary>
        /// <param name="name"> name of the entity set to look up for </param>
        /// <param name="ignoreCase"> true if you want to do a case-insensitive lookup </param>
        /// <param name="entitySet"> out parameter that will contain the result </param>
        /// <returns> </returns>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
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
        ///     Get the relationship set with the given name
        /// </summary>
        /// <param name="name"> name of the relationship set to look up for </param>
        /// <param name="ignoreCase"> true if you want to do a case-insensitive lookup </param>
        /// <returns> </returns>
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
        ///     Get the relationship set with the given name
        /// </summary>
        /// <param name="name"> name of the relationship set to look up for </param>
        /// <param name="ignoreCase"> true if you want to do a case-insensitive lookup </param>
        /// <param name="relationshipSet"> out parameter that will have the result </param>
        /// <returns> </returns>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
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
        ///     Overriding System.Object.ToString to provide better String representation
        ///     for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        internal void AddEntitySetBase(EntitySetBase entitySetBase)
        {
            _baseEntitySets.Source.Add(entitySetBase);
        }

        public void RemoveEntitySetBase(EntitySetBase entitySetBase)
        {
            Check.NotNull(entitySetBase, "entitySetBase");
            Util.ThrowIfReadOnly(this);

            _baseEntitySets.Source.Remove(entitySetBase);
            entitySetBase.ChangeEntityContainerWithoutCollectionFixup(null);
        }

        internal void AddFunctionImport(EdmFunction function)
        {
            Debug.Assert(function != null, "function != null");
            Debug.Assert(function.IsFunctionImport, "function.IsFunctionImport");
            _functionImports.Source.Add(function);
        }
    }
}
