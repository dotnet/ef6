// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class AlterTableOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_annotations()
        {
            var operation = new AlterTableOperation("T", null);

            Assert.Equal("T", operation.Name);
            Assert.Empty(operation.Annotations);

            operation = new AlterTableOperation(
                "T", new Dictionary<string, AnnotationValues> { { "AT1", new AnnotationValues("VT1", "VT2") } });

            Assert.Equal("T", operation.Name);
            Assert.Equal("VT1", operation.Annotations["AT1"].OldValue);
            Assert.Equal("VT2", operation.Annotations["AT1"].NewValue);
        }

        [Fact]
        public void Inverse_should_produce_inverse_alter_operation()
        {
            var operation = new AlterTableOperation(
                "T", new Dictionary<string, AnnotationValues>
                {
                    { "AT1", new AnnotationValues("VT1A", "VT1B") },
                    { "AT2", new AnnotationValues("VT2A", "VT2B") }
                });

            operation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                {
                    Name = "C1",
                    Annotations = new Dictionary<string, AnnotationValues>
                    {
                        { "AC1A", new AnnotationValues("VC1A1", "VC1A2") },
                        { "AC1B", new AnnotationValues("VC1B1", "VC1B2") }
                    }
                });

            operation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int64)
                {
                    Name = "C2",
                    Annotations = new Dictionary<string, AnnotationValues>
                    {
                        { "AC2A", new AnnotationValues("VC2A1", "VC2A2") },
                        { "AC2B", new AnnotationValues("VC2B1", "VC2B2") }
                    }
                });

            var inverse = (AlterTableOperation)operation.Inverse;

            Assert.Equal("T", inverse.Name);

            Assert.Equal("VT1B", inverse.Annotations["AT1"].OldValue);
            Assert.Equal("VT1A", inverse.Annotations["AT1"].NewValue);
            Assert.Equal("VT2B", inverse.Annotations["AT2"].OldValue);
            Assert.Equal("VT2A", inverse.Annotations["AT2"].NewValue);

            Assert.Equal("VC1A1", inverse.Columns.Single(c => c.Name == "C1").Annotations["AC1A"].OldValue);
            Assert.Equal("VC1A2", inverse.Columns.Single(c => c.Name == "C1").Annotations["AC1A"].NewValue);
            Assert.Equal("VC1B1", inverse.Columns.Single(c => c.Name == "C1").Annotations["AC1B"].OldValue);
            Assert.Equal("VC1B2", inverse.Columns.Single(c => c.Name == "C1").Annotations["AC1B"].NewValue);
            Assert.Equal("VC2A1", inverse.Columns.Single(c => c.Name == "C2").Annotations["AC2A"].OldValue);
            Assert.Equal("VC2A2", inverse.Columns.Single(c => c.Name == "C2").Annotations["AC2A"].NewValue);
            Assert.Equal("VC2B1", inverse.Columns.Single(c => c.Name == "C2").Annotations["AC2B"].OldValue);
            Assert.Equal("VC2B2", inverse.Columns.Single(c => c.Name == "C2").Annotations["AC2B"].NewValue);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new AlterTableOperation(null, null)).Message);
        }
    }
}
