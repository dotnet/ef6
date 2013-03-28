// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Mapping;
    using Xunit;

    public class CreateModificationFunctionsOperationTests
    {
        [Fact]
        public void Can_initialize_mapping_and_command_trees()
        {
            var storageEntityTypeModificationFunctionMapping = new StorageEntityTypeModificationFunctionMapping();

            var insertTrees = new[] { new DbInsertCommandTree() };
            var updateTrees = new[] { new DbUpdateCommandTree() };
            var deleteTrees = new[] { new DbDeleteCommandTree() };

            var createModificationFunctionsOperation
                = new CreateModificationFunctionsOperation(
                    storageEntityTypeModificationFunctionMapping, insertTrees, updateTrees, deleteTrees);

            Assert.Same(storageEntityTypeModificationFunctionMapping, createModificationFunctionsOperation.ModificationFunctionMapping);
            Assert.Same(insertTrees, createModificationFunctionsOperation.InsertCommandTrees);
            Assert.Same(updateTrees, createModificationFunctionsOperation.UpdateCommandTrees);
            Assert.Same(deleteTrees, createModificationFunctionsOperation.DeleteCommandTrees);
        }

        [Fact]
        public void Inverse_should_produce_drop_modification_functions_operation()
        {
            var storageEntityTypeModificationFunctionMapping = new StorageEntityTypeModificationFunctionMapping();

            var createModificationFunctionsOperation
                = new CreateModificationFunctionsOperation(storageEntityTypeModificationFunctionMapping);

            var dropModificationFunctionsOperation
                = (DropModificationFunctionsOperation)createModificationFunctionsOperation.Inverse;

            Assert.Same(storageEntityTypeModificationFunctionMapping, dropModificationFunctionsOperation.ModificationFunctionMapping);
        }
    }
}
