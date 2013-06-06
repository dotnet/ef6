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

    public sealed class ConventionsConfigurationTests
    {
        [Fact]
        public void Add_should_append_configuration_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConfigurationConvention>();
            var mockConvention2 = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Add_should_append_entity_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var mockConvention2 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add(DataSpace.CSpace, mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void Add_should_append_entity_db_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var mockConvention2 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object }));

            conventionsConfiguration.Add(DataSpace.SSpace, mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void Add_throws_for_model_convention_when_configuration_convention_expected()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(mockConvention1.Object.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add(mockConvention1.Object)).Message);
        }

        [Fact]
        public void Add_throws_for_configuration_convention_when_model_configuration_expected()
        {
            var mockConvention1 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(mockConvention1.Object.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add(DataSpace.CSpace, mockConvention1.Object)).Message);
        }

        [Fact]
        public void Add_throws_for_invalid_DataSpace()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add(DataSpace.CSSpace, mockConvention1.Object)).Message);
        }

        [Fact]
        public void Generic_Add_should_append_configuration_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add<ConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.ConfigurationConventions.Count());
            Assert.Same(mockConvention1.Object, conventionsConfiguration.ConfigurationConventions.First());
            Assert.IsType<ConventionFixture>(conventionsConfiguration.ConfigurationConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_entity_model_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Add<ModelConventionFixture>(DataSpace.CSpace);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.IsType<ModelConventionFixture>(conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_entity_db_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object }));

            conventionsConfiguration.Add<ModelConventionFixture>(DataSpace.SSpace);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.IsType<ModelConventionFixture>(conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void Generic_Add_throws_for_model_convention_when_configuration_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add<ModelConventionFixture>()).Message);
        }

        [Fact]
        public void Generic_Add_throws_for_configuration_convention_when_model_configuration_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add<ConventionFixture>(DataSpace.CSpace)).Message);
        }

        [Fact]
        public void Generic_Add_throws_for_invalid_DataSpace()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Add<ModelConventionFixture>(DataSpace.CSSpace)).Message);
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
        public void AddAfter_should_add_after_existing_entity_model_convention()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddAfter<ModelConventionFixture>(DataSpace.CSpace, mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualModelConventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_Db_convention()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture() }));

            conventionsConfiguration.AddAfter<ModelConventionFixture>(DataSpace.SSpace, mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.StoreModelConventions.Last());
        }

        [Fact]
        public void AddAfter_throws_for_model_convention_when_configuration_convention_expected()
        {
            var mockConvention = new Mock<IModelConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(mockConvention.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_after_model_convention_when_configuration_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ModelConventionFixture>(new Mock<IConfigurationConvention>().Object)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_configuration_convention_when_entity_model_convention_expected()
        {
            var mockConvention = new Mock<IConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(mockConvention.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ModelConventionFixture>(DataSpace.CSpace, mockConvention)).Message);
        }

        [Fact]
        public void AddAfter_throws_for_after_configuration_convention_when_entity_model_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ConventionFixture>(DataSpace.CSpace, new Mock<IModelConvention>().Object)).Message);
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
        public void AddAfter_throws_when_after_model_convention_not_found()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddAfter<ModelConventionFixture>(DataSpace.CSpace, mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddAfter_throws_for_invalid_DataSpace()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddAfter<ModelConventionFixture>(DataSpace.CSSpace, mockConvention1.Object)).Message);
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
        public void AddBefore_should_add_before_existing_entity_model_convention()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.AddBefore<ModelConventionFixture>(DataSpace.CSpace, mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.ConceptualModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.ConceptualModelConventions.First());
        }

        [Fact]
        public void AddBefore_should_add_before_existing_Db_convention()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture() }));

            conventionsConfiguration.AddBefore<ModelConventionFixture>(DataSpace.SSpace, mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.StoreModelConventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.StoreModelConventions.First());
        }

        [Fact]
        public void AddBefore_throws_for_model_convention_when_configuration_convention_expected()
        {
            var mockConvention = new Mock<IModelConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(mockConvention.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_before_model_convention_when_configuration_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ModelConventionFixture>(new Mock<IConfigurationConvention>().Object)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_configuration_convention_when_entity_model_convention_expected()
        {
            var mockConvention = new Mock<IConvention>().Object;
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(mockConvention.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ModelConventionFixture>(DataSpace.CSpace, mockConvention)).Message);
        }

        [Fact]
        public void AddBefore_throws_for_before_configuration_convention_when_entity_model_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ConventionFixture>(DataSpace.CSpace, new Mock<IModelConvention>().Object)).Message);
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
        public void AddBefore_throws_when_before_model_convention_not_found()
        {
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () => conventionsConfiguration.AddBefore<ModelConventionFixture>(DataSpace.CSpace, mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddBefore_throws_for_invalid_DataSpace()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.AddBefore<ModelConventionFixture>(DataSpace.CSSpace, mockConvention1.Object)).Message);
        }

        private class ConventionFixture : Convention
        {
        }

        private class ModelConventionFixture : IModelConvention
        {
            public virtual void Apply(EdmModel model)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Remove_should_remove_the_configuration_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var mockConvention2 = new Mock<IConvention>();
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
        public void Remove_should_remove_the_entity_model_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var mockConvention2 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove(DataSpace.CSpace,new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.ConceptualModelConventions.Count());
        }

        [Fact]
        public void Remove_should_remove_the_db_model_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var mockConvention2 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention1.Object, mockConvention2.Object }));

            conventionsConfiguration.Remove(DataSpace.SSpace, new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.StoreModelConventions.Count());
        }

        [Fact]
        public void Remove_throws_for_model_convention_when_configuration_convention_expected()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(mockConvention1.Object.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove(mockConvention1.Object)).Message);
        }

        [Fact]
        public void Remove_throws_for_configuration_convention_when_model_configuration_expected()
        {
            var mockConvention1 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(mockConvention1.Object.GetType()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove(DataSpace.CSpace, mockConvention1.Object)).Message);
        }

        [Fact]
        public void Remove_throws_for_invalid_DataSpace()
        {
            var mockConvention1 = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove(DataSpace.CSSpace, mockConvention1.Object)).Message);
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
        public void Generic_Remove_should_remove_all_matching_entity_model_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture(), new ModelConventionFixture() },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.Remove<ModelConventionFixture>(DataSpace.CSpace);

            Assert.Equal(0, conventionsConfiguration.ConceptualModelConventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_db_model_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { new ModelConventionFixture(), new ModelConventionFixture() }));

            conventionsConfiguration.Remove<ModelConventionFixture>(DataSpace.SSpace);

            Assert.Equal(0, conventionsConfiguration.StoreModelConventions.Count());
        }

        [Fact]
        public void Generic_Remove_throws_for_model_convention_when_configuration_convention_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotConfigurationConvention(typeof(ModelConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove<ModelConventionFixture>()).Message);
        }

        [Fact]
        public void Generic_Remove_throws_for_configuration_convention_when_model_configuration_expected()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_NotModelConvention(typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove<ConventionFixture>(DataSpace.CSpace)).Message);
        }

        [Fact]
        public void Generic_Remove_throws_for_invalid_DataSpace()
        {
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet());

            Assert.Equal(
                Strings.ConventionsConfiguration_InvalidDataSpace(DataSpace.CSSpace),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    conventionsConfiguration.Remove<ModelConventionFixture>(DataSpace.CSSpace)).Message);
        }

        [Fact]
        public void ApplyModel_should_run_model_conventions()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(model), Times.Once());
        }

        [Fact]
        public void ApplyMapping_should_run_mapping_conventions()
        {
            var mapping = new DbDatabaseMapping();
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyMapping(mapping);

            mockConvention.Verify(c => c.Apply(mapping), Times.Once());
        }

        [Fact]
        public void ApplyDatabase_should_run_database_conventions()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var mockConvention = new Mock<IModelConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention.Object }));

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(database), Times.Once());
        }

        [Fact]
        public void ApplyModel_should_run_targeted_model_conventions()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var mockConvention = new Mock<IModelConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    new IConvention[]
                        {
                            mockConvention.Object
                        },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(entityType, model), Times.Once());
        }

        [Fact]
        public void ApplyDatabase_should_run_targeted_model_conventions()
        {
            var database = new EdmModel(DataSpace.SSpace);
            var table = database.AddTable("T");
            var mockConvention = new Mock<IModelConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new IConvention[]
                        {
                            mockConvention.Object
                        }));

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(table, database), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_model_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);

            mockConvention.Verify(c => c.Apply(modelConfiguration), Times.Once());
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
        public void ApplyModelConfiguration_should_run_type_model_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<Type>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(typeof(object), modelConfiguration);

            mockConvention.Verify(
                c => c.Apply(typeof(object), modelConfiguration), Times.Once());
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
        public void ApplyTypeConfiguration_should_run_type_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<Type, EntityTypeConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<Type, StructuralTypeConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));

            var entityTypeConfiguration = new Func<EntityTypeConfiguration>(() => new EntityTypeConfiguration(typeof(object)));

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
        public void ApplyPluralizingTableNameConvention_should_run_PluralizingTableName_conventions()
        {
            var model = new EdmModel(DataSpace.SSpace);
            model.AddItem(new EntityType("foo", "bar", DataSpace.SSpace));
            var mockConvention = new Mock<PluralizingTableNameConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    new[] { mockConvention.Object }));

            conventionsConfiguration.ApplyPluralizingTableNameConvention(model);

            mockConvention.Verify(c => c.Apply(It.IsAny<EntityType>(), model), Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<PropertyInfo, Properties.Primitive.StringPropertyConfiguration>>();
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
        public void ApplyPropertyConfiguration_should_run_property_model_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<PropertyInfo>>();

            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new[] { mockConvention.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var mockPropertyInfo = new MockPropertyInfo(typeof(object), "N");
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration);

            mockConvention.Verify(
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
        public void ApplyPropertyTypeConfiguration_should_run_property_type_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, ComplexTypeConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, StructuralTypeConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new ConventionSet(
                    new IConvention[] { mockConvention1.Object, mockConvention2.Object },
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>(),
                    Enumerable.Empty<IConvention>()));
            var complexTypeConfiguration = new Func<ComplexTypeConfiguration>(() => new ComplexTypeConfiguration(typeof(object)));
            var mockPropertyInfo = new MockPropertyInfo();

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
            var mockConvention1 = new Mock<IConvention>();
            var mockConvention2 = new Mock<IModelConvention>();
            var mockConvention3 = new Mock<IConvention>();
            var mockConvention4 = new Mock<IModelConvention>();
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
    }
}
