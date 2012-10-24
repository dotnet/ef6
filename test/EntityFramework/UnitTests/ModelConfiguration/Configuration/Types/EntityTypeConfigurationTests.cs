// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using Moq;
    using Xunit;

    public sealed class EntityTypeConfigurationTests
    {
        [Fact]
        public void Configure_should_set_configuration()
        {
            var entityType = new EntityType
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
            var entityType = new EntityType
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
            var entityType = new EntityType
                                 {
                                     Name = "E"
                                 };
            var property1 = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
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
            var entityType = new EntityType
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
        public void TableName_returns_current_TableName()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            Assert.Equal(null, entityTypeConfiguration.TableName);

            entityTypeConfiguration.ToTable("Foo");
            Assert.Equal("Foo", entityTypeConfiguration.TableName);
        }

        [Fact]
        public void SchemaName_returns_current_SchemaName()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));

            Assert.Equal(null, entityTypeConfiguration.SchemaName);

            entityTypeConfiguration.ToTable("Foo", "Bar");
            Assert.Equal("Bar", entityTypeConfiguration.SchemaName);
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
            var entityType = new EntityType
                                 {
                                     Name = "E"
                                 };
            var property = EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var property1 = EdmProperty.Primitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);

            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            var mockPropertyInfo2 = new MockPropertyInfo(typeof(int), "P2");
            entityTypeConfiguration.Key(mockPropertyInfo2);
            entityTypeConfiguration.Property(new PropertyPath(mockPropertyInfo2)).ColumnOrder = 1;
            (entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P2")).SetClrPropertyInfo(mockPropertyInfo2);
            var mockPropertyInfo1 = new MockPropertyInfo(typeof(int), "P1");
            entityTypeConfiguration.Key(mockPropertyInfo1);
            entityTypeConfiguration.Property(new PropertyPath(mockPropertyInfo1)).ColumnOrder = 0;
            (entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == "P1")).SetClrPropertyInfo(mockPropertyInfo1);

            entityTypeConfiguration.Configure(entityType, new EdmModel());

            Assert.Equal(2, entityType.DeclaredKeyProperties.Count);
            Assert.Equal("P1", entityType.DeclaredKeyProperties.First().Name);
        }

        [Fact]
        public void Configure_should_throw_when_key_properties_and_not_root_type()
        {
            var entityType = new EntityType
                                 {
                                     Name = "E",
                                     BaseType = new EntityType()
                                 };
            var type = typeof(string);
            var tempQualifier = entityType.BaseType;

            tempQualifier.Annotations.SetClrType(type);
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(object));
            entityTypeConfiguration.Key(new MockPropertyInfo(typeof(int), "Id"));

            Assert.Equal(
                Strings.KeyRegisteredOnDerivedType(typeof(object), typeof(string)),
                Assert.Throws<InvalidOperationException>(() => entityTypeConfiguration.Configure(entityType, new EdmModel())).Message);
        }

        [Fact]
        public void Configure_should_throw_when_key_property_not_found()
        {
            var entityType = new EntityType
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
