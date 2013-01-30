// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using Xunit;

    public class DbContextExtensionsTests
    {
        [Fact]
        public void Should_be_able_to_get_model_from_context()
        {
            var context = new ShopContext_v1();

            var edmX = context.GetModel();

            Assert.NotNull(edmX);
        }
    }
}
