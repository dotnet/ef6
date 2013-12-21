// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DropTableOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_annotations()
        {
            var operation = new DropTableOperation("T");

            Assert.Equal("T", operation.Name);
            Assert.Empty(operation.RemovedAnnotations);
            Assert.Empty(operation.RemovedColumnAnnotations);
            Assert.Null(operation.Inverse);

            operation = new DropTableOperation("T", null);
            Assert.Equal("T", operation.Name);
            Assert.Empty(operation.RemovedAnnotations);
            Assert.Empty(operation.RemovedColumnAnnotations);
            Assert.Null(operation.Inverse);

            operation = new DropTableOperation("T", null, null, null);
            Assert.Equal("T", operation.Name);
            Assert.Empty(operation.RemovedAnnotations);
            Assert.Empty(operation.RemovedColumnAnnotations);
            Assert.Null(operation.Inverse);

            operation = new DropTableOperation(
                "T", new Dictionary<string, object> { { "AT1", "VT1" } },
                new Dictionary<string, IDictionary<string, object>> { { "C1", new Dictionary<string, object> { { "AC1", "VC1" } } } });

            Assert.Equal("T", operation.Name);
            Assert.Equal("VT1", operation.RemovedAnnotations["AT1"]);
            Assert.Equal("VC1", operation.RemovedColumnAnnotations["C1"]["AC1"]);
            Assert.Null(operation.Inverse);


            operation = new DropTableOperation(
                "T", new Dictionary<string, object> { { "AT1", "VT1" } },
                new Dictionary<string, IDictionary<string, object>> { { "C1", new Dictionary<string, object> { { "AC1", "VC1" } } } }, null);

            Assert.Equal("T", operation.Name);
            Assert.Equal("VT1", operation.RemovedAnnotations["AT1"]);
            Assert.Equal("VC1", operation.RemovedColumnAnnotations["C1"]["AC1"]);
            Assert.Null(operation.Inverse);
        }

        [Fact]
        public void Inverse_should_return_set_create_table_operation()
        {
            var inverse = new CreateTableOperation("T");
            var operation = new DropTableOperation("T", inverse);

            Assert.Equal("T", operation.Name);
            Assert.Same(inverse, operation.Inverse);

            operation = new DropTableOperation("T", null, null, inverse);

            Assert.Equal("T", operation.Name);
            Assert.Same(inverse, operation.Inverse);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropTableOperation(null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropTableOperation(null, null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropTableOperation(null, null, null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropTableOperation(null, null, null, null)).Message);
        }
    }
}
