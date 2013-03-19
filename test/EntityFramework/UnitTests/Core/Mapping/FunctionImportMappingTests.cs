// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class FunctionImportMappingTests
    {
        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_store_function()
        {
            Assert.Equal(
                "functionImport",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingFake(null, new EdmFunction("f", "n", DataSpace.SSpace))).ParamName);
        }

        [Fact]
        public void Can_not_create_composable_function_import_mapping_with_null_function_import()
        {
            Assert.Equal(
                "targetFunction",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportMappingFake(new EdmFunction("f", "n", DataSpace.CSpace), null)).ParamName);
        }

        [Fact]
        public void Can_get_function_import_and_store_function()
        {
            var functionImport = 
                new EdmFunction("f", "entityModel", DataSpace.CSpace);
            var storeFunction =
                new EdmFunction("f", "storeModel", DataSpace.SSpace);

            var functionImporMapping = new FunctionImportMappingFake(functionImport, storeFunction);

            Assert.Same(functionImport, functionImporMapping.FunctionImport);
            Assert.Same(storeFunction, functionImporMapping.TargetFunction);
        }

        private class FunctionImportMappingFake : FunctionImportMapping
        {
            public FunctionImportMappingFake(EdmFunction functionImport, EdmFunction targetFunction)
                : base(functionImport, targetFunction)
            {
            }
        }

    }
}
