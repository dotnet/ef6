namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
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
            Contract.Requires(isOfTypeEntityTypeColumnsRenameMapping != null);
            Contract.Requires(entityTypeColumnsRenameMapping != null);

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
            Contract.Requires(entityType != null);
            Contract.Requires(columnsRenameMapping != null);

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
