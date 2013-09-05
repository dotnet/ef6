// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using Xunit;

    public class SerializableImplementorTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(SerializableImplementor.AddValueMethod);
            Assert.NotNull(SerializableImplementor.GetValueMethod);
            Assert.NotNull(SerializableImplementor.GetTypeFromHandleMethod);
        }
    }
}
