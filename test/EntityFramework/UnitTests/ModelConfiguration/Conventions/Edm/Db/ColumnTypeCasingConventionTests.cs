// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm.Db;
    using Xunit;

    public sealed class ColumnTypeCasingConventionTests
    {
        [Fact]
        public void Apply_should_put_key_columns_first()
        {
            var tableColumn = new DbTableColumnMetadata { TypeName = "Foo" };

            ((IDbConvention<DbTableColumnMetadata>)new ColumnTypeCasingConvention()).Apply(tableColumn, new DbDatabaseMetadata());

            Assert.Equal("foo", tableColumn.TypeName);
        }
    }
}