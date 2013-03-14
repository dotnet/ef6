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
    public sealed class StorageEntityTypeModificationFunctionMapping
    {
        internal StorageEntityTypeModificationFunctionMapping(
            EntityType entityType,
            StorageModificationFunctionMapping deleteFunctionMapping,
            StorageModificationFunctionMapping insertFunctionMapping,
            StorageModificationFunctionMapping updateFunctionMapping)
        {
            DebugCheck.NotNull(entityType);

            EntityType = entityType;
            DeleteFunctionMapping = deleteFunctionMapping;
            InsertFunctionMapping = insertFunctionMapping;
            UpdateFunctionMapping = updateFunctionMapping;
        }

        /// <summary>
        ///     Gets (specific) entity type these functions handle.
        /// </summary>
        internal readonly EntityType EntityType;

        /// <summary>
        ///     Gets delete function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping DeleteFunctionMapping;

        /// <summary>
        ///     Gets insert function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping InsertFunctionMapping;

        /// <summary>
        ///     Gets update function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping UpdateFunctionMapping;

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
