// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using Moq;
    using Xunit;

    public class DbCommandTreeDispatcherTests : TestBase
    {
        [Fact]
        public void Created_dispatches_to_interceptors_which_can_modify_result()
        {
            var interceptionContext = new DbInterceptionContext();
            var tree = new Mock<DbCommandTree>().Object;

            var mockInterceptor = new Mock<IDbCommandTreeInterceptor>();
            var interceptedTree = new Mock<DbCommandTree>().Object;
            mockInterceptor.Setup(m => m.TreeCreated(It.IsAny<DbCommandTreeInterceptionContext>()))
                .Callback<DbCommandTreeInterceptionContext>(
                    i =>
                        {
                            Assert.Same(tree, i.Result);
                            Assert.Same(tree, i.OriginalResult);
                            i.Result = interceptedTree;
                        });

            var dispatcher = new DbCommandTreeDispatcher();
            var internalDispatcher = dispatcher.InternalDispatcher;
            internalDispatcher.Add(mockInterceptor.Object);

            Assert.Same(interceptedTree, dispatcher.Created(tree, interceptionContext));

            mockInterceptor.Verify(m => m.TreeCreated(It.IsAny<DbCommandTreeInterceptionContext>()));
        }

        [Fact]
        public void Created_returns_tree_if_no_interceptors_are_registered()
        {
            var tree = new Mock<DbCommandTree>().Object;
            Assert.Same(tree, new DbCommandTreeDispatcher().Created(tree, new DbCommandTreeInterceptionContext()));
        }
    }
}
