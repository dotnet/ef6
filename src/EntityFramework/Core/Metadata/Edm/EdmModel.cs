// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class EdmModel
    {
        private readonly List<EntityContainer> _containers = new List<EntityContainer>();
        private readonly List<AssociationType> _associationTypes = new List<AssociationType>();
        private readonly List<ComplexType> _complexTypes = new List<ComplexType>();
        private readonly List<EntityType> _entityTypes = new List<EntityType>();
        private readonly List<EnumType> _enumTypes = new List<EnumType>();

        private DbProviderInfo _providerInfo;

        public double Version { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public EdmModel InitializeConceptual(double version = XmlConstants.EdmVersionForV3)
        {
            Version = version;

            _containers.Add(new EntityContainer("CodeFirstContainer", DataSpace.CSpace));

            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public EdmModel InitializeStore(double version = XmlConstants.StoreVersionForV3)
        {
            Version = version;

            _containers.Add(new EntityContainer("CodeFirstDatabase", DataSpace.SSpace));

            return this;
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
                    .Concat(_enumTypes);
            }
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

        public void AddItem(AssociationType associationType)
        {
            Check.NotNull(associationType, "associationType");

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

            _complexTypes.Add(complexType);
        }

        public void AddItem(EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

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

            _enumTypes.Add(enumType);
        }
    }
}
