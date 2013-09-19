// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Conventions.Sets;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;
    using System.Data.Entity.Infrastructure;

    public sealed class ConventionsConfigurationTests
    {
        [Fact]
        public void Add_should_append_configuration_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            
            conventionsConfiguration.Add(mockConvention1.Object);
            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Add_should_append_configuration_convention_on_to_internal_list_before_initial_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConfigurationConvention>();
            var mockConvention3 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add(mockConvention2.Object);
            conventionsConfiguration.Add(mockConvention3.Object);

            Assert.Equal(3, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConfigurationConventions.First());
            Assert.Same(mockConvention1.Object, conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Add_should_append_conceptual_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConceptualModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void Add_should_append_store_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IStoreModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object }));

            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void Add_should_append_mapping_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IDbMappingConvention>();
            var mockConvention2 = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConceptualToStoreMappingConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_configuration_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            
            conventionsConfiguration.Add(mockConvention1.Object);
            conventionsConfiguration.Add<ConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention1.Object, conventionsConfiguration.ConfigurationConventions.First());
            Assert.IsType<ConventionFixture>(conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_configuration_convention_on_to_internal_list_before_initial_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add<ConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.IsType<ConventionFixture>(conventionsConfiguration.ConfigurationConventions.First());
            Assert.Same(mockConvention1.Object, conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_conceptual_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add<ConceptualModelConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.IsType<ConceptualModelConventionFixture>(conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_store_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object }));

            conventionsConfiguration.Add<StoreModelConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.IsType<StoreModelConventionFixture>(conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_mapping_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add<MappingConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
            Assert.IsType<MappingConventionFixture>(conventionsConfiguration.ConceptualToStoreMappingConventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_configuration_convention()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { new ConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_conceptual_model_convention()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ConceptualModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddAfter<ConceptualModelConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_store_model_convention()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new StoreModelConventionFixture() }));

            conventionsConfiguration.AddAfter<StoreModelConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_mapping_convention()
        {
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new MappingConventionFixture() },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddAfter<MappingConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualToStoreMappingConventions.Last());
        }

        [Fact]
        public void AddAfter_throws_for_configuration_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IConfigurationConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<StoreModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_conceptual_model_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<StoreModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_store_model_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_mapping_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IDbMappingConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<StoreModelConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_when_after_configuration_convention_not_found()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(() => conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddAfter_throws_when_after_conceptual_model_convention_not_found()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddAfter<ConceptualModelConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddAfter_throws_when_after_store_model_convention_not_found()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddAfter<StoreModelConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddAfter_throws_when_after_mapping_convention_not_found()
        {
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddAfter<MappingConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddBefore_should_add_before_existing_convention()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { new ConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConfigurationConventions.First());
        }

        [Fact]
        public void AddBefore_should_add_before_existing_conceptual_model_convention()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ConceptualModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddBefore<ConceptualModelConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualModelConventions.First());
        }

        [Fact]
        public void AddBefore_should_add_before_existing_store_model_convention()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new StoreModelConventionFixture() }));

            conventionsConfiguration.AddBefore<StoreModelConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.StoreModelConventions.First());
        }

        [Fact]
        public void AddBefore_should_add_before_existing_mapping_convention()
        {
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new MappingConventionFixture() },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddBefore<MappingConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualToStoreMappingConventions.First());
        }

        [Fact]
        public void AddBefore_throws_for_configuration_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IConfigurationConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<StoreModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_conceptual_model_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<StoreModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_store_model_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<MappingConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_mapping_convention_when_other_convention_category_expected()
        {
            var mockConvention = new Mock<IDbMappingConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConceptualModelConventionFixture>(mockConvention)).Message);

            Assert.Equal(
                Strings.ConventionsConfiguration_ConventionTypeMissmatch(mockConvention, typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<StoreModelConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_when_before_configuration_convention_not_found()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(() => conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention.Object))
                    .Message);
        }

        [Fact]
        public void AddBefore_throws_when_before_conceptual_model_convention_not_found()
        {
            var mockConvention = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConceptualModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddBefore<ConceptualModelConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddBefore_throws_when_before_store_model_convention_not_found()
        {
            var mockConvention = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(StoreModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddBefore<StoreModelConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddBefore_throws_when_before_mapping_convention_not_found()
        {
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(MappingConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddBefore<MappingConventionFixture>(mockConvention.Object)).
                    Message);
        }

        private class ConventionFixture : Convention
        {
        }

        private class ConceptualModelConventionFixture : IConceptualModelConvention<EdmModel>
        {
            public virtual void Apply(EdmModel item, DbModel model)
            {
                throw new NotImplementedException();
            }
        }

        private class StoreModelConventionFixture : IStoreModelConvention<EdmModel>
        {
            public virtual void Apply(EdmModel item, DbModel model)
            {
                throw new NotImplementedException();
            }
        }

        private class MappingConventionFixture : IDbMappingConvention
        {
            public virtual void Apply(DbDatabaseMapping mapping)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Remove_should_remove_the_configuration_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove(new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.ConfigurationConventions.Count());
        }

        [Fact]
        public void Remove_should_remove_the_conceptual_model_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IConceptualModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IConceptualModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove(new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.ConceptualModelConventions.Count());
        }

        [Fact]
        public void Remove_should_remove_the_store_model_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IStoreModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object }));

            conventionsConfiguration.Remove(new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.StoreModelConventions.Count());
        }

        [Fact]
        public void Remove_should_remove_the_mapping_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IDbMappingConvention>();
            var mockConvention2 = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove(new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_configuration_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { new ConventionFixture(), new ConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove<ConventionFixture>();

            Assert.Equal(0, conventionsConfiguration.ConfigurationConventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_conceptual_model_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ConceptualModelConventionFixture(), new ConceptualModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove<ConceptualModelConventionFixture>();

            Assert.Equal(0, conventionsConfiguration.ConceptualModelConventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_store_model_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new StoreModelConventionFixture(), new StoreModelConventionFixture() }));

            conventionsConfiguration.Remove<StoreModelConventionFixture>();

            Assert.Equal(0, conventionsConfiguration.StoreModelConventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_mapping_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new MappingConventionFixture(), new MappingConventionFixture() },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove<MappingConventionFixture>();

            Assert.Equal(0, conventionsConfiguration.ConceptualToStoreMappingConventions.Count());
        }

        [Fact]
        public void ApplyConceptualModel_should_run_model_conventions_in_order()
        {
            var model = CreateDbModel();
            var mockConvention1 = new Mock<IConceptualModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IConceptualModelConvention<EdmModel>>();
            mockConvention1.Setup(c => c.Apply(model.GetConceptualModel(), model))
                .Callback(
                    () => mockConvention2.Verify(c => c.Apply(model.GetConceptualModel(), model), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyConceptualModel(model);

            mockConvention1.Verify(c => c.Apply(model.GetConceptualModel(), model), Times.Once());
            mockConvention2.Verify(c => c.Apply(model.GetConceptualModel(), model), Times.Once());
        }

        [Fact]
        public void ApplyMapping_should_run_mapping_conventions_in_order()
        {
            var mapping = new DbDatabaseMapping();
            var mockConvention1 = new Mock<IDbMappingConvention>();
            var mockConvention2 = new Mock<IDbMappingConvention>();
            mockConvention1.Setup(c => c.Apply(mapping))
                .Callback(
                    () => mockConvention2.Verify(c => c.Apply(mapping), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyMapping(mapping);

            mockConvention1.Verify(c => c.Apply(mapping), Times.Once());
            mockConvention2.Verify(c => c.Apply(mapping), Times.Once());
        }

        [Fact]
        public void ApplyStoreModel_should_run_database_conventions_in_order()
        {
            var model = CreateDbModel();
            var mockConvention1 = new Mock<IStoreModelConvention<EdmModel>>();
            var mockConvention2 = new Mock<IStoreModelConvention<EdmModel>>();
            mockConvention1.Setup(c => c.Apply(model.GetStoreModel(), model))
                .Callback(
                    () => mockConvention2.Verify(c => c.Apply(model.GetStoreModel(), model), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object }));

            conventionsConfiguration.ApplyStoreModel(model);

            mockConvention1.Verify(c => c.Apply(model.GetStoreModel(), model), Times.Once());
            mockConvention2.Verify(c => c.Apply(model.GetStoreModel(), model), Times.Once());
        }

        [Fact]
        public void ApplyConceptualModel_should_run_targeted_model_conventions()
        {
            var model = CreateDbModel();
            var entityType = model.GetConceptualModel().AddEntityType("E");
            var mockConvention = new Mock<IConceptualModelConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new IConvention[]
                        {
                            mockConvention.Object
                        },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyConceptualModel(model);

            mockConvention.Verify(c => c.Apply(entityType, model), Times.Once());
        }

        [Fact]
        public void ApplyStoreModel_should_run_targeted_model_conventions()
        {
            var model = CreateDbModel();
            var table = model.GetStoreModel().AddTable("T");
            var mockConvention = new Mock<IStoreModelConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new IConvention[]
                        {
                            mockConvention.Object
                        }));

            conventionsConfiguration.ApplyStoreModel(model);

            mockConvention.Verify(c => c.Apply(table, model), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_model_configuration_conventions_in_reverse_order()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConfigurationConvention>();
            mockConvention2.Setup(c => c.Apply(modelConfiguration))
                .Callback(() => mockConvention1.Verify(c => c.Apply(modelConfiguration), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);

            mockConvention1.Verify(c => c.Apply(modelConfiguration), Times.Once());
            mockConvention2.Verify(c => c.Apply(modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_encapsulated_model_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);

            mockConvention.Verify(c => c.ApplyModelConfiguration(modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_type_model_configuration_conventions_in_reverse_order()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockConvention1 = new Mock<IConfigurationConvention<Type>>();
            var mockConvention2 = new Mock<IConfigurationConvention<Type>>();
            mockConvention2.Setup(c => c.Apply(typeof(object), modelConfiguration))
                .Callback(() => mockConvention1.Verify(c => c.Apply(typeof(object), modelConfiguration), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyModelConfiguration(typeof(object), modelConfiguration);

            mockConvention1.Verify(c => c.Apply(typeof(object), modelConfiguration), Times.Once());
            mockConvention2.Verify(c => c.Apply(typeof(object), modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_encapsulated_type_model_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(typeof(object), modelConfiguration);

            mockConvention.Verify(c => c.ApplyModelConfiguration(typeof(object), It.IsAny<ModelConfiguration>()), Times.Once());
        }

        [Fact]
        public void ApplyTypeConfiguration_should_run_type_configuration_conventions_in_reverse_order()
        {
            var entityTypeConfiguration = new Func<EntityTypeConfiguration>(() => new EntityTypeConfiguration(typeof(object)));
            var mockConvention1 = new Mock<IConfigurationConvention<Type, EntityTypeConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<Type, StructuralTypeConfiguration>>();
            mockConvention2.Setup(c => c.Apply(typeof(object), entityTypeConfiguration, It.IsAny<ModelConfiguration>()))
                .Callback(() =>
                    mockConvention1.Verify(
                        c => c.Apply(typeof(object), entityTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyTypeConfiguration(typeof(object), entityTypeConfiguration, new ModelConfiguration());

            mockConvention1.Verify(c => c.Apply(typeof(object), entityTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Once());
            mockConvention2.Verify(c => c.Apply(typeof(object), entityTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Once());
        }

        [Fact]
        public void ApplyTypeConfiguration_should_run_encapsulated_type_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var entityTypeConfiguration = new Func<EntityTypeConfiguration>(() => new EntityTypeConfiguration(typeof(object)));

            conventionsConfiguration.ApplyTypeConfiguration(typeof(object), entityTypeConfiguration, new ModelConfiguration());

            mockConvention.Verify(
                c => c.ApplyTypeConfiguration(typeof(object), entityTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Once());
        }

        [Fact]
        public void ApplyPluralizingTableNameConvention_should_run_PluralizingTableName_conventions_in_order()
        {
            var model = CreateDbModel();
            model.GetStoreModel().AddItem(new EntityType("foo", "bar", DataSpace.SSpace));
            var mockConvention1 = new Mock<PluralizingTableNameConvention>();
            var mockConvention2 = new Mock<PluralizingTableNameConvention>();
            mockConvention1.Setup(c => c.Apply(It.IsAny<EntityType>(), model))
                .Callback(() => mockConvention2.Verify(c => c.Apply(It.IsAny<EntityType>(), model), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object }));

            conventionsConfiguration.ApplyPluralizingTableNameConvention(model);

            mockConvention1.Verify(c => c.Apply(It.IsAny<EntityType>(), model), Times.Once());
            mockConvention2.Verify(c => c.Apply(It.IsAny<EntityType>(), model), Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_configuration_conventions_in_reverse_order()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, Properties.Primitive.StringPropertyConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, Properties.Primitive.StringPropertyConfiguration>>();
            mockConvention2.Setup(
                c =>
                c.Apply(
                    mockPropertyInfo, It.IsAny<Func<Properties.Primitive.StringPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()))
                .Callback(
                    () =>
                    mockConvention1.Verify(
                        c =>
                        c.Apply(
                            mockPropertyInfo, It.IsAny<Func<Properties.Primitive.StringPropertyConfiguration>>(),
                            It.IsAny<ModelConfiguration>()), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyPropertyConfiguration(
                mockPropertyInfo, () => new Properties.Primitive.StringPropertyConfiguration(), new ModelConfiguration());

            mockConvention1.Verify(
                c =>
                c.Apply(
                    mockPropertyInfo, It.IsAny<Func<Properties.Primitive.StringPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Once());
            mockConvention2.Verify(
                c =>
                c.Apply(
                    mockPropertyInfo, It.IsAny<Func<Properties.Primitive.StringPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_encapsulated_property_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            conventionsConfiguration.ApplyPropertyConfiguration(
                mockPropertyInfo, () => new Properties.Primitive.StringPropertyConfiguration(), new ModelConfiguration());

            mockConvention.Verify(
                c => c.ApplyPropertyConfiguration(mockPropertyInfo, It.IsAny<Func<PropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_navigation_property_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, PropertyConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var mockPropertyInfo = new MockPropertyInfo(typeof(object), "N");

            conventionsConfiguration.ApplyPropertyConfiguration(
                mockPropertyInfo,
                () => new NavigationPropertyConfiguration(mockPropertyInfo),
                new ModelConfiguration());

            mockConvention1.Verify(
                c => c.Apply(mockPropertyInfo, It.IsAny<Func<PropertyConfiguration>>(), It.IsAny<ModelConfiguration>()), Times.Once());
            mockConvention2.Verify(
                c => c.Apply(mockPropertyInfo, It.IsAny<Func<NavigationPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_model_conventions_in_reverse_order()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(object), "N");
            var modelConfiguration = new ModelConfiguration();
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo>>();
            mockConvention2.Setup(c => c.Apply(mockPropertyInfo, modelConfiguration))
                .Callback(
                    () => mockConvention1.Verify(
                        c => c.Apply(mockPropertyInfo, modelConfiguration), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration);

            mockConvention1.Verify(
                c => c.Apply(mockPropertyInfo, modelConfiguration), Times.Once());
            mockConvention2.Verify(
                c => c.Apply(mockPropertyInfo, modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_encapsulated_property_model_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var mockPropertyInfo = new MockPropertyInfo(typeof(object), "N");
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration);

            mockConvention.Verify(c => c.ApplyPropertyConfiguration(mockPropertyInfo, It.IsAny<ModelConfiguration>()), Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_compatible_property_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, Properties.Primitive.StringPropertyConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, PropertyConfiguration>>();
            var mockConvention3 = new Mock<IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object, mockConvention3.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            conventionsConfiguration.ApplyPropertyConfiguration(
                mockPropertyInfo, () => new Properties.Primitive.StringPropertyConfiguration(), new ModelConfiguration());

            mockConvention1.Verify(
                c =>
                c.Apply(
                    mockPropertyInfo, It.IsAny<Func<Properties.Primitive.StringPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Once());
            mockConvention2.Verify(
                c => c.Apply(mockPropertyInfo, It.IsAny<Func<PropertyConfiguration>>(), It.IsAny<ModelConfiguration>()), Times.Once());
            mockConvention3.Verify(
                c => c.Apply(mockPropertyInfo, It.IsAny<Func<NavigationPropertyConfiguration>>(), It.IsAny<ModelConfiguration>()),
                Times.Never());
        }

        [Fact]
        public void ApplyPropertyTypeConfiguration_should_run_property_type_configuration_conventions_in_reverse_order()
        {
            var complexTypeConfiguration = new Func<ComplexTypeConfiguration>(() => new ComplexTypeConfiguration(typeof(object)));
            var mockPropertyInfo = new MockPropertyInfo();
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, ComplexTypeConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>>();
            mockConvention2.Setup(c => c.Apply(mockPropertyInfo, complexTypeConfiguration, It.IsAny<ModelConfiguration>()))
                .Callback(
                    () =>
                    mockConvention1.Verify(
                        c => c.Apply(mockPropertyInfo, complexTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Never()));

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyPropertyTypeConfiguration(mockPropertyInfo, complexTypeConfiguration, new ModelConfiguration());

            mockConvention1.Verify(c => c.Apply(mockPropertyInfo, complexTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Once());
            mockConvention2.Verify(c => c.Apply(mockPropertyInfo, complexTypeConfiguration, It.IsAny<ModelConfiguration>()), Times.Once());
        }

        [Fact]
        public void ApplyPropertyTypeConfiguration_should_run_encapsulated_property_type_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var complexTypeConfiguration = new Func<ComplexTypeConfiguration>(() => new ComplexTypeConfiguration(typeof(object)));
            var mockPropertyInfo = new MockPropertyInfo();

            conventionsConfiguration.ApplyPropertyTypeConfiguration(mockPropertyInfo, complexTypeConfiguration, new ModelConfiguration());

            mockConvention.Verify(
                c => c.ApplyPropertyTypeConfiguration(mockPropertyInfo, complexTypeConfiguration, It.IsAny<ModelConfiguration>()),
                Times.Once());
        }

        [Fact]
        public void Clone_returns_an_identical_object()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConceptualModelConvention<EdmModel>>();
            var mockConvention3 = new Mock<IDbMappingConvention>();
            var mockConvention4 = new Mock<IStoreModelConvention<EdmModel>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object },
                    new[] { mockConvention2.Object },
                    new[] { mockConvention3.Object },
                    new[] { mockConvention4.Object }));

            var clone = conventionsConfiguration.Clone();

            Assert.NotSame(conventionsConfiguration, clone);
            Assert.Same(mockConvention1.Object, clone.ConfigurationConventions.Single());
            Assert.Same(mockConvention2.Object, clone.ConceptualModelConventions.Single());
            Assert.Same(mockConvention3.Object, clone.ConceptualToStoreMappingConventions.Single());
            Assert.Same(mockConvention4.Object, clone.StoreModelConventions.Single());
        }

        private static DbModel CreateDbModel()
        {
            return new DbModel(ProviderRegistry.Sql2008_ProviderInfo, ProviderRegistry.Sql2008_ProviderManifest);
        }
    }
}
