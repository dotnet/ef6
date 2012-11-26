// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    
    using Xunit;

    public sealed class DbAssociationSetMappingExtensionsTests
    {
        [Fact]
        public void Initialize_should_initialize_ends()
        {
            var associationSetMapping 
                = new StorageAssociationSetMapping(new AssociationSet("AS", new AssociationType()), new EntitySet()).Initialize();

            Assert.NotNull(associationSetMapping.SourceEndMapping);
            Assert.NotNull(associationSetMapping.TargetEndMapping);
        }

        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var associationSetMapping 
                = new StorageAssociationSetMapping(new AssociationSet("AS", new AssociationType()), new EntitySet());

            associationSetMapping.SetConfiguration(42);

            Assert.Equal(42, associationSetMapping.GetConfiguration());
        }
    }
}
