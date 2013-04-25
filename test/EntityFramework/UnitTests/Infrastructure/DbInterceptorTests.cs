// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DbInterceptorTests : TestBase
    {
        [Fact]
        public void DbInterceptor_methods_return_result_given()
        {
            var commandTree = new Mock<DbCommandTree>().Object;
            Assert.Same(commandTree, new DbInterceptor().TreeCreated(commandTree, new DbInterceptionContext()));

            var command = new Mock<DbCommand>().Object;

            Assert.Equal(27, new DbInterceptor().NonQueryExecuted(command, 27, new DbInterceptionContext()));

            var random = new Random();
            Assert.Same(random, new DbInterceptor().ScalarExecuted(command, random, new DbInterceptionContext()));

            var reader = new Mock<DbDataReader>().Object;
            Assert.Same(reader, new DbInterceptor().ReaderExecuted(command, CommandBehavior.Default, reader, new DbInterceptionContext()));

            var intTask = new Task<int>(() => 0);
            Assert.Same(intTask, new DbInterceptor().AsyncNonQueryExecuted(command, intTask, new DbInterceptionContext()));

            var randomTask = new Task<object>(() => new Random());
            Assert.Same(
                randomTask,
                new DbInterceptor().AsyncScalarExecuted(command, randomTask, new DbInterceptionContext()));

            var readerTask = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);
            Assert.Same(
                readerTask,
                new DbInterceptor().AsyncReaderExecuted(command, CommandBehavior.Default, readerTask, new DbInterceptionContext()));
        }
    }
}
