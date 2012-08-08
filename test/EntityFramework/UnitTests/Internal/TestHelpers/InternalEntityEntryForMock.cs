// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using Moq;

    internal class InternalEntityEntryForMock<TEntity> : InternalEntityEntry
        where TEntity : class, new()
    {
        public InternalEntityEntryForMock()
            : base(new Mock<InternalContextForMock>{CallBase = true}.Object, MockHelper.CreateMockStateEntry<TEntity>().Object)
        {
        }
    }
}
