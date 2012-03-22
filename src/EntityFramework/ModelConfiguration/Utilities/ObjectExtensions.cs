namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class ObjectExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T t)
            where T : class
        {
            if (t == null)
            {
                return Enumerable.Empty<T>();
            }

            return new[] { t };
        }

        public static void ParseQualifiedTableName(string qualifiedName, out string schemaName, out string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(qualifiedName));

            qualifiedName = qualifiedName.Trim();

            // determine if there is a schema in the tableName
            var lastDot = qualifiedName.LastIndexOf('.');
            schemaName = null;
            tableName = qualifiedName;
            if (lastDot != -1)
            {
                if (lastDot == 0)
                {
                    throw Error.ToTable_InvalidSchemaName(qualifiedName);
                }
                else if (lastDot == tableName.Length - 1)
                {
                    throw Error.ToTable_InvalidTableName(qualifiedName);
                }
                schemaName = qualifiedName.Substring(0, lastDot);
                tableName = qualifiedName.Substring(lastDot + 1);
            }
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                schemaName = null;
            }
        }
    }
}
