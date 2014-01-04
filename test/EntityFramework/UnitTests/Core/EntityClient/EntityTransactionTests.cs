// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using Moq;
    using Xunit;

    public class EntityTransactionTests
    {
        [Fact]
        public void Commit_wraps_exceptions()
        {
            var transactionMock = new Mock<DbTransaction>();
            transactionMock.Setup(m => m.Commit()).Throws<NotImplementedException>();
            var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);

            Assert.Throws<EntityException>(() => transaction.Commit());
        }

        [Fact]
        public void Commit_does_not_wrap_CommitFailedException()
        {
            var transactionMock = new Mock<DbTransaction>();
            transactionMock.Setup(m => m.Commit()).Throws<CommitFailedException>();
            var transaction = new EntityTransaction(new EntityConnection(), transactionMock.Object);

            Assert.Throws<CommitFailedException>(() => transaction.Commit());
        }
    }
}
