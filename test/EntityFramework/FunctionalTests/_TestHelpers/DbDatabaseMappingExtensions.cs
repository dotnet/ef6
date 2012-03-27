namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Serialization;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public static class DbDatabaseMappingExtensions
    {
        private static readonly MethodInfo _getViewsMethod
            = typeof(StorageMappingItemCollection).GetMethod("GenerateEntitySetViews",
                                                             BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void ShellEdmx(this DbDatabaseMapping databaseMapping, string fileName = "Dump.edmx")
        {
            new EdmxSerializer().Serialize(databaseMapping, databaseMapping.Database.GetProviderInfo(),
                                           XmlWriter.Create(File.CreateText(fileName),
                                                            new XmlWriterSettings { Indent = true }));

            Process.Start(fileName);
        }

        internal static bool EdmxIsEqualTo(this DbDatabaseMapping databaseMapping,
                                           DbDatabaseMapping otherDatabaseMapping)
        {
            return SerializeToString(databaseMapping) == SerializeToString(otherDatabaseMapping);
        }

        internal static string SerializeToString(DbDatabaseMapping databaseMapping)
        {
            var edmx = new StringBuilder();
            new EdmxSerializer().Serialize(databaseMapping, databaseMapping.Database.GetProviderInfo(),
                                           XmlWriter.Create(edmx, new XmlWriterSettings { Indent = true }));
            return edmx.ToString();
        }

        internal static void AssertValid(this DbDatabaseMapping databaseMapping)
        {
            AssertValid(databaseMapping, false);
        }

        internal static void AssertValid(this DbDatabaseMapping databaseMapping, bool shouldThrow)
        {
            var storageItemMappingCollection
                = databaseMapping.ToStorageMappingItemCollection();

            Xunit.Assert.NotNull(_getViewsMethod);

            var args = new object[] { null };

            _getViewsMethod.Invoke(storageItemMappingCollection, args);

            var errors = (IList<EdmSchemaError>)args[0];

            if (errors.Any())
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine();

                foreach (var error in errors)
                {
                    errorMessage.AppendLine(error.ToString());
                }

                if (shouldThrow)
                {
                    throw new MappingException(errorMessage.ToString());
                }
                else
                {
                    Xunit.Assert.True(false, errorMessage.ToString());
                }
            }
        }
    }
}