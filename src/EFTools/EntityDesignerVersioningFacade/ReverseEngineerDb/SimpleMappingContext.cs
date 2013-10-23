// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    internal class SimpleMappingContext
    {
        public readonly EdmModel StoreModel;

        public readonly bool IncludeForeignKeyProperties;

        private readonly Dictionary<EdmProperty, EdmProperty> _propertyMappings =
            new Dictionary<EdmProperty, EdmProperty>();

        private readonly Dictionary<EntityType, EntityType> _entityTypeMappings =
            new Dictionary<EntityType, EntityType>();

        private readonly Dictionary<EntitySet, EntitySet> _entitySetMappings =
            new Dictionary<EntitySet, EntitySet>();

        private readonly Dictionary<EntityContainer, EntityContainer> _entityContainerMappings =
            new Dictionary<EntityContainer, EntityContainer>();

        private readonly Dictionary<AssociationType, AssociationType> _associationTypeMappings =
            new Dictionary<AssociationType, AssociationType>();

        private readonly Dictionary<AssociationSet, AssociationSet> _associationSetMappings =
            new Dictionary<AssociationSet, AssociationSet>();

        private readonly Dictionary<CollapsibleEntityAssociationSets, AssociationSet> _collapsedAssociationSetMappings =
            new Dictionary<CollapsibleEntityAssociationSets, AssociationSet>();

        private readonly Dictionary<AssociationEndMember, AssociationEndMember> _associationEndMemberMappings =
            new Dictionary<AssociationEndMember, AssociationEndMember>();

        private readonly Dictionary<AssociationSetEnd, AssociationSetEnd> _associationSetEndMappings =
            new Dictionary<AssociationSetEnd, AssociationSetEnd>();

        private readonly Dictionary<EdmFunction, EdmFunction> _functionMappings =
            new Dictionary<EdmFunction, EdmFunction>();

        public readonly List<EdmSchemaError> Errors = new List<EdmSchemaError>();

        public readonly HashSet<EdmProperty> StoreForeignKeyProperties = new HashSet<EdmProperty>();

        public SimpleMappingContext(EdmModel storeModel, bool includeForeignKeyProperties)
        {
            Debug.Assert(storeModel != null, "storeModel != null");

            StoreModel = storeModel;
            IncludeForeignKeyProperties = includeForeignKeyProperties;
        }

        public void AddMapping(EdmProperty storeProperty, EdmProperty conceptualProperty)
        {
            _propertyMappings.Add(storeProperty, conceptualProperty);
        }

        public void AddMapping(EntityType storeEntityType, EntityType conceptualEntityType)
        {
            _entityTypeMappings.Add(storeEntityType, conceptualEntityType);
        }

        public void AddMapping(EntitySet storeEntitySet, EntitySet conceptualEntitySet)
        {
            _entitySetMappings.Add(storeEntitySet, conceptualEntitySet);
        }

        public void AddMapping(EntityContainer storeEntityContainer, EntityContainer conceptualEntityContainer)
        {
            _entityContainerMappings.Add(storeEntityContainer, conceptualEntityContainer);
        }

        public void AddMapping(AssociationType storeAssociationType, AssociationType conceptualAssociationType)
        {
            _associationTypeMappings.Add(storeAssociationType, conceptualAssociationType);
        }

        public void AddMapping(AssociationSet storeAssociationSet, AssociationSet conceptualAssociationSet)
        {
            _associationSetMappings.Add(storeAssociationSet, conceptualAssociationSet);
        }

        public void AddMapping(
            CollapsibleEntityAssociationSets collapsedAssociationSet,
            AssociationSet conceptualAssociationSet)
        {
            _collapsedAssociationSetMappings.Add(collapsedAssociationSet, conceptualAssociationSet);
        }

        public void AddMapping(AssociationEndMember storeAssociationEndMember, AssociationEndMember conceptualAssociationEndMember)
        {
            _associationEndMemberMappings.Add(storeAssociationEndMember, conceptualAssociationEndMember);
        }

        public void AddMapping(AssociationSetEnd storeAssociationSetEnd, AssociationSetEnd conceptualAssociationSetEnd)
        {
            _associationSetEndMappings.Add(storeAssociationSetEnd, conceptualAssociationSetEnd);
        }

        public void AddMapping(EdmFunction storeFunction, EdmFunction functionImport)
        {
            _functionMappings.Add(storeFunction, functionImport);
        }

        public void RemoveMapping(EntitySet storeEntitySet)
        {
            _entitySetMappings.Remove(storeEntitySet);
            _entityTypeMappings.Remove(storeEntitySet.ElementType);
        }

        public EdmProperty this[EdmProperty storeProperty]
        {
            get { return _propertyMappings[storeProperty]; }
        }

        public EntityType this[EntityType storeEntityType]
        {
            get { return _entityTypeMappings[storeEntityType]; }
        }

        public EntitySet this[EntitySet storeEntitySet]
        {
            get { return _entitySetMappings[storeEntitySet]; }
        }

        public EntityContainer this[EntityContainer storeEntityContainer]
        {
            get { return _entityContainerMappings[storeEntityContainer]; }
        }

        public AssociationType this[AssociationType storeAssociationType]
        {
            get { return _associationTypeMappings[storeAssociationType]; }
        }

        public AssociationSet this[AssociationSet storeAssociationSet]
        {
            get { return _associationSetMappings[storeAssociationSet]; }
        }

        public AssociationSet this[CollapsibleEntityAssociationSets collapsedAssociationSet]
        {
            get { return _collapsedAssociationSetMappings[collapsedAssociationSet]; }
        }

        public AssociationEndMember this[AssociationEndMember storeAssociationEndMember]
        {
            get { return _associationEndMemberMappings[storeAssociationEndMember]; }
        }

        public AssociationSetEnd this[AssociationSetEnd storeAssociationSetEnd]
        {
            get { return _associationSetEndMappings[storeAssociationSetEnd]; }
        }

        public EdmFunction this[EdmFunction storeFunction]
        {
            get { return _functionMappings[storeFunction]; }
        }

        public bool TryGetValue(EntitySet storeEntitySet, out EntitySet conceptualEntitySet)
        {
            return _entitySetMappings.TryGetValue(storeEntitySet, out conceptualEntitySet);
        }

        public bool TryGetValue(AssociationType storeAssociationType, out AssociationType conceptualAssociationType)
        {
            return _associationTypeMappings.TryGetValue(storeAssociationType, out conceptualAssociationType);
        }

        public IEnumerable<EntityContainer> StoreContainers()
        {
            return _entityContainerMappings.Keys;
        }

        public IEnumerable<EntitySet> StoreEntitySets()
        {
            return _entitySetMappings.Keys;
        }

        public IEnumerable<AssociationType> StoreAssociationTypes()
        {
            return _associationTypeMappings.Keys;
        }

        public IEnumerable<AssociationSet> StoreAssociationSets()
        {
            return _associationSetMappings.Keys;
        }

        public IEnumerable<AssociationEndMember> StoreAssociationEndMembers()
        {
            return _associationEndMemberMappings.Keys;
        }

        public IEnumerable<AssociationSetEnd> StoreAssociationSetEnds()
        {
            return _associationSetEndMappings.Keys;
        }

        public IEnumerable<EntityContainer> ConceptualContainers()
        {
            return _entityContainerMappings.Values;
        }

        public IEnumerable<EntitySet> ConceptualEntitySets()
        {
            return _entitySetMappings.Values;
        }

        public IEnumerable<EntityType> ConceptualEntityTypes()
        {
            return _entityTypeMappings.Values;
        }

        public IEnumerable<AssociationType> ConceptualAssociationTypes()
        {
            return _associationTypeMappings.Values;
        }

        public IEnumerable<AssociationSet> ConceptualAssociationSets()
        {
            return _associationSetMappings.Values.Concat(_collapsedAssociationSetMappings.Values);
        }

        public IEnumerable<AssociationEndMember> ConceptualAssociationEndMembers()
        {
            return _associationEndMemberMappings.Values;
        }

        public IEnumerable<AssociationSetEnd> ConceptualAssociationSetEnds()
        {
            return _associationSetEndMappings.Values;
        }

        public IEnumerable<EdmFunction> MappedStoreFunctions()
        {
            return _functionMappings.Keys;
        }

        public IEnumerable<CollapsibleEntityAssociationSets> CollapsedAssociationSets()
        {
            return _collapsedAssociationSetMappings.Keys;
        }
    }
}
