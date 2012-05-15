namespace System.Data.Entity.Core.Common
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class EntityUtilTests
    {
        public class TryGetProviderInvariantName
        {
            [Fact]
            public void TryGetProviderInvariantName_returns_invariant_name_for_SqlClient_provider()
            {
                string name;
                Assert.True(EntityUtil.TryGetProviderInvariantName(SqlClientFactory.Instance, out name));
                Assert.Equal("System.Data.SqlClient", name);
            }

            [Fact]
            public void TryGetProviderInvariantName_returns_false_for_unknown_provider()
            {
                string _;
                Assert.False(EntityUtil.TryGetProviderInvariantName(new Mock<DbProviderFactory>().Object, out _));
            }

            [Fact]
            public void TryGetProviderInvariantName_returns_invariant_name_for_generic_provider()
            {
                string name;
                Assert.True(EntityUtil.TryGetProviderInvariantName(GenericProviderFactory<DbProviderFactory>.Instance, out name));
                Assert.Equal("My.Generic.Provider.DbProviderFactory", name);
            }

            [Fact]
            public void TryGetProviderInvariantName_returns_invariant_name_for_weakly_named_provider()
            {
                var providerType = WeakProviderType;
                RegisterWeakProviderFactory(providerType.AssemblyQualifiedName);

                var type = Type.GetType(providerType.AssemblyQualifiedName);

                string name;
                Assert.True(EntityUtil.TryGetProviderInvariantName((DbProviderFactory)Activator.CreateInstance(providerType), out name));
                Assert.Equal("Weak.Provider.Factory", name);
            }

            [Fact]
            public void TryGetProviderInvariantName_returns_invariant_name_for_weakly_named_provider_without_version_or_key_information_in_registered_name()
            {
                var providerType = WeakProviderType;
                RegisterWeakProviderFactory("WeakProviderFactory, ProviderAssembly");

                string name;
                Assert.True(EntityUtil.TryGetProviderInvariantName((DbProviderFactory)Activator.CreateInstance(providerType), out name));
                Assert.Equal("Weak.Provider.Factory", name);
            }

            [Fact]
            public void TryGetProviderInvariantName_returns_invariant_name_for_weakly_named_provider_with_non_standard_spacing_in_the_registered_name()
            {
                RegisterWeakProviderFactory("WeakProviderFactory,ProviderAssembly,   Version=0.0.0.0,    Culture=neutral,PublicKeyToken=null");

                string name;
                Assert.True(EntityUtil.TryGetProviderInvariantName((DbProviderFactory)Activator.CreateInstance(WeakProviderType), out name));
                Assert.Equal("Weak.Provider.Factory", name);
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
}
