// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public sealed class ConventionsConfigurationTests
    {
        [Fact]
        public void Add_should_append_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var mockConvention2 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention1.Object });

            conventionsConfiguration.Add(mockConvention2.Object);

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.Same(mockConvention2.Object, conventionsConfiguration.Conventions.Last());
        }

        [Fact]
        public void Generic_Add_should_append_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention1.Object });

            conventionsConfiguration.Add<ConventionFixture>();

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.Same(mockConvention1.Object, conventionsConfiguration.Conventions.First());
            Assert.IsType<ConventionFixture>(conventionsConfiguration.Conventions.Last());
        }

        [Fact]
        public void AddAfter_should_add_after_existing_convention()
        {
            var mockConvention = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { new ConventionFixture() });

            conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.Conventions.Last());
        }

        [Fact]
        public void AddAfter_should_throw_when_after_convention_not_found()
        {
            var mockConvention = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(() => conventionsConfiguration.AddAfter<ConventionFixture>(mockConvention.Object)).
                    Message);
        }

        [Fact]
        public void AddBefore_should_add_before_existing_convention()
        {
            var mockConvention = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { new ConventionFixture() });

            conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention.Object);

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.Same(mockConvention.Object, conventionsConfiguration.Conventions.First());
        }

        [Fact]
        public void AddBefore_should_throw_when_before_convention_not_found()
        {
            var mockConvention = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration();

            Assert.Equal(
                Strings.ConventionNotFound(mockConvention.Object.GetType(), typeof(ConventionFixture)),
                Assert.Throws<InvalidOperationException>(() => conventionsConfiguration.AddBefore<ConventionFixture>(mockConvention.Object))
                    .
                    Message);
        }

        private class ConventionFixture : IConvention
        {
        }

        [Fact]
        public void Remove_should_remove_the_conventions_from_the_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var mockConvention2 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention1.Object, mockConvention2.Object });

            conventionsConfiguration.Remove(new[] { mockConvention1.Object, mockConvention2.Object });

            Assert.Equal(0, conventionsConfiguration.Conventions.Count());
        }

        [Fact]
        public void Generic_Remove_should_remove_all_matching_conventions()
        {
            var conventionsConfiguration = new ConventionsConfiguration(new[] { new ConventionFixture(), new ConventionFixture() });

            conventionsConfiguration.Remove<ConventionFixture>();

            Assert.Equal(0, conventionsConfiguration.Conventions.Count());
        }

        [Fact]
        public void ApplyModel_should_run_model_conventions()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var mockConvention = new Mock<IEdmConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(model), Times.Once());
        }

        [Fact]
        public void ApplyMapping_should_run_mapping_conventions()
        {
            var mapping = new DbDatabaseMapping();
            var mockConvention = new Mock<IDbMappingConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyMapping(mapping);

            mockConvention.Verify(c => c.Apply(mapping), Times.Once());
        }

        [Fact]
        public void ApplyDatabase_should_run_database_conventions()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var mockConvention = new Mock<IDbConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(database), Times.Once());
        }

        [Fact]
        public void ApplyModel_should_run_targeted_model_conventions()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var mockConvention = new Mock<IEdmConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[]
                    {
                        mockConvention.Object
                    });

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(entityType, model), Times.Once());
        }

        [Fact]
        public void ApplyDatabase_should_run_targeted_model_conventions()
        {
            var database = new EdmModel(DataSpace.SSpace);
            var table = database.AddTable("T");
            var mockConvention = new Mock<IDbConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[]
                    {
                        mockConvention.Object
                    });

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(table, database), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_model_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);

            mockConvention.Verify(c => c.Apply(modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_encapsulated_model_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);

            mockConvention.Verify(c => c.ApplyModelConfiguration(modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_type_model_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<Type>>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(typeof(object), modelConfiguration);

            mockConvention.Verify(
                c => c.Apply(typeof(object), modelConfiguration), Times.Once());
        }

        [Fact]
        public void ApplyModelConfiguration_should_run_encapsulated_type_model_configuration_conventions()
        {
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
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
                new IConvention[] { mockConvention1.Object, mockConvention2.Object });
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
                new IConvention[] { mockConvention.Object });
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
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyPluralizingTableNameConvention(model);

            mockConvention.Verify(c => c.Apply(It.IsAny<EntityType>(), model), Times.Once());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<PropertyInfo, Properties.Primitive.StringPropertyConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
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
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
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
                new IConvention[] { mockConvention1.Object, mockConvention2.Object });
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
                new IConvention[] { mockConvention.Object });
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
                new IConvention[] { mockConvention.Object });
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
                new IConvention[] { mockConvention1.Object, mockConvention2.Object, mockConvention3.Object });
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
                new IConvention[] { mockConvention1.Object, mockConvention2.Object });
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
                new IConvention[] { mockConvention.Object });
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
            var mockConvention = new Mock<Convention>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[] { mockConvention.Object });

            var clone = conventionsConfiguration.Clone();

            Assert.NotSame(conventionsConfiguration, clone);
            Assert.Equal(conventionsConfiguration.Conventions, clone.Conventions);
        }
    }
}
