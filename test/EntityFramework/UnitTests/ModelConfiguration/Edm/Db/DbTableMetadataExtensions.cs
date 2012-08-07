// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using Xunit;

    public sealed class DbTableMetadataExtensions
    {
        [Fact]
        public void AddColumn_should_set_properties_and_add_to_columns()
        {
            var table = new DbTableMetadata();

            var tableColumn = table.AddColumn("Foo");

            Assert.NotNull(tableColumn);
            Assert.Equal("Foo", tableColumn.Name);
            Assert.True(table.Columns.Contains(tableColumn));
        }
    }
}
