// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EntitySetExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var entitySet = new EntitySet();

            entitySet.SetConfiguration(42);

            Assert.Equal(42, entitySet.GetConfiguration());
        }
    }
}
