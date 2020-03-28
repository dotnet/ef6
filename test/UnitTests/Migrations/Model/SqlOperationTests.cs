// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class SqlOperationTests
    {
        [Fact]
        public void Can_get_and_set_sql()
        {
            var sqlOperation = new SqlOperation("foo");

            Assert.Equal("foo", sqlOperation.Sql);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("sql")).Message,
                Assert.Throws<ArgumentException>(() => new SqlOperation(null)).Message);
        }
    }
}
