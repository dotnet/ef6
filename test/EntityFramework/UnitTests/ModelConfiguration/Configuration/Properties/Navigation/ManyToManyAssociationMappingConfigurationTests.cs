// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class ManyToManyAssociationMappingConfigurationTests
    {
        [Fact]
        public void Configure_should_rename_table_when_table_configured()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var table = database.AddTable("OriginalName");
            var associationSetMapping = new DbAssociationSetMapping().Initialize();
            associationSetMapping.Table = table;

            var manyToManyAssociationMappingConfiguration
                = new ManyToManyAssociationMappingConfiguration();

            manyToManyAssociationMappingConfiguration.ToTable("NewName");

            var mockPropertyInfo = new MockPropertyInfo();

            associationSetMapping.SourceEndMapping.AssociationEnd = new EdmAssociationEnd();
            associationSetMapping.SourceEndMapping.AssociationEnd.SetClrPropertyInfo(mockPropertyInfo);

            manyToManyAssociationMappingConfiguration.Configure(associationSetMapping, database, mockPropertyInfo);

            Assert.Equal("NewName", table.GetTableName().Name);
            Assert.Same(manyToManyAssociationMappingConfiguration, table.GetConfiguration());
        }

        [Fact]
        public void Configure_should_rename_columns_when_left_keys_configured()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var associationSetMapping = new DbAssociationSetMapping().Initialize();
            var column = new DbTableColumnMetadata();
            associationSetMapping.SourceEndMapping.PropertyMappings.Add(
                new DbEdmPropertyMapping
                    {
                        Column = column
                    });

            var manyToManyAssociationMappingConfiguration
                = new ManyToManyAssociationMappingConfiguration();

            manyToManyAssociationMappingConfiguration.MapLeftKey("NewName");

            var mockPropertyInfo = new MockPropertyInfo();

            associationSetMapping.SourceEndMapping.AssociationEnd = new EdmAssociationEnd();
            associationSetMapping.SourceEndMapping.AssociationEnd.SetClrPropertyInfo(mockPropertyInfo);

            manyToManyAssociationMappingConfiguration.Configure(associationSetMapping, database, mockPropertyInfo);

            Assert.Equal("NewName", column.Name);
        }

        [Fact]
        public void Configure_should_rename_columns_when_right_keys_configured()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var associationSetMapping = new DbAssociationSetMapping().Initialize();
            var column = new DbTableColumnMetadata();
            associationSetMapping.TargetEndMapping.PropertyMappings.Add(
                new DbEdmPropertyMapping
                    {
                        Column = column
                    });

            var manyToManyAssociationMappingConfiguration
                = new ManyToManyAssociationMappingConfiguration();

            manyToManyAssociationMappingConfiguration.MapRightKey("NewName");

            var mockPropertyInfo = new MockPropertyInfo();

            associationSetMapping.SourceEndMapping.AssociationEnd = new EdmAssociationEnd();
            associationSetMapping.SourceEndMapping.AssociationEnd.SetClrPropertyInfo(mockPropertyInfo);

            manyToManyAssociationMappingConfiguration.Configure(associationSetMapping, database, mockPropertyInfo);

            Assert.Equal("NewName", column.Name);
        }

        [Fact]
        public void Configure_should_throw_when_incorrect_number_of_columns_configured()
        {
            var database = new DbDatabaseMetadata().Initialize();
            var associationSetMapping = new DbAssociationSetMapping().Initialize();

            var manyToManyAssociationMappingConfiguration
                = new ManyToManyAssociationMappingConfiguration();

            manyToManyAssociationMappingConfiguration.MapLeftKey("Id1", "Id2");

            var mockPropertyInfo = new MockPropertyInfo();

            associationSetMapping.SourceEndMapping.AssociationEnd = new EdmAssociationEnd();
            associationSetMapping.SourceEndMapping.AssociationEnd.SetClrPropertyInfo(mockPropertyInfo);

            Assert.Equal(
                Strings.IncorrectColumnCount("Id1, Id2"),
                Assert.Throws<InvalidOperationException>(
                    () => manyToManyAssociationMappingConfiguration.Configure(associationSetMapping, database, mockPropertyInfo)).Message);
        }

        [Fact]
        public void Equals_should_return_true_when_table_name_and_columns_equal_but_left_and_right_swapped()
        {
            var manyToManyAssociationMappingConfiguration1 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("Foo");

            manyToManyAssociationMappingConfiguration1.MapLeftKey("Bar");
            manyToManyAssociationMappingConfiguration1.MapRightKey("Baz");

            var manyToManyAssociationMappingConfiguration2 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration2.ToTable("Foo");

            manyToManyAssociationMappingConfiguration2.MapLeftKey("Baz");
            manyToManyAssociationMappingConfiguration2.MapRightKey("Bar");

            Assert.Equal(manyToManyAssociationMappingConfiguration1, manyToManyAssociationMappingConfiguration2);
        }

        [Fact]
        public void Equals_should_return_true_when_table_name_and_columns_equal()
        {
            var manyToManyAssociationMappingConfiguration1 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("Foo");

            manyToManyAssociationMappingConfiguration1.MapLeftKey("Bar");
            manyToManyAssociationMappingConfiguration1.MapRightKey("Baz");

            var manyToManyAssociationMappingConfiguration2 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration2.ToTable("Foo");

            manyToManyAssociationMappingConfiguration2.MapLeftKey("Bar");
            manyToManyAssociationMappingConfiguration2.MapRightKey("Baz");

            Assert.Equal(manyToManyAssociationMappingConfiguration1, manyToManyAssociationMappingConfiguration2);
        }
    }
}
