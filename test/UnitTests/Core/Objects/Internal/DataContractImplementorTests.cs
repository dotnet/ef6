// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Linq;
    using Xunit;

    public class DataContractImplementorTests
    {
        [Fact]
        public void Reflection_fields_are_initialized()
        {
            Assert.NotNull(DataContractImplementor.DataContractAttributeConstructor);
            Assert.NotNull(DataContractImplementor.DataContractProperties.Single());
        }
    }
}
