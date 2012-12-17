// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class NavigationPropertyTests
    {
        [Fact]
        public void Nullability_updated_when_property_goes_readonly()
        {
            var navigationProperty 
                = new NavigationProperty("N", TypeUsage.Create(new EntityType()))
                                         {
                                             ToEndMember =
                                                 new AssociationEndMember(
                                                 "T", new RefType(new EntityType()), RelationshipMultiplicity.ZeroOrOne)
                                         };

            Assert.Equal(true, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);

            navigationProperty.ToEndMember.RelationshipMultiplicity = RelationshipMultiplicity.One;

            Assert.Equal(true, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);

            navigationProperty.SetReadOnly();

            Assert.Equal(false, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);
        }
    }
}
