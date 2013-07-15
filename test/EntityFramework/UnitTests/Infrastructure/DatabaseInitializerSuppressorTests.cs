// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using SimpleModel;
    using Xunit;

    public class DatabaseInitializerSuppressorTests : TestBase
    {
        [Fact]
        public void Context_type_can_be_set_as_suppressed_and_then_later_unsuppressed()
        {
            var suppressor = new DatabaseInitializerSuppressor();

            Assert.False(suppressor.IsSuppressed(typeof(SimpleModelContext)));
            Assert.False(suppressor.IsSuppressed(typeof(SimpleLocalDbModelContext)));

            suppressor.Suppress(typeof(SimpleModelContext));

            Assert.True(suppressor.IsSuppressed(typeof(SimpleModelContext)));
            Assert.False(suppressor.IsSuppressed(typeof(SimpleLocalDbModelContext)));

            suppressor.Unsuppress(typeof(SimpleModelContext));

            Assert.False(suppressor.IsSuppressed(typeof(SimpleModelContext)));
            Assert.False(suppressor.IsSuppressed(typeof(SimpleLocalDbModelContext)));
        }
    }
}
