// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class RelationshipMultiplicityExtensionsTests
    {
        [Fact]
        public void IsX_should_return_true_when_end_kind_is_X()
        {
            Assert.True(RelationshipMultiplicity.One.IsRequired());
            Assert.True(RelationshipMultiplicity.ZeroOrOne.IsOptional());
            Assert.True(RelationshipMultiplicity.Many.IsMany());
        }
    }
}
