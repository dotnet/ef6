// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class InterceptableDbCommandTests
    {
        [Fact]
        public void ExecuteNonQuery_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var mockInterception = new Mock<Interception>();

            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, mockInterception.Object);

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(false);

            interceptableDbCommand.ExecuteNonQuery();

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Never());

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(true);

            interceptableDbCommand.ExecuteNonQuery();

            mockCommand.Verify(m => m.ExecuteNonQuery(), Times.Once());
        }

        [Fact]
        public void ExecuteScalar_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var mockInterception = new Mock<Interception>();

            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, mockInterception.Object);

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(false);

            interceptableDbCommand.ExecuteScalar();

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Never());

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(true);

            interceptableDbCommand.ExecuteScalar();

            mockCommand.Verify(m => m.ExecuteScalar(), Times.Once());
        }

        [Fact]
        public void ExecuteReader_should_dispatch_to_interceptor_and_optionally_execute()
        {
            var mockCommand = new Mock<DbCommand>();
            var mockInterception = new Mock<Interception>();

            var interceptableDbCommand = new InterceptableDbCommand(mockCommand.Object, mockInterception.Object);

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(false);

            var reader = interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            Assert.True(reader.NextResult());
            Assert.False(reader.NextResult());

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Never(), CommandBehavior.SingleRow);

            mockInterception.Setup(m => m.Dispatch(mockCommand.Object)).Returns(true);

            interceptableDbCommand.ExecuteReader(CommandBehavior.SingleRow);

            mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SingleRow);
        }
    }
}
