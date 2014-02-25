// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Xunit;

    public class ConventionsTypeActivatorTests
    {
        [Fact]
        public void Activate_create_instance_for_convention_type_with_private_constructor()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        [Fact]
        public void Activate_create_instance_for_convention_type()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularPublicConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        [Fact]
        public void Activate_create_instance_for_store_model_convention_type_with_private_constructor()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularStoreModelConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        [Fact]
        public void Activate_create_instance_for_store_model_convention_type()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularStoreModelPublicConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        [Fact]
        public void Activate_create_instance_for_conceptual_model_convention_type_with_private_constructor()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularConceptualModelConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        [Fact]
        public void Activate_create_instance_for_conceptual_model_convention_type()
        {
            var instance = new ConventionsTypeActivator().Activate(typeof(RegularConceptualModelPublicConvention));

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IConvention>(instance);
        }

        class RegularConvention
            : Convention
        {
            private RegularConvention() { }
        }

        class RegularPublicConvention
            : Convention
        {
            public RegularPublicConvention() { }
        }

        class RegularStoreModelConvention
            : IStoreModelConvention<EdmProperty>
        {
            private RegularStoreModelConvention() { }

            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }


        class RegularStoreModelPublicConvention
           : IStoreModelConvention<EdmProperty>
        {
            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }

        class RegularConceptualModelPublicConvention
            : IConceptualModelConvention<EdmProperty>
        {
            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }

        class RegularConceptualModelConvention
            : IConceptualModelConvention<EdmProperty>
        {
            private RegularConceptualModelConvention() { }

            public void Apply(EdmProperty item, Infrastructure.DbModel model) { }
        }
    }
}
