// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// extract the column rename info from polymorphic entity type mappings
    /// </summary>
    internal sealed class FunctionImportReturnTypeEntityTypeColumnsRenameBuilder
    {
        /// <summary>
        /// CMember -> SMember*
        /// </summary>
        internal Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ColumnRenameMapping;

        internal FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> isOfTypeEntityTypeColumnsRenameMapping,
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> entityTypeColumnsRenameMapping)
        {
            DebugCheck.NotNull(isOfTypeEntityTypeColumnsRenameMapping);
            DebugCheck.NotNull(entityTypeColumnsRenameMapping);

            ColumnRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();

            // Assign the columns renameMapping to the result dictionary.
            foreach (var entityType in isOfTypeEntityTypeColumnsRenameMapping.Keys)
            {
                SetStructuralTypeColumnsRename(
                    entityType, isOfTypeEntityTypeColumnsRenameMapping[entityType], true /*isTypeOf*/);
            }

            foreach (var entityType in entityTypeColumnsRenameMapping.Keys)
            {
                SetStructuralTypeColumnsRename(
                    entityType, entityTypeColumnsRenameMapping[entityType], false /*isTypeOf*/);
            }
        }

        /// <summary>
        /// Set the column mappings for each defaultMemberName.
        /// </summary>
        private void SetStructuralTypeColumnsRename(
            EntityType entityType,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameMapping,
            bool isTypeOf)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(columnsRenameMapping);

            foreach (var mapping in columnsRenameMapping)
            {
                if (!ColumnRenameMapping.Keys.Contains(mapping.CMember))
                {
                    ColumnRenameMapping[mapping.CMember] = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(mapping.CMember);
                }
                ColumnRenameMapping[mapping.CMember].AddRename(
                    new FunctionImportReturnTypeStructuralTypeColumn(mapping.SColumn, entityType, isTypeOf, mapping.LineInfo));
            }
        }
    }
}
