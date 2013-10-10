// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using Xunit;

    public static class QueryTestHelpers
    {
        public static void VerifyQueryResult<TOuter, TInner>(
            IList<TOuter> outer,
            IList<TInner> inner,
            Func<TOuter, TInner, bool> assertFunc)
        {
            Assert.Equal(outer.Count, inner.Count);
            for (int i = 0; i < outer.Count; i++)
            {
                Assert.True(assertFunc(outer[i], inner[i]));
            }
        }
    }
}
