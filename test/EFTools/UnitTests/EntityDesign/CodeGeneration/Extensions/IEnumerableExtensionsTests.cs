// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Collections;
    using System.Collections.Generic;
    using Moq;
    using Xunit;

    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void MoreThan_returns_true_when_more_and_collection()
        {
            // NOTE: Using Strict ensures that the collection is not enumerated
            var collection = new Mock<ICollection<int>>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(3);

            Assert.True(collection.Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_equal_and_collection()
        {
            var collection = new Mock<ICollection<int>>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(2);

            Assert.False(collection.Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_less_and_collection()
        {
            var collection = new Mock<ICollection<int>>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(1);

            Assert.False(collection.Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_true_when_more_and_nongeneric_collection()
        {
            var collection = new Mock<ICollection>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(3);

            Assert.True(collection.As<IEnumerable<int>>().Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_equal_and_nongeneric_collection()
        {
            var collection = new Mock<ICollection>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(2);

            Assert.False(collection.As<IEnumerable<int>>().Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_less_and_nongeneric_collection()
        {
            var collection = new Mock<ICollection>(MockBehavior.Strict);
            collection.SetupGet(c => c.Count).Returns(1);

            Assert.False(collection.As<IEnumerable<int>>().Object.MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_true_when_more_and_enumerable()
        {
            Assert.True(GetValues(3).MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_equal_and_enumerable()
        {
            Assert.False(GetValues(2).MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_less_and_enumerable()
        {
            Assert.False(GetValues(1).MoreThan(2));
        }

        [Fact]
        public void MoreThan_returns_false_when_empty_enumerable()
        {
            Assert.False(GetValues(0).MoreThan(2));
        }

        private static IEnumerable<int> GetValues(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return i;
            }
        }
    }
}
