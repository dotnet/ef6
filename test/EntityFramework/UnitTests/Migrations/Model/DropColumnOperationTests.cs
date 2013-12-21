// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DropColumnOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column()
        {
            var dropColumnOperation = new DropColumnOperation("T", "c");

            Assert.Equal("T", dropColumnOperation.Table);
            Assert.Equal("c", dropColumnOperation.Name);
            Assert.Empty(dropColumnOperation.RemovedAnnotations);
            Assert.Null(dropColumnOperation.Inverse);

            dropColumnOperation = new DropColumnOperation("T", "c", (IDictionary<string, object>)null);

            Assert.Equal("T", dropColumnOperation.Table);
            Assert.Equal("c", dropColumnOperation.Name);
            Assert.Empty(dropColumnOperation.RemovedAnnotations);
            Assert.Null(dropColumnOperation.Inverse);

            dropColumnOperation = new DropColumnOperation("T", "c", (AddColumnOperation)null);

            Assert.Equal("T", dropColumnOperation.Table);
            Assert.Equal("c", dropColumnOperation.Name);
            Assert.Empty(dropColumnOperation.RemovedAnnotations);
            Assert.Null(dropColumnOperation.Inverse);

            dropColumnOperation = new DropColumnOperation("T", "c", null, null);

            Assert.Equal("T", dropColumnOperation.Table);
            Assert.Equal("c", dropColumnOperation.Name);
            Assert.Empty(dropColumnOperation.RemovedAnnotations);
            Assert.Null(dropColumnOperation.Inverse);
        }

        [Fact]
        public void Can_get_set_set_annotations()
        {
            Assert.Empty(new CreateTableOperation("Foo").Annotations);
            Assert.Empty(new CreateTableOperation("Foo", null).Annotations);

            var operation = new DropColumnOperation("T", "c", new Dictionary<string, object> { { "A1", "V1" } });

            Assert.Equal("V1", operation.RemovedAnnotations["A1"]);

            operation = new DropColumnOperation("T", "c", new Dictionary<string, object> { { "A1", "V1" } }, null);

            Assert.Equal("V1", operation.RemovedAnnotations["A1"]);
        }

        [Fact]
        public void Inverse_should_return_given_AddColumnOperation()
        {
            var inverse = new AddColumnOperation("T", new ColumnModel(PrimitiveTypeKind.Binary));
            var dropColumnOperation = new DropColumnOperation("T", "c", inverse);

            Assert.Same(inverse, dropColumnOperation.Inverse);

            dropColumnOperation = new DropColumnOperation("T", "c", null, inverse);

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

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation(null, "c", (IDictionary<string, object>)null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation("t", null, (IDictionary<string, object>)null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation(null, "c", (AddColumnOperation)null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation("t", null, (AddColumnOperation)null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation(null, "c", null, null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropColumnOperation("t", null, null, null)).Message);
        }
    }
}
