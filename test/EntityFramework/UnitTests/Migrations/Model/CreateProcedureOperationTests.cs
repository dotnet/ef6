// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class CreateProcedureOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new CreateProcedureOperation(null, "foo")).Message);
        }

        [Fact]
        public void Name_should_return_name_from_ctor()
        {
            Assert.Equal("Foo", new CreateProcedureOperation("Foo", "Bar").Name);
        }

        [Fact]
        public void Can_add_and_enumerate_parameters()
        {
            var createProcedureOperation = new CreateProcedureOperation("Foo", "Bar");

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
                = new CreateProcedureOperation("Foo", "Bar");

            var dropProcedureOperation = (DropProcedureOperation)createProcedureOperation.Inverse;

            Assert.Equal("Foo", dropProcedureOperation.Name);
        }
    }
}
