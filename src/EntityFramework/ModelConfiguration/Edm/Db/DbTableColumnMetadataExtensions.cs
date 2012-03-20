namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbTableColumnMetadataExtensions
    {
        private const string OrderAnnotation = "Order";
        private const string PreferredNameAnnotation = "PreferredName";
        private const string UnpreferredUniqueNameAnnotation = "UnpreferredUniqueName";
        private const string AllowOverrideAnnotation = "AllowOverride";

        public static DbTableColumnMetadata Initialize(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            tableColumn.Facets = new DbPrimitiveTypeFacets();

            return tableColumn;
        }

        public static DbTableColumnMetadata Clone(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            return new DbTableColumnMetadata
                {
                    Name = tableColumn.Name,
                    TypeName = tableColumn.TypeName,
                    IsNullable = tableColumn.IsNullable,
                    IsPrimaryKeyColumn = tableColumn.IsPrimaryKeyColumn,
                    StoreGeneratedPattern = tableColumn.StoreGeneratedPattern,
                    Facets = tableColumn.Facets.Clone(),
                    Annotations = tableColumn.Annotations.ToList()
                };
        }

        public static int? GetOrder(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            return (int?)tableColumn.Annotations.GetAnnotation(OrderAnnotation);
        }

        public static void SetOrder(this DbTableColumnMetadata tableColumn, int order)
        {
            Contract.Requires(tableColumn != null);

            tableColumn.Annotations.SetAnnotation(OrderAnnotation, order);
        }

        public static string GetPreferredName(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            return (string)tableColumn.Annotations.GetAnnotation(PreferredNameAnnotation);
        }

        public static void SetPreferredName(this DbTableColumnMetadata tableColumn, string name)
        {
            Contract.Requires(tableColumn != null);

            tableColumn.Annotations.SetAnnotation(PreferredNameAnnotation, name);
        }

        public static string GetUnpreferredUniqueName(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            return (string)tableColumn.Annotations.GetAnnotation(UnpreferredUniqueNameAnnotation);
        }

        public static void SetUnpreferredUniqueName(this DbTableColumnMetadata tableColumn, string name)
        {
            Contract.Requires(tableColumn != null);

            tableColumn.Annotations.SetAnnotation(UnpreferredUniqueNameAnnotation, name);
        }

        public static void RemoveStoreGeneratedIdentityPattern(this DbTableColumnMetadata tableColumn)
        {
            Contract.Requires(tableColumn != null);

            if (tableColumn.StoreGeneratedPattern == DbStoreGeneratedPattern.Identity)
            {
                tableColumn.StoreGeneratedPattern = DbStoreGeneratedPattern.None;
            }
        }

        public static object GetConfiguration(this DbTableColumnMetadata column)
        {
            Contract.Requires(column != null);

            return column.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this DbTableColumnMetadata column, object configuration)
        {
            Contract.Requires(column != null);
            Contract.Requires(configuration != null);

            column.Annotations.SetConfiguration(configuration);
        }

        public static bool GetAllowOverride(this DbTableColumnMetadata column)
        {
            Contract.Requires(column != null);

            return (bool)column.Annotations.GetAnnotation(AllowOverrideAnnotation);
        }

        public static void SetAllowOverride(this DbTableColumnMetadata column, bool allowOverride)
        {
            Contract.Requires(column != null);

            column.Annotations.SetAnnotation(AllowOverrideAnnotation, allowOverride);
        }
    }
}