// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class AddColumnOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column_info()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Decimal);
            var addColumnOperation = new AddColumnOperation("T", column);

            Assert.Equal("T", addColumnOperation.Table);
            Assert.Same(column, addColumnOperation.Column);
        }

        [Fact]
        public void Inverse_should_produce_drop_column_operation()
        {
            var column = new ColumnModel(PrimitiveTypeKind.Decimal)
                             {
                                 Name = "C"
                             };

            var addColumnOperation
                = new AddColumnOperation("T", column);

            var dropColumnOperation = (DropColumnOperation)addColumnOperation.Inverse;

            Assert.Equal("C", dropColumnOperation.Name);
            Assert.Equal("T", dropColumnOperation.Table);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(() => new AddColumnOperation(null, new ColumnModel(PrimitiveTypeKind.Time))).Message);

            Assert.Equal("column", Assert.Throws<ArgumentNullException>(() => new AddColumnOperation("T", null)).ParamName);
        }
    }
}
