// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class CreateModificationFunctionsOperation : MigrationOperation
    {
        private readonly StorageEntityTypeModificationFunctionMapping _modificationFunctionMapping;

        private readonly ICollection<DbInsertCommandTree> _insertCommandTrees;
        private readonly ICollection<DbUpdateCommandTree> _updateCommandTrees;
        private readonly ICollection<DbDeleteCommandTree> _deleteCommandTrees;

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateModificationFunctionsOperation(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");

            _modificationFunctionMapping = modificationFunctionMapping;
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateModificationFunctionsOperation(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            ICollection<DbInsertCommandTree> insertCommandTrees,
            ICollection<DbUpdateCommandTree> updateCommandTrees,
            ICollection<DbDeleteCommandTree> deleteCommandTrees,
            object anonymousArguments = null)
            : this(modificationFunctionMapping, anonymousArguments)
        {
            Check.NotNull(insertCommandTrees, "insertCommandTrees");
            Check.NotNull(insertCommandTrees, "updateCommandTrees");
            Check.NotNull(insertCommandTrees, "deleteCommandTrees");

            _modificationFunctionMapping = modificationFunctionMapping;

            _insertCommandTrees = insertCommandTrees;
            _updateCommandTrees = updateCommandTrees;
            _deleteCommandTrees = deleteCommandTrees;
        }

        public StorageEntityTypeModificationFunctionMapping ModificationFunctionMapping
        {
            get { return _modificationFunctionMapping; }
        }

        public ICollection<DbInsertCommandTree> InsertCommandTrees
        {
            get { return _insertCommandTrees; }
        }

        public ICollection<DbUpdateCommandTree> UpdateCommandTrees
        {
            get { return _updateCommandTrees; }
        }

        public ICollection<DbDeleteCommandTree> DeleteCommandTrees
        {
            get { return _deleteCommandTrees; }
        }

        public override MigrationOperation Inverse
        {
            get { return new DropModificationFunctionsOperation(_modificationFunctionMapping); }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
