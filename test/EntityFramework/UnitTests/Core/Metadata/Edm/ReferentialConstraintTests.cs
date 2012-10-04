// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class ReferentialConstraintTests
    {
        [Fact]
        public void Can_set_and_get_dependent_end()
        {
            var dependentEnd1 = new AssociationEndMember("D", new EntityType());

            var referentialConstraint
                = new ReferentialConstraint(
                    new AssociationEndMember("P", new EntityType()),
                    dependentEnd1,
                    new[] { EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) },
                    new[] { EdmProperty.Primitive("D", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) });

            Assert.Same(dependentEnd1, referentialConstraint.DependentEnd);

            var dependentEnd2 = new AssociationEndMember("D2", new EntityType());

            referentialConstraint.DependentEnd = dependentEnd2;

            Assert.Same(dependentEnd2, referentialConstraint.DependentEnd);
        }
    }
}
