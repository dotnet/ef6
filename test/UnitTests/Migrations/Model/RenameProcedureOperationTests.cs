// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class RenameProcedureOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new RenameProcedureOperation(null, null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("newName")).Message,
                Assert.Throws<ArgumentException>(() => new RenameProcedureOperation("N", null)).Message);
        }

        [Fact]
        public void Can_get_and_set_rename_properties()
        {
            var renameProcedureOperation = new RenameProcedureOperation("N", "N'");

            Assert.Equal("N", renameProcedureOperation.Name);
            Assert.Equal("N'", renameProcedureOperation.NewName);
        }

        [Fact]
        public void Inverse_should_produce_rename_procedure_operation()
        {
            var renameProcedureOperation
                = new RenameProcedureOperation("dbo.Foo", "dbo.Bar");

            var inverse = (RenameProcedureOperation)renameProcedureOperation.Inverse;

            Assert.Equal("dbo.Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);

            renameProcedureOperation = new RenameProcedureOperation("dbo.Foo", "Bar");

            inverse = (RenameProcedureOperation)renameProcedureOperation.Inverse;

            Assert.Equal("dbo.Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);

            renameProcedureOperation = new RenameProcedureOperation("Foo", "Bar");

            inverse = (RenameProcedureOperation)renameProcedureOperation.Inverse;

            Assert.Equal("Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);
        }
    }
}
