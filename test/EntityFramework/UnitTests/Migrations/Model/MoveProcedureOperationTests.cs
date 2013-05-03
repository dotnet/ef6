// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class MoveProcedureOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new MoveProcedureOperation(null, null)).Message);
        }

        [Fact]
        public void Can_get_and_set_name_properties()
        {
            var moveProcedureOperation = new MoveProcedureOperation("dbo.Customers", "crm");

            Assert.Equal("dbo.Customers", moveProcedureOperation.Name);
            Assert.Equal("crm", moveProcedureOperation.NewSchema);
        }

        [Fact]
        public void Inverse_should_produce_move_procedure_operation()
        {
            var moveProcedureOperation
                = new MoveProcedureOperation("dbo.MyCustomers", "crm");

            var inverse = (MoveProcedureOperation)moveProcedureOperation.Inverse;

            Assert.Equal("crm.MyCustomers", inverse.Name);
            Assert.Equal("dbo", inverse.NewSchema);
        }
    }
}
