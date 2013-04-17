// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class DbLocalViewTests
    {
        [Fact]
        public void Explicit_interface_methods_delegate_to_hidden_methods()
        {
            Verify_method(l => ((ICollection<object>)l).Remove(null), l => l.Remove(null));
            Verify_method(l => ((ICollection<object>)l).Contains(null), l => l.Contains(null));
            Verify_method(l => ((IList)l).Remove(null), l => l.Remove(null));
            Verify_method(l => ((IList)l).Contains(null), l => l.Contains(null));
        }

        private void Verify_method(Action<DbLocalView<object>> methodToCall, Expression<Func<DbLocalView<object>, bool>> methodToVerify)
        {
            var mockDbLocalView = new Mock<DbLocalView<object>>(new Mock<InternalContext>().Object);
            methodToCall(mockDbLocalView.Object);

            mockDbLocalView.Verify(methodToVerify, Times.Once());
        }

        public class Remove
        {
            [Fact]
            public void Removes_the_only_item()
            {
                var item = new object();
                var view = CreateDbLocalView(new[] { item });

                view.Remove(item);
                Assert.Equal(0, view.Count);
            }

            [Fact]
            public void Doesnt_remove_nonmatching_items()
            {
                var view = CreateDbLocalView(new[] { new object(), new object() });

                view.Remove(new object());
                Assert.Equal(2, view.Count);
            }

            [Fact]
            public void Does_nothing_for_empty_set()
            {
                var view = CreateDbLocalView(new object[] { });

                view.Remove(new object());
                Assert.Equal(0, view.Count);
            }
        }

        public class Contains
        {
            [Fact]
            public void Returns_true_for_the_only_item()
            {
                var item = new object();
                var view = CreateDbLocalView(new[] { item });

                Assert.True(view.Contains(item));
            }

            [Fact]
            public void Returns_false_for_nonmatching_items()
            {
                var view = CreateDbLocalView(new[] { new object(), new object() });

                Assert.False(view.Contains(new object()));
            }

            [Fact]
            public void Returns_false_for_empty_set()
            {
                var view = CreateDbLocalView(new object[] { });

                Assert.False(view.Contains(new object()));
            }
        }

        private static DbLocalView<T> CreateDbLocalView<T>(IEnumerable<T> initialItems)
            where T : class
        {
            var mockInternalContext = new Mock<InternalContext>();

            var internalSet = new Mock<InternalSetForMock<T>>();

            mockInternalContext.Setup(m => m.Set<T>()).Returns(new DbSet<T>(internalSet.Object));
            mockInternalContext.Setup(m => m.GetLocalEntities<T>()).Returns(initialItems);
            return new DbLocalView<T>(mockInternalContext.Object);
        }
    }
}
