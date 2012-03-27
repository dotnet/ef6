namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class OneToManyCascadeDeleteConventionTests
    {
        [Fact]
        public void Apply_should_not_add_action_when_self_reference()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType
                = associationType.TargetEnd.EntityType
                    = new EdmEntityType();

            ((IEdmConvention<EdmAssociationType>)new OneToManyCascadeDeleteConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Equal(null, associationType.SourceEnd.DeleteAction);
            Assert.Equal(null, associationType.TargetEnd.DeleteAction);
        }

        [Fact]
        public void Apply_should_not_add_action_when_has_existing_action()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.TargetEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.DeleteAction = EdmOperationAction.Restrict;

            ((IEdmConvention<EdmAssociationType>)new OneToManyCascadeDeleteConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Equal(EdmOperationAction.Restrict, associationType.SourceEnd.DeleteAction);
            Assert.Equal(null, associationType.TargetEnd.DeleteAction);
        }

        [Fact]
        public void Apply_should_add_action_when_is_required_to_many()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.TargetEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;

            ((IEdmConvention<EdmAssociationType>)new OneToManyCascadeDeleteConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Equal(EdmOperationAction.Cascade, associationType.SourceEnd.DeleteAction);
        }

        [Fact]
        public void Apply_should_add_action_when_is_many_to_required()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.TargetEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;

            ((IEdmConvention<EdmAssociationType>)new OneToManyCascadeDeleteConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Equal(EdmOperationAction.Cascade, associationType.TargetEnd.DeleteAction);
        }
    }
}