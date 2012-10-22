// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
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
        public void Add_lightweight_should_append_convention_on_to_internal_list()
        {
            var mockConvention1 = new Mock<IConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention1.Object });

            conventionsConfiguration.Add(entities => { });

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.IsType<LightweightConvention>(conventionsConfiguration.Conventions.Last());
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
        public void AddAfter_lightweight_should_add_after_existing_convention()
        {
            var conventionsConfiguration = new ConventionsConfiguration(new[] { new ConventionFixture() });

            conventionsConfiguration.AddAfter<ConventionFixture>(entities => { });

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.IsType<LightweightConvention>(conventionsConfiguration.Conventions.Last());
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
        public void AddBefore_lightweight_should_add_before_existing_convention()
        {
            var conventionsConfiguration = new ConventionsConfiguration(new[] { new ConventionFixture() });

            conventionsConfiguration.AddBefore<ConventionFixture>(entities => { });

            Assert.Equal(2, conventionsConfiguration.Conventions.Count());
            Assert.IsType<LightweightConvention>(conventionsConfiguration.Conventions.First());
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
        public void ApplyModel_should_run_model_conventions()
        {
            var model = new EdmModel().Initialize();
            var mockConvention = new Mock<IEdmConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(model), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyDatabase_should_run_database_conventions()
        {
            var database = new EdmModel().Initialize();
            var mockConvention = new Mock<IDbConvention>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(database), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyModel_should_run_targeted_model_conventions()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var mockConvention = new Mock<IEdmConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[]
                    {
                        mockConvention.Object
                    });

            conventionsConfiguration.ApplyModel(model);

            mockConvention.Verify(c => c.Apply(entityType, model), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyDatabase_should_run_targeted_model_conventions()
        {
            var database = new EdmModel().Initialize();
            var table = database.AddTable("T");
            var mockConvention = new Mock<IDbConvention<EntityType>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[]
                    {
                        mockConvention.Object
                    });

            conventionsConfiguration.ApplyDatabase(database);

            mockConvention.Verify(c => c.Apply(table, database), Times.AtMostOnce());
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
        public void ApplyModelConfiguration_should_run_type_model_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<Type, ModelConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyModelConfiguration(typeof(object), modelConfiguration);

            mockConvention.Verify(c => c.Apply(typeof(object), It.IsAny<Func<ModelConfiguration>>()), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyTypeConfiguration_should_run_type_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<Type, EntityTypeConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<Type, StructuralTypeConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[] { mockConvention1.Object, mockConvention2.Object });
            var entityTypeConfiguration = new Func<EntityTypeConfiguration>(() => new EntityTypeConfiguration(typeof(object)));

            conventionsConfiguration.ApplyTypeConfiguration(typeof(object), entityTypeConfiguration);

            mockConvention1.Verify(c => c.Apply(typeof(object), entityTypeConfiguration), Times.AtMostOnce());
            mockConvention2.Verify(c => c.Apply(typeof(object), entityTypeConfiguration), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_configuration_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<PropertyInfo, StringPropertyConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(new[] { mockConvention.Object });
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, () => new StringPropertyConfiguration());

            mockConvention.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<StringPropertyConfiguration>>()), Times.AtMostOnce());
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
                () => new NavigationPropertyConfiguration(mockPropertyInfo));

            mockConvention1.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<PropertyConfiguration>>()), Times.AtMostOnce());
            mockConvention2.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<NavigationPropertyConfiguration>>()), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_property_model_conventions()
        {
            var mockConvention = new Mock<IConfigurationConvention<PropertyInfo, ModelConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[] { mockConvention.Object });
            var mockPropertyInfo = new MockPropertyInfo(typeof(object), "N");
            var modelConfiguration = new ModelConfiguration();

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration);

            mockConvention.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<ModelConfiguration>>()), Times.AtMostOnce());
        }

        [Fact]
        public void ApplyPropertyConfiguration_should_run_compatible_property_configuration_conventions()
        {
            var mockConvention1 = new Mock<IConfigurationConvention<PropertyInfo, StringPropertyConfiguration>>();
            var mockConvention2 = new Mock<IConfigurationConvention<PropertyInfo, PropertyConfiguration>>();
            var mockConvention3 = new Mock<IConfigurationConvention<PropertyInfo, NavigationPropertyConfiguration>>();
            var conventionsConfiguration = new ConventionsConfiguration(
                new IConvention[] { mockConvention1.Object, mockConvention2.Object, mockConvention3.Object });
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");

            conventionsConfiguration.ApplyPropertyConfiguration(mockPropertyInfo, () => new StringPropertyConfiguration());

            mockConvention1.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<StringPropertyConfiguration>>()), Times.AtMostOnce());
            mockConvention2.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<PropertyConfiguration>>()), Times.AtMostOnce());
            mockConvention3.Verify(c => c.Apply(mockPropertyInfo, It.IsAny<Func<NavigationPropertyConfiguration>>()), Times.Never());
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

            conventionsConfiguration.ApplyPropertyTypeConfiguration(mockPropertyInfo, complexTypeConfiguration);

            mockConvention1.Verify(c => c.Apply(mockPropertyInfo, complexTypeConfiguration), Times.AtMostOnce());
            mockConvention2.Verify(c => c.Apply(mockPropertyInfo, complexTypeConfiguration), Times.AtMostOnce());
        }
    }
}
