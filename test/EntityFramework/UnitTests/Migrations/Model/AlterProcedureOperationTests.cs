// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class AlterProcedureOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new AlterProcedureOperation(null, "foo")).Message);
        }

        [Fact]
        public void Name_should_return_name_from_ctor()
        {
            Assert.Equal("Foo", new AlterProcedureOperation("Foo", "Bar").Name);
        }

        [Fact]
        public void Can_add_and_enumerate_parameters()
        {
            var createProcedureOperation = new AlterProcedureOperation("Foo", "Bar");

            createProcedureOperation.Parameters.Add(
                new ParameterModel(PrimitiveTypeKind.Int64)
                    {
                        Name = "Bar"
                    });

            Assert.NotNull(createProcedureOperation.Parameters.Single());
        }

        [Fact]
        public void Inverse_should_produce_drop_table_operation()
        {
            var createProcedureOperation
                = new AlterProcedureOperation("Foo", "Bar");

            Assert.IsType<NotSupportedOperation>(createProcedureOperation.Inverse);
        }
    }
}
