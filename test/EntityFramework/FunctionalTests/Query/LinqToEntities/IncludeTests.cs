// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class IncludeTests : FunctionalTestBase
    {
        [Fact]
        public void Nested_include()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include("OwnedRun.Tasks");
                var results = query.ToList();
                var tasksForOwners = context.Owners.Select(o => o.OwnedRun.Tasks).ToList();
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(tasksForOwners[i].Count, results[i].OwnedRun.Tasks.Count);
                    var expectedTasks = tasksForOwners[i].Select(t => t.Id).ToList();
                    var actualTasks = results[i].OwnedRun.Tasks.Select(t => t.Id).ToList();
                    Enumerable.SequenceEqual(expectedTasks, actualTasks);
                }
            }
        }

        [Fact]
        public void Include_propagation_over_filter()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include(o => o.OwnedRun).Where(o => o.Id == 1);
                var results = query.ToList();
                Assert.NotNull(results.First().OwnedRun);
            }
        }

        [Fact]
        public void Include_propagation_over_sort()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Owners.Include(o => o.OwnedRun).OrderBy(o => o.Id == 1);
                var results = query.ToList();
                Assert.NotNull(results.First().OwnedRun);
            }
        }

        [Fact]
        public void Include_propagation_over_type_filter()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Configs.Include(o => o.Failures).OfType<ArubaMachineConfig>();
                var results = query.ToList();
                Assert.True(results.Any(r => r.Failures.Count > 0));
            }
        }

        [Fact]
        public void Include_propagation_over_first()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).First();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_with_predicate()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).First(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).FirstOrDefault();
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_propagation_over_first_or_default_with_predicate()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var results = context.Configs.Include(o => o.Failures).FirstOrDefault(o => o.Id > 0);
                Assert.NotNull(results.Failures);
                Assert.True(results.Failures.Count > 0);
            }
        }

        [Fact]
        public void Include_from_concat_combined()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Failures.Include(f => f.Bugs).Concat(context.Failures.Include(f => f.Configs));
                var results = query.ToList();
                Assert.True(results.Any(r => r.Bugs.Count > 0));
                Assert.True(results.Any(r => r.Configs.Count > 0));
            }
        }

        [Fact]
        public void Include_from_union_combined()
        {
            using (var context = new ArubaContext())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
                var query = context.Failures.Include(f => f.Bugs).Union(context.Failures.Include(f => f.Configs));
                var results = query.ToList();
                Assert.True(results.Any(r => r.Bugs.Count > 0));
                Assert.True(results.Any(r => r.Configs.Count > 0));
            }
        }

        public class CodePlex1710 : FunctionalTestBase
        {
            private const string Ssdl =
@"<Schema Namespace=""QueryViewStackOverflowRepro.ContextModel.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2012"" Alias=""Self"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
    <EntityType Name=""Dependents"">
        <Key>
        <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""int"" Nullable=""false"" />
    </EntityType>
    <EntityType Name=""Principals"">
        <Key>
        <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""int"" StoreGeneratedPattern=""Identity"" Nullable=""false"" />
    </EntityType>
    <EntityContainer Name=""QueryViewStackOverflowReproContextModelStoreContainer"">
        <EntitySet Name=""Dependents"" EntityType=""Self.Dependents"" Schema=""dbo"" store:Type=""Tables"" />
        <EntitySet Name=""Principals"" EntityType=""Self.Principals"" Schema=""dbo"" store:Type=""Tables"" />
    </EntityContainer>
</Schema>";

            private const string Csdl =
@"<Schema Namespace=""QueryViewStackOverflowRepro.ContextModel"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
    <EntityType Name=""Dependent"">
        <Key>
        <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Int32"" Nullable=""false"" />
        <NavigationProperty Name=""Principal"" Relationship=""Self.FK_dbo_Dependents_dbo_Principals_Id"" FromRole=""Dependents"" ToRole=""Principals"" />
    </EntityType>
    <EntityType Name=""Principal"">
        <Key>
        <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
        <NavigationProperty Name=""Dependent"" Relationship=""Self.FK_dbo_Dependents_dbo_Principals_Id"" FromRole=""Principals"" ToRole=""Dependents"" />
    </EntityType>
    <Association Name=""FK_dbo_Dependents_dbo_Principals_Id"">
        <End Role=""Principals"" Type=""Self.Principal"" Multiplicity=""1"" />
        <End Role=""Dependents"" Type=""Self.Dependent"" Multiplicity=""0..1"" />
        <ReferentialConstraint>
        <Principal Role=""Principals"">
            <PropertyRef Name=""Id"" />
        </Principal>
        <Dependent Role=""Dependents"">
            <PropertyRef Name=""Id"" />
        </Dependent>
        </ReferentialConstraint>
    </Association>
    <EntityContainer Name=""Entities1"" annotation:LazyLoadingEnabled=""true"">
        <EntitySet Name=""Dependents"" EntityType=""Self.Dependent"" />
        <EntitySet Name=""Principals"" EntityType=""Self.Principal"" />
        <AssociationSet Name=""FK_dbo_Dependents_dbo_Principals_Id"" Association=""Self.FK_dbo_Dependents_dbo_Principals_Id"">
        <End Role=""Principals"" EntitySet=""Principals"" />
        <End Role=""Dependents"" EntitySet=""Dependents"" />
        </AssociationSet>
    </EntityContainer>
</Schema>";

            private const string Msl =
@"<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">
    <EntityContainerMapping StorageEntityContainer=""QueryViewStackOverflowReproContextModelStoreContainer"" CdmEntityContainer=""Entities1"">
        <EntitySetMapping Name=""Principals"">
        <EntityTypeMapping TypeName=""QueryViewStackOverflowRepro.ContextModel.Principal"">
            <MappingFragment StoreEntitySet=""Principals"">
            <ScalarProperty Name=""Id"" ColumnName=""Id"" />
            </MappingFragment>
        </EntityTypeMapping>
        </EntitySetMapping>
        <EntitySetMapping Name=""Dependents"">
        <QueryView>
            SELECT VALUE QueryViewStackOverflowRepro.ContextModel.Dependent(g.Id) FROM QueryViewStackOverflowReproContextModelStoreContainer.Dependents AS g  <!-- WHERE g.PrincipalId > 100 -->
        </QueryView>
        </EntitySetMapping>
    </EntityContainerMapping>
</Mapping>";

            public class Dependent
            {
                public int Id { get; set; }
                public int PrincipalId { get; set; }

                public virtual Principal Principal { get; set; }
            }

            public class Principal
            {
                public int Id { get; set; }
                public int? DependentId { get; set; }

                public virtual Dependent Dependent { get; set; }
            }

            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer<Context>(null);
                }

                public Context(ObjectContext objectContext, bool dbContextOwnsObjectContext)
                    : base(objectContext, dbContextOwnsObjectContext)
                {
                }

                public virtual DbSet<Dependent> Dependents { get; set; }
                public virtual DbSet<Principal> Principals { get; set; }
            }

            [Fact]
            public void Query_using_Include_does_not_throw()
            {
                var storeCollection = new StoreItemCollection(new[] { XDocument.Parse(Ssdl).CreateReader() });
                var edmCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });
                var mappingCollection = new StorageMappingItemCollection(
                    edmCollection, storeCollection, new[] { XDocument.Parse(Msl).CreateReader() });

                var workspace = new MetadataWorkspace(
                    () => edmCollection,
                    () => storeCollection,
                    () => mappingCollection);

                using (var connection = new EntityConnection(workspace, new SqlConnection(), true))
                {
                    using (var context = new Context(new ObjectContext(connection), true))
                    {
                        context.Dependents.Include(d => d.Principal).ToString();
                    }
                }
            }
        }
    }
}
