// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class AlterColumnOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column_info()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Boolean);
            var alterColumnOperation = new AlterColumnOperation("T", column, isDestructiveChange: false);

            Assert.Equal("T", alterColumnOperation.Table);
            Assert.Same(column, alterColumnOperation.Column);
        }

        [Fact]
        public void Inverse_should_produce_change_column_operation()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Boolean);
            var inverse = new AlterColumnOperation("T", column, isDestructiveChange: false);
            var alterColumnOperation = new AlterColumnOperation("T", column, isDestructiveChange: false, inverse: inverse);

            Assert.Same(inverse, alterColumnOperation.Inverse);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(
                    () => new AlterColumnOperation(null, new ColumnModel(PrimitiveTypeKind.Boolean), isDestructiveChange: false)).Message);

            Assert.Equal(
                "column",
                Assert.Throws<ArgumentNullException>(() => new AlterColumnOperation("T", null, isDestructiveChange: false)).ParamName);
        }
    }
}
