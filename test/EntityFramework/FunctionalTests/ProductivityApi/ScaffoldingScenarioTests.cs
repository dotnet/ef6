// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns;
    using Xunit;

    public class ScaffoldingScenarioTests : FunctionalTestBase
    {
        static ScaffoldingScenarioTests()
        {
            TemplateTestsDatabaseInitializer.InitializeModelFirstDatabases();
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

        [Fact]
        public void DbContextInfo_can_provide_a_code_first_model_without_hitting_the_database()
        {
            Database.SetInitializer<CodeFirstScaffoldingContext>(null);

            using (var context = new DbContextInfo(
                typeof(CodeFirstScaffoldingContext),
                new DbProviderInfo("System.Data.SqlClient", "2008")).CreateInstance())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("CodeFirstScaffoldingContext", objectContext.DefaultContainerName);
                Assert.Equal("Foo", ((CodeFirstScaffoldingContext)context).ExtraInfo);
            }
        }

        [Fact]
        public void DbContextInfo_can_provide_a_code_first_model_without_hitting_the_database_when_also_give_config()
        {
            Database.SetInitializer<CodeFirstScaffoldingContextWithConnection>(null);

            using (var context = new DbContextInfo(
                typeof(CodeFirstScaffoldingContextWithConnection),
                AddConnectionStrings(CreateEmptyConfig()),
                new DbProviderInfo("System.Data.SqlClient", "2008")).CreateInstance())
            {
                // Will throw if CodeFirstConnectionString is not found and will throw if it is used to
                // make a connection since it points to a server that does not exist.
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                Assert.Equal("CodeFirstScaffoldingContextWithConnection", objectContext.DefaultContainerName);
                Assert.Equal("Bar", ((CodeFirstScaffoldingContextWithConnection)context).ExtraInfo);
            }
        }

        private Configuration AddConnectionStrings(Configuration config)
        {
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(
                    "DbFirstConnectionString",
                    @"metadata=.\AdvancedPatterns.csdl|.\AdvancedPatterns.ssdl|.\AdvancedPatterns.msl;provider=System.Data.SqlClient;provider connection string='Server=.\SQLEXPRESS;Integrated Security=True;Database=AdvancedPatternsModelFirst;'",
                    "System.Data.EntityClient"));

            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(
                    "CodeFirstConnectionString",
                    @"Server=IIsNotAValidServer;Integrated Security=True;Database=ItMattersNot;",
                    "System.Data.SqlClient"));

            return config;
        }
    }
}
