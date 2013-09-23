// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Describes modification function mappings for an entity type within an entity set.
    /// </summary>
    public sealed class EntityTypeModificationFunctionMapping : MappingItem
    {
        private readonly ModificationFunctionMapping _deleteFunctionMapping;
        private readonly ModificationFunctionMapping _insertFunctionMapping;
        private readonly ModificationFunctionMapping _updateFunctionMapping;

        internal EntityTypeModificationFunctionMapping()
        {
            // Testing
        }

        internal EntityTypeModificationFunctionMapping(
            EntityType entityType,
            ModificationFunctionMapping deleteFunctionMapping,
            ModificationFunctionMapping insertFunctionMapping,
            ModificationFunctionMapping updateFunctionMapping)
        {
            DebugCheck.NotNull(entityType);

            EntityType = entityType;
            _deleteFunctionMapping = deleteFunctionMapping;
            _insertFunctionMapping = insertFunctionMapping;
            _updateFunctionMapping = updateFunctionMapping;
        }

        // <summary>
        // Gets (specific) entity type these functions handle.
        // </summary>
        internal readonly EntityType EntityType;

        /// <summary>
        /// Gets delete function for the current entity type.
        /// </summary>
        public ModificationFunctionMapping DeleteFunctionMapping
        {
            get { return _deleteFunctionMapping; }
        }

        /// <summary>
        /// Gets insert function for the current entity type.
        /// </summary>
        public ModificationFunctionMapping InsertFunctionMapping
        {
            get { return _insertFunctionMapping; }
        }

        /// <summary>
        /// Gets update function for the current entity type.
        /// </summary>
        public ModificationFunctionMapping UpdateFunctionMapping
        {
            get { return _updateFunctionMapping; }
        }

        internal IEnumerable<ModificationFunctionParameterBinding> PrimaryParameterBindings
        {
            get
            {
                var result = Enumerable.Empty<ModificationFunctionParameterBinding>();

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

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping,
                InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
        }
    }
}
