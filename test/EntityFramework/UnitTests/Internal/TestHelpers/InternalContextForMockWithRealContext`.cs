// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using Moq;

    /// <summary>
    ///     This class uses a real DbContext so that GetType on it doesn't return the mock type.
    /// </summary>
    /// <typeparam name="TContext"> The type of the context. </typeparam>
    internal abstract class InternalContextForMockWithRealContext<TContext>
        : LazyInternalContext
        where TContext : DbContextUsingMockInternalContext, new()
    {
        protected InternalContextForMockWithRealContext()
            : base(new TContext(), InternalConnection(typeof(TContext).Name), null)
        {
            ((DbContextUsingMockInternalContext)Owner).MockedInternalContext = this;
        }

        private static IInternalConnection InternalConnection(string name)
        {
            var mock = new Mock<IInternalConnection>();
            mock.Setup(c => c.ConnectionKey).Returns(name);
            return mock.Object;
        }

        protected override void InitializeContext()
        {
        }
    }
}
