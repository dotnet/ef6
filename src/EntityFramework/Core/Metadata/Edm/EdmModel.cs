// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm.Validation;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class EdmModel : MetadataItem
    {
        private readonly List<EntityContainer> _containers = new List<EntityContainer>();
        private readonly List<AssociationType> _associationTypes = new List<AssociationType>();
        private readonly List<ComplexType> _complexTypes = new List<ComplexType>();
        private readonly List<EntityType> _entityTypes = new List<EntityType>();
        private readonly List<EnumType> _enumTypes = new List<EnumType>();
        private readonly List<EdmFunction> _functions = new List<EdmFunction>();

        private readonly DataSpace _dataSpace;

        private DbProviderInfo _providerInfo;
        private DbProviderManifest _providerManifest;

        public double SchemaVersion { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public EdmModel(EntityContainer entityContainer, double version = XmlConstants.SchemaVersionLatest)
        {
            Check.NotNull(entityContainer, "entityContainer");

            _dataSpace = entityContainer.DataSpace;
            _containers.Add(entityContainer);
            SchemaVersion = version;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public EdmModel(DataSpace dataSpace, double version = XmlConstants.SchemaVersionLatest)
        {
            if (dataSpace != DataSpace.CSpace
                && dataSpace != DataSpace.SSpace)
            {
                throw new ArgumentException(Strings.EdmModel_InvalidDataSpace(dataSpace), "dataSpace");
            }

            _containers.Add(
                new EntityContainer(
                    dataSpace == DataSpace.CSpace
                        ? "CodeFirstContainer"
                        : "CodeFirstDatabase",
                    dataSpace));

            _dataSpace = dataSpace;
            SchemaVersion = version;
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

        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.MetadataItem; }
        }

        internal override string Identity
        {
            get { return "EdmModel" + Containers.Single().Name;  }
        }

        public virtual IEnumerable<EntityContainer> Containers
        {
            get { return _containers; }
        }

        public virtual IEnumerable<string> NamespaceNames
        {
            get
            {
                return NamespaceItems
                    .Select(t => t.NamespaceName)
                    .Distinct();
            }
        }

        public IEnumerable<EdmType> NamespaceItems
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

        public IEnumerable<GlobalItem> GlobalItems
        {
            get { return NamespaceItems.Concat<GlobalItem>(Containers); }
        }

        public DbProviderInfo ProviderInfo
        {
            get { return _providerInfo; }
            set
            {
                DebugCheck.NotNull(value);

                _providerInfo = value;
            }
        }

        public DbProviderManifest ProviderManifest
        {
            get { return _providerManifest; }
            set
            {
                DebugCheck.NotNull(value);

                _providerManifest = value;
            }
        }

        public IEnumerable<AssociationType> AssociationTypes
        {
            get { return _associationTypes; }
        }

        public IEnumerable<ComplexType> ComplexTypes
        {
            get { return _complexTypes; }
        }

        public IEnumerable<EntityType> EntityTypes
        {
            get { return _entityTypes; }
        }

        public IEnumerable<EnumType> EnumTypes
        {
            get { return _enumTypes; }
        }

        public IEnumerable<EdmFunction> Functions
        {
            get { return _functions; }
        }

        public void AddItem(AssociationType associationType)
        {
            Check.NotNull(associationType, "associationType");
            ValidateSpace(associationType, "associationType");

            _associationTypes.Add(associationType);
        }

        public void RemoveItem(AssociationType associationType)
        {
            Check.NotNull(associationType, "associationType");

            _associationTypes.Remove(associationType);
        }

        public void AddItem(ComplexType complexType)
        {
            Check.NotNull(complexType, "complexType");
            ValidateSpace(complexType, "complexType");

            _complexTypes.Add(complexType);
        }

        public void AddItem(EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");
            ValidateSpace(entityType, "entityType");

            _entityTypes.Add(entityType);
        }

        public void RemoveItem(EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityTypes.Remove(entityType);
        }

        public void AddItem(EnumType enumType)
        {
            Check.NotNull(enumType, "enumType");
            ValidateSpace(enumType, "enumType");

            _enumTypes.Add(enumType);
        }

        public void AddItem(EdmFunction function)
        {
            Check.NotNull(function, "function");
            ValidateSpace(function, "function");

            _functions.Add(function);
        }

        private void ValidateSpace(GlobalItem item, string parameterName)
        {
            if (item.DataSpace != _dataSpace)
            {
                throw new ArgumentException(Strings.EdmModel_AddItem_NonMatchingNamespace, parameterName);
            }
        }
    }
}
