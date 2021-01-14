// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class StorageAssociationSetMappingExtensionsTests
    {
        [Fact]
        public void Initialize_should_initialize_ends()
        {
            var associationSetMapping 
                = new AssociationSetMapping(new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), new EntitySet()).Initialize();

            Assert.NotNull(associationSetMapping.SourceEndMapping);
            Assert.NotNull(associationSetMapping.TargetEndMapping);
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var associationSetMapping 
                = new AssociationSetMapping(new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), new EntitySet());

            associationSetMapping.SetConfiguration(42);

            Assert.Equal(42, associationSetMapping.GetConfiguration());
        }
    }
}
