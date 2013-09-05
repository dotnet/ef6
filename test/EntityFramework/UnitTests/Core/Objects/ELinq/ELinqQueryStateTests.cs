// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using Xunit;

    public class ELinqQueryStateTests
    {
        [Fact]
        public void GetIncludeMethod_returns_query_Include_MethodInfo()
        {
            Assert.NotNull(ELinqQueryState.GetIncludeMethod(new ObjectQuery<int>()));
        }
    }
}
