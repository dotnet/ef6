// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Mapping;
    using Xunit;

    public class DropModificationFunctionsOperationTests
    {
        [Fact]
        public void Can_initialize_mapping()
        {
            var storageEntityTypeModificationFunctionMapping = new StorageEntityTypeModificationFunctionMapping();

            var dropModificationFunctionsOperation
                = new DropModificationFunctionsOperation(storageEntityTypeModificationFunctionMapping);

            Assert.Same(storageEntityTypeModificationFunctionMapping, dropModificationFunctionsOperation.ModificationFunctionMapping);
        }

        [Fact]
        public void Inverse_should_produce_create_modification_functions_operation()
        {
            var storageEntityTypeModificationFunctionMapping = new StorageEntityTypeModificationFunctionMapping();

            var dropModificationFunctionsOperation
                = new DropModificationFunctionsOperation(storageEntityTypeModificationFunctionMapping);

            var createModificationFunctionsOperation
                = (CreateModificationFunctionsOperation)dropModificationFunctionsOperation.Inverse;

            Assert.Same(storageEntityTypeModificationFunctionMapping, createModificationFunctionsOperation.ModificationFunctionMapping);
        }
    }
}
