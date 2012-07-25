// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Data.Entity.Internal;
    using Moq;

    /// <summary>
    /// Allows the mocked internal context to be returned from the real DbContext that is
    /// needed for tests that key on the context type
    /// </summary>
    public abstract class DbContextUsingMockInternalContext : DbContext
    {
        internal InternalContext MockedInternalContext { get; set; }

        internal override InternalContext InternalContext
        {
            get
            {
                return MockedInternalContext;
            }
        }
    }

    /// <summary>
    /// This class uses a real DbContext so that GetType on it doesn't return the mock type.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    internal abstract class InternalContextForMockWithRealContext<TContext>
        : LazyInternalContext where TContext : DbContextUsingMockInternalContext, new()
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