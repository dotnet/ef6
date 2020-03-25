// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class FunctionImportEntityTypeMappingTests
    {
        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var entityType = new EntityType("ET", "N", DataSpace.CSpace);
            var propertyMapping = new FunctionImportReturnTypeScalarPropertyMapping("P", "C");
            var mappingCondition = new FunctionImportEntityTypeMappingConditionIsNull("P", true);
            var mapping
                = new FunctionImportEntityTypeMapping(
                    Enumerable.Empty<EntityType>(),
                    new[] { entityType },
                    new Collection<FunctionImportReturnTypePropertyMapping> { propertyMapping },
                    new[] { mappingCondition });

            Assert.False(propertyMapping.IsReadOnly);
            Assert.False(mappingCondition.IsReadOnly);
            mapping.SetReadOnly();
            Assert.True(propertyMapping.IsReadOnly);
            Assert.True(mappingCondition.IsReadOnly);
        }
    }
}
