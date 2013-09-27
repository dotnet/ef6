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
        internal AssociationSetModificationFunctionMapping(
            AssociationSet associationSet,
            ModificationFunctionMapping deleteFunctionMapping,
            ModificationFunctionMapping insertFunctionMapping)
        {
            DebugCheck.NotNull(associationSet);

            AssociationSet = associationSet;
            DeleteFunctionMapping = deleteFunctionMapping;
            InsertFunctionMapping = insertFunctionMapping;
        }

        // <summary>
        // Association set these functions handles.
        // </summary>
        internal readonly AssociationSet AssociationSet;

        // <summary>
        // Delete function for this association set.
        // </summary>
        internal readonly ModificationFunctionMapping DeleteFunctionMapping;

        // <summary>
        // Insert function for this association set.
        // </summary>
        internal readonly ModificationFunctionMapping InsertFunctionMapping;

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "AS{{{0}}}:{3}DFunc={{{1}}},{3}IFunc={{{2}}}", AssociationSet, DeleteFunctionMapping,
                InsertFunctionMapping, Environment.NewLine + "  ");
        }
    }
}
