// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class ConfigurationScenarioTests : TestBase
    {
        public class CodePlex1559 : FunctionalTestBase
        {
            public class E
            {
                public int Id { get; set; }
                public decimal P1 { get; set; }
                public TimeSpan P2 { get; set; }
                public DateTime P3 { get; set; }
                public DateTimeOffset P4 { get; set; }
            }


            [Fact]
            public void Default_precision_facets_are_not_null_within_store_model()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<E>();
                modelBuilder.Properties<DateTime>().Configure(p => p.HasColumnType("datetime2"));

                var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

                var storeModel = ((IEdmModelAdapter)model).StoreModel;
                var entityType = storeModel.EntityTypes.ElementAt(0);

                Assert.Equal(18, (byte)entityType.DeclaredProperties["P1"].TypeUsage.Facets["Precision"].Value);
                Assert.Equal(7, (byte)entityType.DeclaredProperties["P2"].TypeUsage.Facets["Precision"].Value);
                Assert.Equal(7, (byte)entityType.DeclaredProperties["P3"].TypeUsage.Facets["Precision"].Value);
                Assert.Equal(7, (byte)entityType.DeclaredProperties["P4"].TypeUsage.Facets["Precision"].Value);
            }
        }
    }
}
