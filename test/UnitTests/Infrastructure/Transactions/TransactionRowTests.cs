// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Transactions
{
    using Xunit;

    public class TransactionRowTests
    {
        [Fact]
        public void Equals_returns_correct_results()
        {
            var id = Guid.NewGuid();
            Assert.True(new TransactionRow().Equals(new TransactionRow()));
            Assert.True(
                new TransactionRow { Id = id, CreationTime = DateTime.Now }.Equals(
                    new TransactionRow { Id = id, CreationTime = DateTime.Now + TimeSpan.FromHours(1) }));
            Assert.False(
                new TransactionRow { Id = Guid.NewGuid() }.Equals(new TransactionRow { Id = Guid.NewGuid() }));
            Assert.False(new TransactionRow().Equals(new object()));
        }

        [Fact]
        public void GetHashCode_returns_correct_results()
        {
            var id = Guid.NewGuid();
            Assert.Equal(
                new TransactionRow().GetHashCode(),
                new TransactionRow().GetHashCode());
            Assert.Equal(
                new TransactionRow { Id = id, CreationTime = DateTime.Now }.GetHashCode(),
                new TransactionRow { Id = id, CreationTime = DateTime.Now + TimeSpan.FromHours(1) }.GetHashCode());
            Assert.NotEqual(
                new TransactionRow { Id = Guid.NewGuid() }.GetHashCode(),
                new TransactionRow { Id = Guid.NewGuid() }.GetHashCode());
        }
    }
}
