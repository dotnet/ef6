// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public sealed class EntityTypeConfigurationTests
    {
        [Fact]
        public void Configure_should_set_configuration()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            entityTypeConfiguration.Configure(entityType, new EdmModel());

            Assert.Same(entityTypeConfiguration, entityType.GetConfiguration());
        }

        [Fact]
        public void Configure_should_configure_entity_set_name()
        {
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var entitySet = model.AddEntitySet("ESet", entityType);

            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object))
                                              {
                                                  EntitySetName = "MySet"
                                              };

            entityTypeConfiguration.Configure(entityType, model);

            Assert.Equal("MySet", entitySet.Name);
            Assert.Same(entityTypeConfiguration, entitySet.GetConfiguration());
        }

        [Fact]
        public void Configure_should_configure_properties()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            var mockPropertyConfiguration = new Mock<PrimitivePropertyConfiguration>();
            var mockPropertyInfo = new MockPropertyInfo();
            property.SetClrPropertyInfo(mockPropertyInfo);
            entityTypeConfiguration.Property(new PropertyPath(mockPropertyInfo), () => mockPropertyConfiguration.Object);

            entityTypeConfiguration.Configure(entityType, new EdmModel());

            mockPropertyConfiguration.Verify(p => p.Configure(property));
        }

        [Fact]
        public void Configure_should_throw_when_property_not_found()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            var mockPropertyConfiguration = new Mock<PrimitivePropertyConfiguration>();
            entityTypeConfiguration.Property(new PropertyPath(new MockPropertyInfo()), () => mockPropertyConfiguration.Object);

            Assert.Equal(
                Strings.PropertyNotFound(("P"), "E"),
                Assert.Throws<InvalidOperationException>(() => entityTypeConfiguration.Configure(entityType, new EdmModel())).Message);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            entityTypeConfiguration.ToTable("Foo");

            Assert.Equal("Foo", entityTypeConfiguration.GetTableName().Name);
        }

        [Fact]
        public void GetTableName_returns_current_TableName()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            Assert.Equal(null, entityTypeConfiguration.GetTableName());

            entityTypeConfiguration.ToTable("Foo");
            Assert.Equal("Foo", entityTypeConfiguration.GetTableName().Name);
        }

        [Fact]
        public void ToTable_overwrites_existing_name()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            entityTypeConfiguration.ToTable("Foo");
            entityTypeConfiguration.ToTable("Bar");

            Assert.Equal("Bar", entityTypeConfiguration.GetTableName().Name);
        }

        [Fact]
        public void Configure_should_configure_and_order_keys_when_keys_and_order_specified()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            entityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;

            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            var mockPropertyInfo2 = new MockPropertyInfo(typeof(int), "P2");
            entityTypeConfiguration.Key(mockPropertyInfo2);
            entityTypeConfiguration.Property(new PropertyPath(mockPropertyInfo2)).ColumnOrder = 1;
            entityType.GetDeclaredPrimitiveProperty("P2").SetClrPropertyInfo(mockPropertyInfo2);
            var mockPropertyInfo1 = new MockPropertyInfo(typeof(int), "P1");
            entityTypeConfiguration.Key(mockPropertyInfo1);
            entityTypeConfiguration.Property(new PropertyPath(mockPropertyInfo1)).ColumnOrder = 0;
            entityType.GetDeclaredPrimitiveProperty("P1").SetClrPropertyInfo(mockPropertyInfo1);

            entityTypeConfiguration.Configure(entityType, new EdmModel());

            Assert.Equal(2, entityType.DeclaredKeyProperties.Count);
            Assert.Equal("P1", entityType.DeclaredKeyProperties.First().Name);
        }

        [Fact]
        public void Configure_should_throw_when_key_properties_and_not_root_type()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E",
                                     BaseType = new EdmEntityType()
                                 };
            entityType.BaseType.SetClrType(typeof(string));
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            entityTypeConfiguration.Key(new MockPropertyInfo(typeof(int), "Id"));

            Assert.Equal(
                Strings.KeyRegisteredOnDerivedType(typeof(object), typeof(string)),
                Assert.Throws<InvalidOperationException>(() => entityTypeConfiguration.Configure(entityType, new EdmModel())).Message);
        }

        [Fact]
        public void Configure_should_throw_when_key_property_not_found()
        {
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "Id");
            entityTypeConfiguration.Key(mockPropertyInfo);

            Assert.Equal(
                Strings.KeyPropertyNotFound(("Id"), "E"),
                Assert.Throws<InvalidOperationException>(() => entityTypeConfiguration.Configure(entityType, new EdmModel())).Message);
        }

        [Fact]
        public void AddMappingConfiguration_multiple_mapping_fragments_for_same_table_should_throw()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(new MockType("E1"));
            var entityMappingConfiguration1 =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1Table")
                    };
            entityTypeConfiguration.AddMappingConfiguration(entityMappingConfiguration1);

            Assert.Equal(
                Strings.InvalidTableMapping("E1", "E1Table"), Assert.Throws<InvalidOperationException>(
                    () => entityTypeConfiguration
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          TableName = new DatabaseName("E1Table")
                                      })).Message);
        }

        [Fact]
        public void AddMappingConfiguration_multiple_mapping_fragments_with_no_table_name_throws()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(new MockType("E1"));
            var entityMappingConfiguration1 =
                new EntityMappingConfiguration();
            entityTypeConfiguration.AddMappingConfiguration(entityMappingConfiguration1);

            Assert.Equal(
                Strings.InvalidTableMapping_NoTableName("E1"), Assert.Throws<InvalidOperationException>(
                    () => entityTypeConfiguration
                              .AddMappingConfiguration(
                                  new EntityMappingConfiguration
                                      {
                                          TableName = new DatabaseName("E1Table")
                                      })).Message);
        }

        [Fact]
        public void AddMappingConfiguration_multiple_mapping_fragments_for_different_tables_allowed()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(new MockType("E1"));
            var entityMappingConfiguration1 =
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1Table")
                    };
            entityTypeConfiguration.AddMappingConfiguration(entityMappingConfiguration1);

            entityTypeConfiguration.AddMappingConfiguration(
                new EntityMappingConfiguration
                    {
                        TableName = new DatabaseName("E1TableExtended")
                    });
        }
    }
}
