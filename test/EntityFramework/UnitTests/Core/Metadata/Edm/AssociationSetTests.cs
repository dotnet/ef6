// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class AssociationSetTests
    {
        [Fact]
        public void Can_get_and_set_ends_via_wrapper_properties()
        {
            var associationType
                = new AssociationType
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType()),
                          TargetEnd = new AssociationEndMember("T", new EntityType())
                      };

            var associationSet = new AssociationSet("A", associationType);

            Assert.Null(associationSet.SourceSet);
            Assert.Null(associationSet.TargetSet);

            var sourceEntitySet = new EntitySet();

            associationSet.SourceSet = sourceEntitySet;

            var targetEntitySet = new EntitySet();

            associationSet.TargetSet = targetEntitySet;

            Assert.Same(sourceEntitySet, associationSet.SourceSet);
            Assert.Same(targetEntitySet, associationSet.TargetSet);
        }
    }
}
