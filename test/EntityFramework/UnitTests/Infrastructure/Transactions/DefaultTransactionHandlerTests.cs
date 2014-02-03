// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Transactions
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DefaultTransactionHandlerTests
    {
        [Fact]
        public void BuildDatabaseInitializationScript_returns_empty()
        {
            Assert.Empty(new DefaultTransactionHandler().BuildDatabaseInitializationScript());
        }

        [Fact]
        public void Commited_wraps_exceptions_for_known_transactions()
        {
            var context = Core.Objects.MockHelper.CreateMockObjectContext<object>();
            var handler = new DefaultTransactionHandler();
            handler.Initialize(context);
            var mockTransaction = new Mock<DbTransaction>().Object;

            var beginTransactionInterceptionContext = new BeginTransactionInterceptionContext();
            beginTransactionInterceptionContext.Result = mockTransaction;

            handler.BeganTransaction(handler.Connection, beginTransactionInterceptionContext);

            var interceptionContext = new DbTransactionInterceptionContext().WithConnection(handler.Connection);
            interceptionContext.Exception = new Exception();
            handler.Committed(mockTransaction, interceptionContext);

            Assert.IsType<CommitFailedException>(interceptionContext.Exception);
        }

        [Fact]
        public void Committed_does_not_wrap_exception_if_unknown_connection()
        {
            var context = Core.Objects.MockHelper.CreateMockObjectContext<object>();
            var handler = new DefaultTransactionHandler();
            handler.Initialize(context);
            var mockTransaction = new Mock<DbTransaction>().Object;

            var interceptionContext = new DbTransactionInterceptionContext().WithConnection(new Mock<DbConnection>().Object);
            interceptionContext.Exception = new Exception();
            handler.Committed(mockTransaction, interceptionContext);

            Assert.IsNotType<CommitFailedException>(interceptionContext.Exception);
        }

        [Fact]
        public void BeganTransaction_does_not_record_transaction_if_connection_does_not_match()
        {
            var context = Core.Objects.MockHelper.CreateMockObjectContext<object>();
            var handler = new DefaultTransactionHandler();
            handler.Initialize(context);
            var mockTransaction = new Mock<DbTransaction>().Object;

            var beginTransactionInterceptionContext = new BeginTransactionInterceptionContext();
            beginTransactionInterceptionContext.Result = mockTransaction;

            handler.BeganTransaction(new Mock<DbConnection>().Object, beginTransactionInterceptionContext);

            var interceptionContext = new DbTransactionInterceptionContext();
            interceptionContext.Exception = new Exception();
            handler.Committed(mockTransaction, interceptionContext);

            Assert.IsNotType<CommitFailedException>(interceptionContext.Exception);
        }
    }
}
