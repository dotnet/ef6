// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using Moq;

    /// <summary>
    /// A derived InternalSet implementation that exposes a parameterless constructor
    /// that creates a mocked underlying InternalContext such that the internal set can
    /// be mocked.
    /// </summary>
    internal class InternalSetForMock<TEntity> : InternalSet<TEntity>
        where TEntity : class
    {
        protected InternalSetForMock()
            : base(new Mock<InternalContextForMock>().Object)
        {
        }
    }
}
