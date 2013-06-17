// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Describes modification function mappings for an entity type within an entity set.
    /// </summary>
    internal sealed class StorageEntityTypeModificationFunctionMapping
    {
        private readonly StorageModificationFunctionMapping _deleteFunctionMapping;
        private readonly StorageModificationFunctionMapping _insertFunctionMapping;
        private readonly StorageModificationFunctionMapping _updateFunctionMapping;

        internal StorageEntityTypeModificationFunctionMapping()
        {
            // Testing
        }

        internal StorageEntityTypeModificationFunctionMapping(
            EntityType entityType,
            StorageModificationFunctionMapping deleteFunctionMapping,
            StorageModificationFunctionMapping insertFunctionMapping,
            StorageModificationFunctionMapping updateFunctionMapping)
        {
            DebugCheck.NotNull(entityType);

            EntityType = entityType;
            _deleteFunctionMapping = deleteFunctionMapping;
            _insertFunctionMapping = insertFunctionMapping;
            _updateFunctionMapping = updateFunctionMapping;
        }

        /// <summary>
        ///     Gets (specific) entity type these functions handle.
        /// </summary>
        internal readonly EntityType EntityType;

        /// <summary>
        ///     Gets delete function for the current entity type.
        /// </summary>
        public StorageModificationFunctionMapping DeleteFunctionMapping
        {
            get { return _deleteFunctionMapping; }
        }

        /// <summary>
        ///     Gets insert function for the current entity type.
        /// </summary>
        public StorageModificationFunctionMapping InsertFunctionMapping
        {
            get { return _insertFunctionMapping; }
        }

        /// <summary>
        ///     Gets update function for the current entity type.
        /// </summary>
        public StorageModificationFunctionMapping UpdateFunctionMapping
        {
            get { return _updateFunctionMapping; }
        }

        internal IEnumerable<StorageModificationFunctionParameterBinding> PrimaryParameterBindings
        {
            get
            {
                var result = Enumerable.Empty<StorageModificationFunctionParameterBinding>();

                if (DeleteFunctionMapping != null)
                {
                    result = result.Concat(DeleteFunctionMapping.ParameterBindings);
                }

                if (InsertFunctionMapping != null)
                {
                    result = result.Concat(InsertFunctionMapping.ParameterBindings);
                }

                if (UpdateFunctionMapping != null)
                {
                    result = result.Concat(UpdateFunctionMapping.ParameterBindings.Where(pb => pb.IsCurrent));
                }

                return result;
            }
        }

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping,
                InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
        }
    }
}
