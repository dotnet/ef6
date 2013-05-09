// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;    
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;    
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public sealed class EntityMappingConfigurationTests
    {
        [Fact]
        public void TableName_can_get_and_set()
        {
            var entityMappingConfiguration = new EntityMappingConfiguration
                                                 {
                                                     TableName = new DatabaseName("Foo")
                                                 };

            Assert.Equal("Foo", entityMappingConfiguration.TableName.Name);
        }

        [Fact]
        public void Configure_should_update_table_name_when_base_type_is_null()
        {
            var entityMappingConfiguration
                = new EntityMappingConfiguration
                      {
                          TableName = new DatabaseName("Foo")
                      };

            var entityTypeMapping = new StorageEntityTypeMapping(null);

            entityTypeMapping.AddType(new EntityType("E", "N", DataSpace.CSpace));

            var databaseMapping =
                new DbDatabaseMapping().Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var table = databaseMapping.Database.AddTable("foo");
            var entitySet = databaseMapping.Database.GetEntitySet(table);

            entityTypeMapping.AddFragment(new StorageMappingFragment(entitySet, entityTypeMapping, false));
            
            entityMappingConfiguration.Configure(
                databaseMapping, ProviderRegistry.Sql2008_ProviderManifest, entityTypeMapping.EntityType, ref entityTypeMapping, false, 0, 1);

            Assert.Equal("Foo", table.GetTableName().Name);
        }

        [Fact]
        public void AddValueCondition_overwrites_existing_value_for_a_discriminator()
        {
            var entityMappingConfiguration1 = new EntityMappingConfiguration();
            entityMappingConfiguration1
                .AddValueCondition(
                    new ValueConditionConfiguration(entityMappingConfiguration1, "disc")
                        {
                            Value = 1
                        });
            entityMappingConfiguration1
                .AddValueCondition(
                    new ValueConditionConfiguration(entityMappingConfiguration1, "disc")
                        {
                            Value = 2
                        });

            Assert.Equal(2, entityMappingConfiguration1.ValueConditions.First().Value);
        }

        private struct MyStruct
        {
        }

        private class MyEntity
        {
            public MyStruct StructProperty { get; set; }
            public MyStruct? NullableStructProperty { get; set; }
            public DbGeometry DbGeometryProperty { get; set; }
            public DbGeography DbGeographyProperty { get; set; }
            public string StringProperty { get; set; }
            public byte[] ByteArrayProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public decimal? NullableDecimalProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public DateTime? NullableDateTimeProperty { get; set; }
            public DateTimeOffset DateTimeOffsetProperty { get; set; }
            public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }
            public TimeSpan TimeSpanProperty { get; set; }
            public TimeSpan? NullableTimeSpanProperty { get; set; }
        }

        [Fact]
        public static void Property_is_added_only_once_if_does_not_exist()
        {
            var entityMappingConfigurationOfMyEntity = new EntityMappingConfiguration<MyEntity>();
            var entityMappingConfiguration = entityMappingConfigurationOfMyEntity.EntityMappingConfigurationInstance;
            Expression<Func<MyEntity, string>> expression1 = e => e.StringProperty;
            Expression<Func<MyEntity, decimal>> expression2 = e => e.DecimalProperty;

            Assert.Null(entityMappingConfiguration.Properties);

            entityMappingConfigurationOfMyEntity.Property(expression1);

            Assert.NotNull(entityMappingConfiguration.Properties);
            Assert.Equal(1, entityMappingConfiguration.Properties.Count);
            Assert.Equal(expression1.GetComplexPropertyAccess(), entityMappingConfiguration.Properties[0]);

            entityMappingConfigurationOfMyEntity.Property(expression2);

            Assert.Equal(2, entityMappingConfiguration.Properties.Count);
            Assert.Equal(expression1.GetComplexPropertyAccess(), entityMappingConfiguration.Properties[0]);
            Assert.Equal(expression2.GetComplexPropertyAccess(), entityMappingConfiguration.Properties[1]);

            entityMappingConfigurationOfMyEntity.Property(expression1);

            Assert.Equal(2, entityMappingConfiguration.Properties.Count);
            Assert.Equal(expression1.GetComplexPropertyAccess(), entityMappingConfiguration.Properties[0]);
            Assert.Equal(expression2.GetComplexPropertyAccess(), entityMappingConfiguration.Properties[1]);
        }

        [Fact]
        public static void Property_mapping_configuration_is_created_only_once_if_does_not_exist()
        {
            var entityMappingConfigurationOfMyEntity = new EntityMappingConfiguration<MyEntity>();
            var entityMappingConfiguration = entityMappingConfigurationOfMyEntity.EntityMappingConfigurationInstance;
            Expression<Func<MyEntity, string>> expression = e => e.StringProperty;

            Assert.Null(entityMappingConfiguration.Properties);

            var configuration1 = entityMappingConfigurationOfMyEntity.Property(expression);

            Assert.Equal(1, entityMappingConfiguration.PrimitivePropertyConfigurations.Count);

            var configuration2 = entityMappingConfigurationOfMyEntity.Property(expression);

            Assert.Equal(1, entityMappingConfiguration.PrimitivePropertyConfigurations.Count);
            Assert.Same(configuration1.Configuration, configuration2.Configuration);
        }

        [Fact]
        public static void Property_mapping_configurations_are_created_and_stored_correctly()
        {
            var entityMappingConfiguration = new EntityMappingConfiguration<MyEntity>();
            var primitivePropertyConfigurations = 
                entityMappingConfiguration.EntityMappingConfigurationInstance.PrimitivePropertyConfigurations;

            PropertyMappingConfiguration configuration;

            Expression<Func<MyEntity, MyStruct>> expression1 = e => e.StructProperty;
            configuration = entityMappingConfiguration.Property(expression1);            
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression1.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, MyStruct?>> expression2 = e => e.NullableStructProperty;
            configuration = entityMappingConfiguration.Property(expression2);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression2.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DbGeometry>> expression3 = e => e.DbGeometryProperty;
            configuration = entityMappingConfiguration.Property(expression3);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression3.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DbGeography>> expression4 = e => e.DbGeographyProperty;
            configuration = entityMappingConfiguration.Property(expression4);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression4.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, string>> expression5 = e => e.StringProperty;
            configuration = entityMappingConfiguration.Property(expression5);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression5.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, byte[]>> expression6 = e => e.ByteArrayProperty;
            configuration = entityMappingConfiguration.Property(expression6);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression6.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, decimal>> expression7 = e => e.DecimalProperty;
            configuration = entityMappingConfiguration.Property(expression7);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression7.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, decimal?>> expression8 = e => e.NullableDecimalProperty;
            configuration = entityMappingConfiguration.Property(expression8);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression8.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DateTime>> expression9 = e => e.DateTimeProperty;
            configuration = entityMappingConfiguration.Property(expression9);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression9.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DateTime?>> expression10 = e => e.NullableDateTimeProperty;
            configuration = entityMappingConfiguration.Property(expression10);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression10.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DateTimeOffset>> expression11 = e => e.DateTimeOffsetProperty;
            configuration = entityMappingConfiguration.Property(expression11);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression11.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, DateTimeOffset?>> expression12 = e => e.NullableDateTimeOffsetProperty;
            configuration = entityMappingConfiguration.Property(expression12);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression12.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, TimeSpan>> expression13 = e => e.TimeSpanProperty;
            configuration = entityMappingConfiguration.Property(expression13);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression13.GetComplexPropertyAccess()]);

            Expression<Func<MyEntity, TimeSpan?>> expression14 = e => e.NullableTimeSpanProperty;
            configuration = entityMappingConfiguration.Property(expression14);
            Assert.Same(configuration.Configuration, 
                primitivePropertyConfigurations[expression14.GetComplexPropertyAccess()]);

            Assert.Equal(14, primitivePropertyConfigurations.Count);
        }
    }
}
