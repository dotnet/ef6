namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public sealed class DbConnectionExtensionsTests
    {
        [Fact]
        public void GetProviderInvariantName_should_return_correct_name()
        {
            Assert.Equal("System.Data.SqlClient", new SqlConnection().GetProviderInvariantName());
        }

        [Fact]
        public void GetProviderInvariantName_should_return_correct_name_when_generic_provider()
        {
            Assert.Equal("My.Generic.Provider.DbProviderFactory", new GenericConnection<DbProviderFactory>().GetProviderInvariantName());
        }

        [Fact]
        public void GetProviderInvariantName_throws_for_unknown_provider()
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(new Mock<DbProviderFactory>().Object);
            mockConnection.Setup(m => m.ToString()).Returns("I Be A Bad Bad Connection Is What I Be.");

            Assert.Equal(
                Strings.ModelBuilder_ProviderNameNotFound("Castle.Proxies.DbProviderFactoryProxy"),
                Assert.Throws<NotSupportedException>(() => mockConnection.Object.GetProviderInvariantName()).Message);
        }

        [Fact]
        public void GetProviderInvariantName_returns_invariant_name_for_weakly_named_provider()
        {
            Assert.Equal(
                "Weak.Provider.Factory", 
                CreateMockConnection(WeakProviderType.AssemblyQualifiedName).Object.GetProviderInvariantName());
        }

        [Fact]
        public void GetProviderInvariantName_returns_invariant_name_for_weakly_named_provider_without_version_or_key_information_in_registered_name()
        {
            Assert.Equal(
                "Weak.Provider.Factory",
                CreateMockConnection("WeakProviderFactory, ProviderAssembly").Object.GetProviderInvariantName());
        }

        [Fact]
        public void GetProviderInvariantName_returns_invariant_name_for_weakly_named_provider_with_non_standard_spacing_in_the_registered_name()
        {
            Assert.Equal(
                "Weak.Provider.Factory",
                CreateMockConnection("WeakProviderFactory,ProviderAssembly,   Version=0.0.0.0,    Culture=neutral,PublicKeyToken=null").
                    Object.GetProviderInvariantName());
        }

        private static Mock<DbConnection> CreateMockConnection(string assemblyQualifiedName)
        {
            var providerType = WeakProviderType;
            RegisterWeakProviderFactory(assemblyQualifiedName);
            var dbProviderFactory = (DbProviderFactory)Activator.CreateInstance(providerType);

            var mockConnection = new Mock<DbConnection>();
            mockConnection.Protected().Setup<DbProviderFactory>("DbProviderFactory").Returns(dbProviderFactory);
            return mockConnection;
        }

        private static readonly Type WeakProviderType = CreateWeakProviderType();
        private static Type CreateWeakProviderType()
        {
            var assembly = new DynamicAssembly();
            var dynamicType = assembly.DynamicType("WeakProviderFactory").HasBaseClass(typeof(DbProviderFactory));
            dynamicType.CtorAccess = MemberAccess.Public;
            dynamicType.Field("Instance").HasType(dynamicType).IsStatic().IsInstance();
            var compiledAssembly = assembly.Compile(new AssemblyName("ProviderAssembly"));

            // We need this so that Type.GetType() used in DbProviderFactories.GetFactory will work for
            // the dynamic assembly. In other words, this is only needed for the test code to work.
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) => args.Name.StartsWith("ProviderAssembly") ? compiledAssembly : null;

            return assembly.GetType("WeakProviderFactory");
        }

        private static void RegisterWeakProviderFactory(string assemblyQualifiedName)
        {
            var providerTable = (DataTable)typeof(DbProviderFactories)
                                               .GetMethod("GetProviderTable", BindingFlags.Static | BindingFlags.NonPublic)
                                               .Invoke(null, null);

            var row = providerTable.Rows
                .OfType<DataRow>()
                .FirstOrDefault(r => (string)r["InvariantName"] == "Weak.Provider.Factory");
            if (row != null)
            {
                providerTable.Rows.Remove(row);
            }

            row = providerTable.NewRow();
            row["Name"] = "WeakProviderFactory";
            row["InvariantName"] = "Weak.Provider.Factory";
            row["Description"] = "Provider factory that is not strongly named.";
            row["AssemblyQualifiedName"] = assemblyQualifiedName;
            providerTable.Rows.Add(row);
        }

    }
}