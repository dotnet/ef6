// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class MigratorLoggingDecoratorTests : DbTestCase
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("innerMigrator", Assert.Throws<ArgumentNullException>(() => new MigratorLoggingDecorator(null, null)).ParamName);
            Assert.Equal(
                "logger", Assert.Throws<ArgumentNullException>(() => new MigratorLoggingDecorator(new DbMigrator(), null)).ParamName);
        }

        [Fact]
        public void ExecuteSql_dispatches_to_interceptors()
        {
            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(2013);

            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCommand.Object);

            var mockTransaction = new Mock<DbTransaction>(MockBehavior.Strict);
            mockTransaction.Protected().Setup<DbConnection>("DbConnection").Returns(mockConnection.Object);

            var migrator = new DbMigrator();
            var statement = new MigrationStatement
            {
                Sql = "Some Sql"
            };

            var providerFactoryServiceMock = new Mock<IDbProviderFactoryResolver>();
            providerFactoryServiceMock.Setup(m => m.ResolveProviderFactory(It.IsAny<DbConnection>()))
                .Returns(FakeSqlProviderFactory.Instance);
            MutableResolver.AddResolver<IDbProviderFactoryResolver>(k => providerFactoryServiceMock.Object);
            var mockInterceptor = new Mock<DbCommandInterceptor> { CallBase = true };
            DbInterception.Add(mockInterceptor.Object);
            var transactionInterceptorMock = new Mock<IDbTransactionInterceptor>();
            DbInterception.Add(transactionInterceptorMock.Object);
            try
            {
                new MigratorLoggingDecorator(migrator, new Mock<MigrationsLogger>().Object)
                    .ExecuteSql(statement, mockConnection.Object, mockTransaction.Object, new DbInterceptionContext());
            }
            finally
            {
                MutableResolver.ClearResolvers();
                DbInterception.Remove(mockInterceptor.Object);
                DbInterception.Remove(transactionInterceptorMock.Object);
            }

            mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Once());
            mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, It.IsAny<DbCommandInterceptionContext<int>>()), Times.Once());

            transactionInterceptorMock.Verify(
                m => m.ConnectionGetting(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<DbConnection>>()),
                Times.Never());
            transactionInterceptorMock.Verify(
                m => m.ConnectionGot(It.IsAny<DbTransaction>(), It.IsAny<DbTransactionInterceptionContext<DbConnection>>()),
                Times.Never());
            mockTransaction.Protected().Verify<DbConnection>("DbConnection", Times.Never());
        }
    }
}
