// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using Xunit;

    public class PocoPropertyAccessorStrategyTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(PocoPropertyAccessorStrategy.AddToCollectionGeneric);
            Assert.NotNull(PocoPropertyAccessorStrategy.RemoveFromCollectionGeneric);
        }
    }
}
