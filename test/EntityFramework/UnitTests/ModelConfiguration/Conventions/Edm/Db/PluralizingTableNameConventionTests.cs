// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using Xunit;

    public sealed class PluralizingTableNameConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_pluralized_table_name_as_identitier()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var entitySet = database.AddEntitySet("ES", table);
            entitySet.Table = "Customer";

            (new PluralizingTableNameConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal("Customers", entitySet.Table);
        }

        [Fact]
        public void Apply_should_ignored_configured_tables()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            table.SetTableName(new DatabaseName("Foo"));
            var entitySet = database.AddEntitySet("ES", table);
            entitySet.Table = "Customer";

            (new PluralizingTableNameConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal("Customer", entitySet.Table);
            Assert.Equal("Foo", table.GetTableName().Name);
        }

        [Fact]
        public void Apply_should_ignore_current_table()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var entitySet = database.AddEntitySet("ES", table);
            entitySet.Table = "Customers";

            (new PluralizingTableNameConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal("Customers", entitySet.Table);
        }

        [Fact]
        public void Apply_should_uniquify_names()
        {
            var database = new EdmModel(DataSpace.CSpace);
            var tableA = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var tableB = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var entitySetA = database.AddEntitySet("ESA", tableA);
            entitySetA.Table = "Customers";
            var entitySetB = database.AddEntitySet("ESB", tableB);
            entitySetB.Table = "Customer";

            (new PluralizingTableNameConvention()).Apply(tableB, new DbModel(null, database));

            Assert.Equal("Customers1", entitySetB.Table);
        }
    }
}
