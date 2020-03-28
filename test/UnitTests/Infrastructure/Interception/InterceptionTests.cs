// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using Xunit;

    public class InterceptionTests : TestBase
    {
        [Fact]
        public void Interception_add_and_remove_check_for_null_interceptors()
        {
            Assert.Equal(
                "interceptor",
                Assert.Throws<ArgumentNullException>(() => DbInterception.Add(null)).ParamName);

            Assert.Equal(
                "interceptor",
                Assert.Throws<ArgumentNullException>(() => DbInterception.Remove(null)).ParamName);
        }
    }
}
