// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public sealed class EntityTypeConfigurationTests
    {
        [Fact]
        public void Property_should_return_property_configuration()
        {
            var entityConfiguration = new EntityTypeConfiguration<Fixture>();

            Assert.NotNull(entityConfiguration.Property(f => f.Id));
        }

        [Fact]
        public void Property_should_return_property_configuration_for_complex_type()
        {
            var entityConfiguration = new EntityTypeConfiguration<Fixture>();

            Assert.NotNull(entityConfiguration.Property(f => f.Complex.Id));
        }

        [Fact]
        public void Configuration_should_return_internal_configuration()
        {
            var entityConfiguration = new EntityTypeConfiguration<object>();

            Assert.NotNull(entityConfiguration.Configuration);
            Assert.Equal(typeof(EntityTypeConfiguration), entityConfiguration.Configuration.GetType());
        }

        [Fact]
        public void HasKey_should_add_key_properties()
        {
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(Fixture));
            var entityConfiguration = new EntityTypeConfiguration<Fixture>(mockEntityTypeConfiguration.Object);

            entityConfiguration.HasKey(f => f.Id);

            mockEntityTypeConfiguration.Verify(e => e.Key(new[] { typeof(Fixture).GetProperty("Id") }));
        }

        [Fact]
        public void MapToStoredProcedures_should_call_method_on_internal_configuration()
        {
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(Fixture));
            var entityConfiguration = new EntityTypeConfiguration<Fixture>(mockEntityTypeConfiguration.Object);

            entityConfiguration.MapToStoredProcedures();

            mockEntityTypeConfiguration.Verify(e => e.MapToStoredProcedures());
        }

        [Fact]
        public void MapToStoredProcedures_when_config_action_should_call_method_on_internal_configuration()
        {
            var mockEntityTypeConfiguration = new Mock<EntityTypeConfiguration>(typeof(Fixture));
            var entityConfiguration = new EntityTypeConfiguration<Fixture>(mockEntityTypeConfiguration.Object);

            ModificationFunctionsConfiguration<Fixture> configuration = null;

            entityConfiguration.MapToStoredProcedures(c => { configuration = c; });
            
            mockEntityTypeConfiguration.Verify(e => e.MapToStoredProcedures(configuration.Configuration));
        }

        [Fact]
        public void HasKey_should_throw_when_invalid_key_expression()
        {
            var entityConfiguration = new EntityTypeConfiguration<object>();

            Assert.Equal(
                Strings.InvalidPropertiesExpression("o => o.ToString()"),
                Assert.Throws<InvalidOperationException>(() => entityConfiguration.HasKey(o => o.ToString())).Message);
        }

        [Fact]
        public void HasKey_should_throw_with_null_expression()
        {
            Assert.Equal(
                new ArgumentNullException("keyExpression").Message,
                Assert.Throws<ArgumentNullException>(() => new EntityTypeConfiguration<object>().HasKey<int>(null)).Message);
        }

        [Fact]
        public void Map_TDerived_should_throw_for_repeat_configuration_of_derived_type()
        {
            var entityConfiguration = new EntityTypeConfiguration<A>();
            Assert.Equal(
                Strings.InvalidChainedMappingSyntax("B"), Assert.Throws<InvalidOperationException>(
                    () => entityConfiguration
                              .Map<A>(m => m.ToTable("A"))
                              .Map<B>(mb => mb.ToTable("B"))
                              .Map<C>(mc => mc.ToTable("C"))
                              .Map<B>(mb2 => mb2.ToTable("B"))).Message);
        }

        [Fact]
        public void Map_TDerived_should_add_mapping_configuration_to_self_if_tderived_is_same_as_tentity()
        {
            var entityConfiguration = new EntityTypeConfiguration<A>();
            entityConfiguration.Map<A>(m => m.ToTable("A"));

            Assert.Equal("A", ((EntityTypeConfiguration)entityConfiguration.Configuration).GetTableName().Name);
        }

        #region Test Fixtures

        private class Fixture
        {
            public int Id { get; private set; }
            public Fixture Complex { get; set; }
        }

        private class A
        {
        }

        private class B : A
        {
        }

        private class C : A
        {
        }

        #endregion
    }
}
