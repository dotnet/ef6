namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Reflection;

    /// <summary>
    /// Used with the FakeSqlConnection class to fake provider info so that Code First can create SSDL
    /// without having to hit a real store.
    /// </summary>
    public class FakeSqlProviderFactory : DbProviderFactory, IServiceProvider
    {
        public static readonly FakeSqlProviderFactory Instance = new FakeSqlProviderFactory();

        private FakeSqlProviderFactory()
        {
            var providerTable = (DataTable)typeof(DbProviderFactories).GetMethod("GetProviderTable", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);

            var row = providerTable.NewRow();
            row["Name"] = "FakeSqlClient";
            row["InvariantName"] = "System.Data.FakeSqlClient";
            row["Description"] = "Fake SQL Client";
            row["AssemblyQualifiedName"] = GetType().AssemblyQualifiedName;
            providerTable.Rows.Add(row);
        }

        public static void Initialize()
        {
            // Does nothing but ensures that the singleton instance has been created.
        }

// ReSharper disable EmptyConstructor
        static FakeSqlProviderFactory()
// ReSharper restore EmptyConstructor
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
        }

        public bool ForceNullConnection { get; set; }

        public object GetService(Type serviceType)
        {
            return new FakeSqlProviderServices();
        }

        public override DbConnection CreateConnection()
        {
            return ForceNullConnection ? null : new FakeSqlConnection();
        }
    }
}