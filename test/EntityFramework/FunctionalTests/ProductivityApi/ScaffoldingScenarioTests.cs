// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns;
    using SimpleModel;
    using Xunit;

    public class ScaffoldingScenarioTests : FunctionalTestBase
    {
        static ScaffoldingScenarioTests()
        {
            InitializeModelFirstDatabases();
        }

        internal class DatabaseFirstContext : AdvancedPatternsModelFirstContext
        {
            public DatabaseFirstContext()
                : base("name=DbFirstConnectionString")
            {
            }
        }

        [Fact]
        public void DbContextInfo_can_provide_a_database_first_model()
        {
            using (var context = new DbContextInfo(
                typeof(DatabaseFirstContext),
                AddConnectionStrings(CreateEmptyConfig())).CreateInstance())
            {
                // Will throw if DbFirstConnectionString is not found and used.
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("AdvancedPatternsModelFirstContext", objectContext.DefaultContainerName);
            }
        }

        [Fact]
        public void DbContextInfo_can_provide_a_database_first_model_even_when_given_provider_information()
        {
            using (var context = new DbContextInfo(
                typeof(DatabaseFirstContext),
                AddConnectionStrings(CreateEmptyConfig()),
                new DbProviderInfo("Please.Do.Not.Use.Me", "8002")).CreateInstance())
            {
                // Will throw if DbFirstConnectionString is not found and used.
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("AdvancedPatternsModelFirstContext", objectContext.DefaultContainerName);
            }
        }

        public class CodeFirstContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void DbContextInfo_can_provide_a_code_first_model_without_hitting_the_database()
        {
            Database.SetInitializer<CodeFirstContext>(null);

            using (var context = new DbContextInfo(
                typeof(CodeFirstContext),
                new DbProviderInfo("System.Data.SqlClient", "2008")).CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("CodeFirstContext", objectContext.DefaultContainerName);
            }
        }

        public class CodeFirstContextWithConnection : DbContext
        {
            public CodeFirstContextWithConnection()
                : base("name=CodeFirstConnectionString")
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        [Fact]
        public void DbContextInfo_can_provide_a_code_first_model_without_hitting_the_database_when_also_give_config()
        {
            Database.SetInitializer<CodeFirstContextWithConnection>(null);

            using (var context = new DbContextInfo(
                typeof(CodeFirstContextWithConnection),
                AddConnectionStrings(CreateEmptyConfig()),
                new DbProviderInfo("System.Data.SqlClient", "2008")).CreateInstance())
            {
                // Will throw if CodeFirstConnectionString is not found and will throw if it is used to
                // make a connection since it points to a server that does not exist.
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("CodeFirstContextWithConnection", objectContext.DefaultContainerName);
            }
        }

        private Configuration AddConnectionStrings(Configuration config)
        {
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(
                    "DbFirstConnectionString",
                    @"metadata=.\AdvancedPatterns.csdl|.\AdvancedPatterns.ssdl|.\AdvancedPatterns.msl;provider=System.Data.SqlClient;provider connection string='Server=.\SQLEXPRESS;Integrated Security=True;Database=AdvancedPatternsModelFirst;MultipleActiveResultSets=True;'",
                    "System.Data.EntityClient"));

            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(
                    "CodeFirstConnectionString",
                    @"Server=IIsNotAValidServer;Integrated Security=True;Database=ItMattersNot;MultipleActiveResultSets=True;",
                    "System.Data.SqlClient"));

            return config;
        }
    }
}
