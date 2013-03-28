// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class DropModificationFunctionsOperation : MigrationOperation
    {
        private readonly StorageEntityTypeModificationFunctionMapping _modificationFunctionMapping;
        
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropModificationFunctionsOperation(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotNull(modificationFunctionMapping, "modificationFunctionMapping");

            _modificationFunctionMapping = modificationFunctionMapping;
        }

        public StorageEntityTypeModificationFunctionMapping ModificationFunctionMapping
        {
            get { return _modificationFunctionMapping; }
        }

        public override MigrationOperation Inverse
        {
            get { return new CreateModificationFunctionsOperation(_modificationFunctionMapping); }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
