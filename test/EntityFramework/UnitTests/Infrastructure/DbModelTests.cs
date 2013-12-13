// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class DbModelTests
    {
        [Fact]
        public void Can_retrieve_entity_container_mapping()
        {
            var model = new DbModel(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var containerMapping = new EntityContainerMapping();

            Assert.Null(model.ConceptualToStoreMapping);

            model.DatabaseMapping.AddEntityContainerMapping(containerMapping);

            Assert.Same(containerMapping, model.ConceptualToStoreMapping);
        }
    }
}
