// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using Xunit;

    public sealed class EdmAssociationEndExtensionsTests
    {
        [Fact]
        public void IsMany_should_return_true_when_end_kind_is_many()
        {
            var associationEnd = new EdmAssociationEnd { EndKind = EdmAssociationEndKind.Many };

            Assert.True(associationEnd.IsMany());
        }

        [Fact]
        public void IsOptional_should_return_true_when_end_kind_is_optional()
        {
            var associationEnd = new EdmAssociationEnd { EndKind = EdmAssociationEndKind.Optional };

            Assert.True(associationEnd.IsOptional());
        }

        [Fact]
        public void IsRequired_should_return_true_when_end_kind_is_required()
        {
            var associationEnd = new EdmAssociationEnd { EndKind = EdmAssociationEndKind.Required };

            Assert.True(associationEnd.IsRequired());
        }
    }
}