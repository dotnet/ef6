// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class RenameTableOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("name")).Message,
                Assert.Throws<ArgumentException>(() => new RenameTableOperation(null, null)).Message);

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("newName")).Message,
                Assert.Throws<ArgumentException>(() => new RenameTableOperation("N", null)).Message);
        }

        [Fact]
        public void Can_get_and_set_rename_properties()
        {
            var renameTableOperation = new RenameTableOperation("N", "N'");

            Assert.Equal("N", renameTableOperation.Name);
            Assert.Equal("N'", renameTableOperation.NewName);
        }

        [Fact]
        public void Inverse_should_produce_rename_column_operation()
        {
            var renameTableOperation = new RenameTableOperation("dbo.Foo", "dbo.Bar");

            var inverse = (RenameTableOperation)renameTableOperation.Inverse;

            Assert.Equal("dbo.Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);

            renameTableOperation = new RenameTableOperation("dbo.Foo", "Bar");

            inverse = (RenameTableOperation)renameTableOperation.Inverse;

            Assert.Equal("dbo.Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);

            renameTableOperation = new RenameTableOperation("Foo", "Bar");

            inverse = (RenameTableOperation)renameTableOperation.Inverse;

            Assert.Equal("Bar", inverse.Name);
            Assert.Equal("Foo", inverse.NewName);
        }
    }
}
