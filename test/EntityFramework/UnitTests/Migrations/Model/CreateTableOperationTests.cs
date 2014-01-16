// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class CreateTableOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new CreateTableOperation(null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new CreateTableOperation(null, null)).Message);
        }

        [Fact]
        public void Name_should_return_name_from_ctor()
        {
            Assert.Equal("Foo", new CreateTableOperation("Foo").Name);
        }

        [Fact]
        public void Annotations_should_return_set_annotations()
        {
            Assert.Empty(new CreateTableOperation("Foo").Annotations);
            Assert.Empty(new CreateTableOperation("Foo", null).Annotations);

            var operation = new CreateTableOperation("Foo", new Dictionary<string, object> { { "A1", "V1" } });

            Assert.Equal("V1", operation.Annotations["A1"]);
        }

        [Fact]
        public void Can_add_and_enumerate_columns()
        {
            var createTableOperation = new CreateTableOperation("Foo");
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                    {
                        Name = "Bar",
                        IsNullable = true
                    });

            Assert.NotNull(createTableOperation.Columns.Single());
        }

        [Fact]
        public void Inverse_should_produce_drop_table_operation()
        {
            var operation = new CreateTableOperation(
                "Foo", new Dictionary<string, object>
                {
                    { "AT1", "VT1" },
                    { "AT2", "VT2" }
                });

            operation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                {
                    Name = "C1",
                    Annotations = new Dictionary<string, AnnotationValues>
                    {
                        { "AC1A", new AnnotationValues(null, "VC1A") },
                        { "AC1B", new AnnotationValues(null, "VC1B") }
                    }
                });

            operation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                {
                    Name = "C2",
                    Annotations = new Dictionary<string, AnnotationValues>
                    {
                        { "AC2A", new AnnotationValues(null, "VC2A") },
                        { "AC2B", new AnnotationValues(null, "VC2B") }
                    }
                });

            var inverse = (DropTableOperation)operation.Inverse;

            Assert.Equal("Foo", inverse.Name);

            Assert.Equal("VT1", inverse.RemovedAnnotations["AT1"]);
            Assert.Equal("VT2", inverse.RemovedAnnotations["AT2"]);

            Assert.Equal("VC1A", inverse.RemovedColumnAnnotations["C1"]["AC1A"]);
            Assert.Equal("VC1B", inverse.RemovedColumnAnnotations["C1"]["AC1B"]);
            Assert.Equal("VC2A", inverse.RemovedColumnAnnotations["C2"]["AC2A"]);
            Assert.Equal("VC2B", inverse.RemovedColumnAnnotations["C2"]["AC2B"]);
        }

        [Fact]
        public void Can_set_and_get_primary_key()
        {
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation();
            var table = new CreateTableOperation("T")
                            {
                                PrimaryKey = addPrimaryKeyOperation
                            };

            Assert.Same(addPrimaryKeyOperation, table.PrimaryKey);
            Assert.Equal("T", addPrimaryKeyOperation.Table);
        }
    }
}
