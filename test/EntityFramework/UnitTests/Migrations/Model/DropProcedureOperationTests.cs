// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DropProcedureOperationTests
    {
        [Fact]
        public void Can_get_and_set_procedure()
        {
            var dropTableOperation = new DropProcedureOperation("T");

            Assert.Equal("T", dropTableOperation.Name);
        }

        [Fact]
        public void Inverse_should_produce_not_supported_operation()
        {
            var dropTableOperation = new DropProcedureOperation("T");

            Assert.Same(NotSupportedOperation.Instance, dropTableOperation.Inverse);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new DropProcedureOperation(null)).Message);
        }
    }
}
