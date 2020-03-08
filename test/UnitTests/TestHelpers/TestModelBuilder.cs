// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;

    internal class TestModelBuilder
    {
        public static implicit operator EdmModel(TestModelBuilder testModelBuilder)
        {
            return testModelBuilder._model;
        }

        private readonly EdmModel _model;
        private EntityType _entityType;
        private int _counter;

        public TestModelBuilder()
        {
            _model = new EdmModel(DataSpace.CSpace);
        }

        public TestModelBuilder Entities(params string[] names)
        {
            foreach (var name in names)
            {
                Entity(name);
            }

            return this;
        }

        public TestModelBuilder Entity(string name, bool addSet = true)
        {
            _entityType = _model.AddEntityType(name);

            Type type = new MockType(name);

            _entityType.GetMetadataProperties().SetClrType(type);

            if (addSet)
            {
                _model.AddEntitySet(name + "Set", _entityType);
            }

            return this;
        }

        public TestModelBuilder Property(string propertyName)
        {
            var property1 = EdmProperty.CreatePrimitive(propertyName, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            _entityType.AddMember(property1);
            var property = property1;
            property.SetClrPropertyInfo(new MockPropertyInfo(typeof(string), propertyName));
            return this;
        }

        public TestModelBuilder Key(string key)
        {
            var property1 = EdmProperty.CreatePrimitive(key, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            _entityType.AddMember(property1);
            var property = property1;
            property.SetClrPropertyInfo(new MockPropertyInfo(typeof(int), key));

            _entityType.AddKeyMember(property);

            return this;
        }

        public TestModelBuilder Subclass(string name, string baseName = null)
        {
            var previous = baseName == null ? _entityType : _model.GetEntityType(baseName);

            Entity(name, false);

            _entityType.BaseType = previous;

            return this;
        }

        public TestModelBuilder Association(
            string sourceEntity, RelationshipMultiplicity sourceEndKind,
            string targetEntity, RelationshipMultiplicity targetEndKind)
        {
            _model.AddAssociationSet(
                "AssociationSet" + _counter++,
                _model.AddAssociationType(
                    "Association",
                    _model.GetEntityType(sourceEntity), sourceEndKind,
                    _model.GetEntityType(targetEntity), targetEndKind));

            return this;
        }

        public TestModelBuilder Association(
            string name,
            string sourceEntity, RelationshipMultiplicity sourceEndKind, string sourceNavigationProperty,
            string targetEntity, RelationshipMultiplicity targetEndKind, string targetNavigationProperty)
        {
            var sourceEntityType = _model.GetEntityType(sourceEntity);
            var targetEntityType = _model.GetEntityType(targetEntity);

            var association
                = _model.AddAssociationType(name, sourceEntityType, sourceEndKind, targetEntityType, targetEndKind);

            _model.AddAssociationSet(name + "Set", association);

            if (sourceNavigationProperty != null)
            {
                sourceEntityType.AddNavigationProperty(sourceNavigationProperty, association);
            }

            if (targetNavigationProperty != null)
            {
                targetEntityType.AddNavigationProperty(targetNavigationProperty, association);
            }

            return this;
        }
    }
}
