// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    /// Describes modification function mappings for an association set.
    /// </summary>
    public sealed class AssociationSetModificationFunctionMapping : MappingItem
    {
        private readonly AssociationSet _associationSet;
        private readonly ModificationFunctionMapping _deleteFunctionMapping;
        private readonly ModificationFunctionMapping _insertFunctionMapping;

        /// <summary>
        /// Initalizes a new AssociationSetModificationFunctionMapping instance.
        /// </summary>
        /// <param name="associationSet">An association set.</param>
        /// <param name="deleteFunctionMapping">A delete function mapping.</param>
        /// <param name="insertFunctionMapping">An insert function mapping.</param>
        public AssociationSetModificationFunctionMapping(
            AssociationSet associationSet,
            ModificationFunctionMapping deleteFunctionMapping,
            ModificationFunctionMapping insertFunctionMapping)
        {
            Check.NotNull(associationSet, "associationSet");

            _associationSet = associationSet;
            _deleteFunctionMapping = deleteFunctionMapping;
            _insertFunctionMapping = insertFunctionMapping;
        }

        /// <summary>
        /// Gets the association set.
        /// </summary>
        public AssociationSet AssociationSet
        {
            get { return _associationSet; }
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

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "AS{{{0}}}:{3}DFunc={{{1}}},{3}IFunc={{{2}}}", AssociationSet, DeleteFunctionMapping,
                InsertFunctionMapping, Environment.NewLine + "  ");
        }

        internal override void SetReadOnly()
        {
            SetReadOnly(_deleteFunctionMapping);
            SetReadOnly(_insertFunctionMapping);

            base.SetReadOnly();
        }
    }
}
