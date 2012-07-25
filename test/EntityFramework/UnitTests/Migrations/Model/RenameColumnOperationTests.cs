// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class RenameColumnOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message, Assert.Throws<ArgumentException>(() => new RenameColumnOperation(null, null, null)).Message);

            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message, Assert.Throws<ArgumentException>(() => new RenameColumnOperation("T", null, null)).Message);

            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("newName")).Message, Assert.Throws<ArgumentException>(() => new RenameColumnOperation("T", "N", null)).Message);
        }

        [Fact]
        public void Can_get_and_set_rename_properties()
        {
            var renameColumnOperation = new RenameColumnOperation("T", "N", "N'");

            Assert.Equal("T", renameColumnOperation.Table);
            Assert.Equal("N", renameColumnOperation.Name);
            Assert.Equal("N'", renameColumnOperation.NewName);
        }

        [Fact]
        public void Inverse_should_produce_rename_column_operation()
        {
            var renameColumnOperation = new RenameColumnOperation("T", "N", "N'");

            var inverse = (RenameColumnOperation)renameColumnOperation.Inverse;

            Assert.Equal("T", inverse.Table);
            Assert.Equal("N'", inverse.Name);
            Assert.Equal("N", inverse.NewName);
        }
    }
}