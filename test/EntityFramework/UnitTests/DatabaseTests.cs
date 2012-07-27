// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DatabaseTests : TestBase
    {
        public class Exists
        {
            [Fact]
            public void With_null_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists((string)null)).Message);
            }

            [Fact]
            public void With_empty_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists("")).Message);
            }

            [Fact]
            public void With_whitespace_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Exists(" ")).Message);
            }

            [Fact]
            public void With_null_existingConnection_throws()
            {
                Assert.Equal(
                    "existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Exists((DbConnection)null)).ParamName);
            }
        }

        public class Delete
        {
            [Fact]
            public void With_null_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete((string)null)).Message);
            }

            [Fact]
            public void With_empty_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete("")).Message);
            }

            [Fact]
            public void With_whitespace_nameOrConnectionString_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                    Assert.Throws<ArgumentException>(() => Database.Delete(" ")).Message);
            }

            [Fact]
            public void With_null_existingConnection_throws()
            {
                Assert.Equal(
                    "existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Delete((DbConnection)null)).ParamName);
            }
        }

        public class ExecuteSqlCommand
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommand(" ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => database.ExecuteSqlCommand("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var internalContextMock = new Mock<InternalContextForMock>();
                var database = new Database(internalContextMock.Object);

                Assert.NotNull(database.ExecuteSqlCommand("query"));
                internalContextMock.Verify(m => m.ExecuteSqlCommand("query", new object[0]), Times.Once());
            }
        }

        public class ExecuteSqlCommandAsync
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync(null).Result).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync("").Result).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.ExecuteSqlCommandAsync(" ").Result).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.ExecuteSqlCommandAsync("query", null).Result).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var internalContextMock = new Mock<InternalContextForMock>();
                internalContextMock.Setup(
                    m => m.ExecuteSqlCommandAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<object[]>()))
                    .Returns(Task.FromResult(1));
                var database = new Database(internalContextMock.Object);

                Assert.NotNull(database.ExecuteSqlCommandAsync("query").Result);
                internalContextMock.Verify(
                    m => m.ExecuteSqlCommandAsync("query", CancellationToken.None, It.IsAny<object[]>()), Times.Once());
            }
        }

        public class SqlQuery_Generic
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery<Random>(" ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery<Random>("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_dont_throw()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.NotNull(database.SqlQuery<Random>("query"));
            }
        }

        public class SqlQuery_NonGeneric
        {
            [Fact]
            public void With_null_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), null)).Message);
            }

            [Fact]
            public void With_empty_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), "")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("sql"),
                    Assert.Throws<ArgumentException>(() => database.SqlQuery(typeof(Random), " ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "parameters",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery(typeof(Random), "query", null)).ParamName);
            }

            [Fact]
            public void With_null_type_throws()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.Equal(
                    "elementType",
                    Assert.Throws<ArgumentNullException>(() => database.SqlQuery(null, "query")).ParamName);
            }

            [Fact]
            public void With_valid_arguments_dont_throw()
            {
                var database = new Database(new Mock<InternalContextForMock>().Object);

                Assert.NotNull(database.SqlQuery(typeof(Random), "query"));
            }
        }
    }
}
