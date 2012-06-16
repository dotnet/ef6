namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    /// <summary>
    /// Describes modification function mappings for an entity type within an entity set.
    /// </summary>
    internal sealed class StorageEntityTypeModificationFunctionMapping
    {
        internal StorageEntityTypeModificationFunctionMapping(
            EntityType entityType,
            StorageModificationFunctionMapping deleteFunctionMapping,
            StorageModificationFunctionMapping insertFunctionMapping,
            StorageModificationFunctionMapping updateFunctionMapping)
        {
            Contract.Requires(entityType != null);

            EntityType = entityType;
            DeleteFunctionMapping = deleteFunctionMapping;
            InsertFunctionMapping = insertFunctionMapping;
            UpdateFunctionMapping = updateFunctionMapping;
        }

        /// <summary>
        /// Gets (specific) entity type these functions handle.
        /// </summary>
        internal readonly EntityType EntityType;

        /// <summary>
        /// Gets delete function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping DeleteFunctionMapping;

        /// <summary>
        /// Gets insert function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping InsertFunctionMapping;

        /// <summary>
        /// Gets update function for the current entity type.
        /// </summary>
        internal readonly StorageModificationFunctionMapping UpdateFunctionMapping;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "ET{{{0}}}:{4}DFunc={{{1}}},{4}IFunc={{{2}}},{4}UFunc={{{3}}}", EntityType, DeleteFunctionMapping,
                InsertFunctionMapping, UpdateFunctionMapping, Environment.NewLine + "  ");
        }

#if DEBUG
        internal void Print(int index)
        {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            var sb = new StringBuilder();
            sb.Append("Entity Type Function Mapping");
            sb.Append("   ");
            sb.Append(ToString());
            Console.WriteLine(sb.ToString());
        }
#endif
    }
}
