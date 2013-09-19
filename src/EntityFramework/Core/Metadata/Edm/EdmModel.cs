// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a conceptual or store model. This class can be used to access information about the shape of the model 
    /// and the way the that it has been configured. 
    /// </summary>
    public class EdmModel : MetadataItem
    {
        private readonly List<AssociationType> _associationTypes = new List<AssociationType>();
        private readonly List<ComplexType> _complexTypes = new List<ComplexType>();
        private readonly List<EntityType> _entityTypes = new List<EntityType>();
        private readonly List<EnumType> _enumTypes = new List<EnumType>();
        private readonly List<EdmFunction> _functions = new List<EdmFunction>();
        private readonly EntityContainer _container;

        private double _schemaVersion;

        private DbProviderInfo _providerInfo;
        private DbProviderManifest _providerManifest;

        private EdmModel(EntityContainer entityContainer, double version = XmlConstants.SchemaVersionLatest)
        {
            DebugCheck.NotNull(entityContainer);

            _container = entityContainer;
            SchemaVersion = version;
        }

        internal EdmModel(DataSpace dataSpace, double schemaVersion = XmlConstants.SchemaVersionLatest)
        {
            if (dataSpace != DataSpace.CSpace && dataSpace != DataSpace.SSpace)
            {
                throw new ArgumentException(Strings.EdmModel_InvalidDataSpace(dataSpace), "dataSpace");
            }

            _container = new EntityContainer(
                dataSpace == DataSpace.CSpace
                    ? "CodeFirstContainer"
                    : "CodeFirstDatabase",
                dataSpace);

            _schemaVersion = schemaVersion;
        }

        /// <summary>Gets the built-in type kind for this type.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this type.
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.MetadataItem; }
        }

        internal override string Identity
        {
            get { return "EdmModel" + Container.Identity; }
        }

        /// <summary>
        /// Gets the data space associated with the model, which indicates whether 
        /// it is a conceptual model (DataSpace.CSpace) or a store model (DataSpace.SSpace).
        /// </summary>
        public DataSpace DataSpace
        {
            get { return Container.DataSpace; }
        }

        /// <summary>
        /// Gets the association types in the model.
        /// </summary>
        public IEnumerable<AssociationType> AssociationTypes
        {
            get { return _associationTypes; }
        }

        /// <summary>
        /// Gets the complex types in the model.
        /// </summary>
        public IEnumerable<ComplexType> ComplexTypes
        {
            get { return _complexTypes; }
        }

        /// <summary>
        /// Gets the entity types in the model.
        /// </summary>
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes; }
        }

        /// <summary>
        /// Gets the enum types in the model.
        /// </summary>
        public IEnumerable<EnumType> EnumTypes
        {
            get { return _enumTypes; }
        }

        /// <summary>
        /// Gets the functions in the model.
        /// </summary>
        public IEnumerable<EdmFunction> Functions
        {
            get { return _functions; }
        }

        /// <summary>
        /// Gets the container that stores entity and association sets, and function imports.
        /// </summary>
        public EntityContainer Container
        {
            get { return _container; }
        }

        /// <summary>Gets the version of the schema for the model.</summary>
        /// <returns>The version of the schema for the model.</returns>
        internal double SchemaVersion
        {
            get { return _schemaVersion; }
            set { _schemaVersion = value; }
        }

        /// <summary>Gets the provider information for this model.</summary>
        /// <returns>The provider information for this model.</returns>
        internal DbProviderInfo ProviderInfo
        {
            get { return _providerInfo; }
            private set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(DataSpace == DataSpace.SSpace);

                _providerInfo = value;
            }
        }

        /// <summary>Gets the provider manifest associated with the model.</summary>
        /// <returns>The provider manifest associated with the model.</returns>
        internal DbProviderManifest ProviderManifest
        {
            get { return _providerManifest; }
            private set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(DataSpace == DataSpace.SSpace);

                _providerManifest = value;
            }
        }

        /// <summary>Gets the namespace names associated with the model.</summary>
        /// <returns>The namespace names associated with the model.</returns>
        internal virtual IEnumerable<string> NamespaceNames
        {
            get
            {
                return NamespaceItems
                    .Select(t => t.NamespaceName)
                    .Distinct();
            }
        }

        /// <summary>Gets the namespace items associated with the model.</summary>
        /// <returns>The namespace items associated with the model.</returns>
        internal IEnumerable<EdmType> NamespaceItems
        {
            get
            {
                return _associationTypes
                    .Concat<EdmType>(_complexTypes)
                    .Concat(_entityTypes)
                    .Concat(_enumTypes)
                    .Concat(_functions);
            }
        }

        /// <summary>Gets the global items associated with the model.</summary>
        /// <returns>The global items associated with the model.</returns>
        public IEnumerable<GlobalItem> GlobalItems
        {
            get { return NamespaceItems.Concat<GlobalItem>(Containers); }
        }

        /// <summary>Gets the containers associated with the model.</summary>
        /// <returns>The containers associated with the model.</returns>
        internal virtual IEnumerable<EntityContainer> Containers
        {
            get { yield return Container; }
        }

        /// <summary>
        /// Adds an association type to the model.
        /// </summary>
        /// <param name="item">The AssociationType instance to be added.</param>
        public void AddItem(AssociationType item)
        {
            Check.NotNull(item, "item");
            ValidateSpace(item);

            _associationTypes.Add(item);
        }

        /// <summary>
        /// Adds a complex type to the model.
        /// </summary>
        /// <param name="item">The ComplexType instance to be added.</param>
        public void AddItem(ComplexType item)
        {
            Check.NotNull(item, "item");
            ValidateSpace(item);

            _complexTypes.Add(item);
        }

        /// <summary>
        /// Adds an entity type to the model.
        /// </summary>
        /// <param name="item">The EntityType instance to be added.</param>
        public void AddItem(EntityType item)
        {
            Check.NotNull(item, "item");
            ValidateSpace(item);

            _entityTypes.Add(item);
        }

        /// <summary>
        /// Adds an enumeration type to the model.
        /// </summary>
        /// <param name="item">The EnumType instance to be added.</param>
        public void AddItem(EnumType item)
        {
            Check.NotNull(item, "item");
            ValidateSpace(item);

            _enumTypes.Add(item);
        }

        /// <summary>
        /// Adds a function to the model.
        /// </summary>
        /// <param name="item">The EdmFunction instance to be added.</param>
        public void AddItem(EdmFunction item)
        {
            Check.NotNull(item, "item");
            ValidateSpace(item);

            _functions.Add(item);
        }

        /// <summary>
        /// Removes an association type from the model.
        /// </summary>
        /// <param name="item">The AssociationType instance to be removed.</param>
        public void RemoveItem(AssociationType item)
        {
            Check.NotNull(item, "item");

            _associationTypes.Remove(item);
        }

        /// <summary>
        /// Removes a complex type from the model.
        /// </summary>
        /// <param name="item">The ComplexType instance to be removed.</param>
        public void RemoveItem(ComplexType item)
        {
            Check.NotNull(item, "item");

            _complexTypes.Remove(item);
        }

        /// <summary>
        /// Removes an entity type from the model.
        /// </summary>
        /// <param name="item">The EntityType instance to be removed.</param>
        public void RemoveItem(EntityType item)
        {
            Check.NotNull(item, "item");

            _entityTypes.Remove(item);
        }

        /// <summary>
        /// Removes an enumeration type from the model.
        /// </summary>
        /// <param name="item">The EnumType instance to be removed.</param>
        public void RemoveItem(EnumType item)
        {
            Check.NotNull(item, "item");

            _enumTypes.Remove(item);
        }

        /// <summary>
        /// Removes a function from the model.
        /// </summary>
        /// <param name="item">The EdmFunction instance to be removed.</param>
        public void RemoveItem(EdmFunction item)
        {
            Check.NotNull(item, "item");

            _functions.Remove(item);
        }

        internal virtual void Validate()
        {
            var validationErrors = new List<DataModelErrorEventArgs>();

            var validator = new DataModelValidator();
            validator.OnError += (_, e) => validationErrors.Add(e);
            validator.Validate(this, true);

            if (validationErrors.Count > 0)
            {
                throw new ModelValidationException(validationErrors);
            }
        }

        private void ValidateSpace(EdmType item)
        {
            if (item.DataSpace != DataSpace)
            {
                throw new ArgumentException(Strings.EdmModel_AddItem_NonMatchingNamespace, "item");
            }
        }

        internal static EdmModel CreateStoreModel(
            DbProviderInfo providerInfo, DbProviderManifest providerManifest,
            double schemaVersion = XmlConstants.SchemaVersionLatest)
        {
            DebugCheck.NotNull(providerInfo);
            DebugCheck.NotNull(providerManifest);

            return
                new EdmModel(DataSpace.SSpace, schemaVersion)
                {
                    ProviderInfo = providerInfo,
                    ProviderManifest = providerManifest
                };
        }

        internal static EdmModel CreateStoreModel(
            EntityContainer entityContainer,
            DbProviderInfo providerInfo,
            DbProviderManifest providerManifest,
            double schemaVersion = XmlConstants.SchemaVersionLatest)
        {
            DebugCheck.NotNull(entityContainer);
            Debug.Assert(entityContainer.DataSpace == DataSpace.SSpace);

            var storeModel = new EdmModel(entityContainer, schemaVersion);

            if (providerInfo != null)
            {
                storeModel.ProviderInfo = providerInfo;
            }

            if (providerManifest != null)
            {
                storeModel.ProviderManifest = providerManifest;
            }

            return storeModel;
        }

        internal static EdmModel CreateConceptualModel(
            double schemaVersion = XmlConstants.SchemaVersionLatest)
        {
            return new EdmModel(DataSpace.CSpace, schemaVersion);
        }

        internal static EdmModel CreateConceptualModel(
            EntityContainer entityContainer,
            double schemaVersion = XmlConstants.SchemaVersionLatest)
        {
            DebugCheck.NotNull(entityContainer);
            Debug.Assert(entityContainer.DataSpace == DataSpace.CSpace);

            return new EdmModel(entityContainer, schemaVersion);
        }
    }
}
