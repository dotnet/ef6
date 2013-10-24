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
        private readonly EntityType _entityType;
        private readonly ModificationFunctionMapping _deleteFunctionMapping;
        private readonly ModificationFunctionMapping _insertFunctionMapping;
        private readonly ModificationFunctionMapping _updateFunctionMapping;

        /// <summary>
        /// Initializes a new EntityTypeModificationFunctionMapping instance.
        /// </summary>
        /// <param name="entityType">An entity type.</param>
        /// <param name="deleteFunctionMapping">A delete function mapping.</param>
        /// <param name="insertFunctionMapping">An insert function mapping.</param>
        /// <param name="updateFunctionMapping">An updated function mapping.</param>
        public EntityTypeModificationFunctionMapping(
            EntityType entityType,
            ModificationFunctionMapping deleteFunctionMapping,
            ModificationFunctionMapping insertFunctionMapping,
            ModificationFunctionMapping updateFunctionMapping)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
            _deleteFunctionMapping = deleteFunctionMapping;
            _insertFunctionMapping = insertFunctionMapping;
            _updateFunctionMapping = updateFunctionMapping;
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType EntityType
        {
            get { return _entityType; }
        }

        /// <summary>
        /// Gets the delete function mapping.
        /// </summary>
        public ModificationFunctionMapping DeleteFunctionMapping
        {
            get { return _deleteFunctionMapping; }
        }

        /// <summary>
        /// Gets the insert function mapping.
        /// </summary>
        public ModificationFunctionMapping InsertFunctionMapping
        {
            get { return _insertFunctionMapping; }
        }

        /// <summary>
        /// Gets hte update function mapping.
        /// </summary>
        public ModificationFunctionMapping UpdateFunctionMapping
        {
            get { return _updateFunctionMapping; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping,
                InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
        }

        internal override void SetReadOnly()
        {
            SetReadOnly(_deleteFunctionMapping);
            SetReadOnly(_insertFunctionMapping);
            SetReadOnly(_updateFunctionMapping);

            base.SetReadOnly();
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
    }
}
