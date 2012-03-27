namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class PluralizingEntitySetNameConventionTests
    {
        [Fact]
        public void Apply_should_set_pluralized_name()
        {
            var model = new EdmModel().Initialize();
            var entitySet = model.AddEntitySet("Cat", new EdmEntityType());

            ((IEdmConvention<EdmEntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_ignore_current_entity_set()
        {
            var model = new EdmModel().Initialize();
            var entitySet = model.AddEntitySet("Cats", new EdmEntityType());

            ((IEdmConvention<EdmEntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_uniquify_names()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("Cats", new EdmEntityType());
            var entitySet = model.AddEntitySet("Cat", new EdmEntityType());

            ((IEdmConvention<EdmEntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats1", entitySet.Name);
        }

        [Fact]
        public void Apply_should_uniquify_names_multiple()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("Cats1", new EdmEntityType());
            var entitySet1 = model.AddEntitySet("Cat", new EdmEntityType());
            var entitySet2 = model.AddEntitySet("Cat", new EdmEntityType());

            ((IEdmConvention<EdmEntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet1, model);

            ((IEdmConvention<EdmEntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet2, model);

            Assert.Equal("Cats", entitySet1.Name);
            Assert.Equal("Cats2", entitySet2.Name);
        }
    }
}