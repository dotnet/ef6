// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using Xunit;

    public class InsertHistoryOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var model = new byte[] { 0xDA };

            var insertHistoryOperation
                = new InsertHistoryOperation("Foo", "Migration1", model);

            Assert.Equal("Foo", insertHistoryOperation.Table);
            Assert.Equal("Migration1", insertHistoryOperation.MigrationId);
            Assert.Same(model, insertHistoryOperation.Model);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("table")).Message, Assert.Throws<ArgumentException>(() => new InsertHistoryOperation(null, "Migration1", new byte[0])).Message);

            Assert.Equal(new ArgumentException(Strings.ArgumentIsNullOrWhitespace("migrationId")).Message, Assert.Throws<ArgumentException>(() => new InsertHistoryOperation("Foo", null, new byte[0])).Message);

            Assert.Equal("model", Assert.Throws<ArgumentNullException>(() => new InsertHistoryOperation("Foo", "Migration1", null)).ParamName);
        }
    }
}