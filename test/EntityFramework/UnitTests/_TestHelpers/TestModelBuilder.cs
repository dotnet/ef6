namespace System.Data.Entity
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;

    internal class TestModelBuilder
    {
        public static implicit operator EdmModel(TestModelBuilder testModelBuilder)
        {
            return testModelBuilder._model;
        }

        private readonly EdmModel _model;
        private EdmEntityType _entityType;

        public TestModelBuilder()
        {
            _model = new EdmModel().Initialize();
        }

        public TestModelBuilder Entities(params string[] names)
        {
            foreach (var name in names)
            {
                _model.AddEntityType(name);
            }

            return this;
        }

        public TestModelBuilder Entity(string name, bool addSet = true)
        {
            _entityType = _model.AddEntityType(name);
            _entityType.SetClrType(new MockType(name));

            if (addSet)
            {
                _model.AddEntitySet(name + "Set", _entityType);
            }

            return this;
        }

        public TestModelBuilder Property(string propertyName)
        {
            var property = _entityType.AddPrimitiveProperty(propertyName);
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.SetClrPropertyInfo(new MockPropertyInfo(typeof(string), propertyName));
            return this;
        }

        public TestModelBuilder Key(string key)
        {
            var property = _entityType.AddPrimitiveProperty(key);
            property.SetClrPropertyInfo(new MockPropertyInfo(typeof(int), key));
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;

            _entityType.DeclaredKeyProperties.Add(property);

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
            string sourceEntity, EdmAssociationEndKind sourceEndKind,
            string targetEntity, EdmAssociationEndKind targetEndKind)
        {
            _model.AddAssociationSet(
                "AssociationSet",
                _model.AddAssociationType(
                    "Association",
                    _model.GetEntityType(sourceEntity), sourceEndKind,
                    _model.GetEntityType(targetEntity), targetEndKind));

            return this;
        }

        public TestModelBuilder Association(string name,
            string sourceEntity, EdmAssociationEndKind sourceEndKind, string sourceNavigationProperty,
            string targetEntity, EdmAssociationEndKind targetEndKind, string targetNavigationProperty)
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