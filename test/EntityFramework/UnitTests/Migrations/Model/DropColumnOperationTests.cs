// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class DropColumnOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column()
        {
            var dropColumnOperation = new DropColumnOperation("T", "c");

            Assert.Equal("T", dropColumnOperation.Table);
            Assert.Equal("c", dropColumnOperation.Name);
        }

        [Fact]
        public void Inverse_should_produce_drop_column_operation()
        {
            var inverse = new AddColumnOperation("T", new ColumnModel(PrimitiveTypeKind.Binary));
            var dropColumnOperation = new DropColumnOperation("T", "c", inverse);

            Assert.Same(inverse, dropColumnOperation.Inverse);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation(null, "c")).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation("t", null)).Message);
        }
    }
}
