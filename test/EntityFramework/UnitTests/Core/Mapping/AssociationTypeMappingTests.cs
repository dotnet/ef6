// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class AssociationTypeMappingTests
    {
        [Fact]
        public void Can_get_association_type()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);

            var setMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Same(
                associationType,
                new AssociationTypeMapping(associationType, setMapping).AssociationType);
        }

        [Fact]
        public void Association_type_returned_in_type_collection()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);

            var setMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Same(
                associationType,
                new AssociationTypeMapping(associationType, setMapping).Types.Single());
        }

        [Fact]
        public void IsOfType_collection_empty()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);

            var setMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Empty(new AssociationTypeMapping(associationType, setMapping).IsOfTypes);
        }
    }
}
