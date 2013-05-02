// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
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

            Assert.Equal(27, new DbInterceptor().NonQueryExecuted(command, 27, new DbCommandInterceptionContext()));

            var random = new Random();
            Assert.Same(random, new DbInterceptor().ScalarExecuted(command, random, new DbCommandInterceptionContext()));

            var reader = new Mock<DbDataReader>().Object;
            Assert.Same(reader, new DbInterceptor().ReaderExecuted(command, reader, new DbCommandInterceptionContext()));
        }
    }
}
