// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using Xunit;

    public class DbModelExtensionsTests
    {
        [Fact]
        public void Should_be_able_to_get_xdocument_from_model()
        {
            var model = new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo);

            var edmX = model.GetModel();

            Assert.NotNull(edmX);
        }
    }
}
