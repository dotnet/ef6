// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class RelationshipEndMemberTests
    {
        [Fact]
        public void Can_set_and_get_relationship_multiplicity()
        {
            var relationshipEndMember = new AssociationEndMember("E", new EntityType());

            Assert.Equal(default(RelationshipMultiplicity), relationshipEndMember.RelationshipMultiplicity);

            relationshipEndMember.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            Assert.Equal(RelationshipMultiplicity.Many, relationshipEndMember.RelationshipMultiplicity);
        }
    }
}
