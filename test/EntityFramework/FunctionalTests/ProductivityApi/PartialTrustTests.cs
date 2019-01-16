// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security;
    using AdvancedPatternsModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     Tests that run various things in a partial trust sandbox.
    /// </summary>
    [PartialTrustFixture]
    public class PartialTrustTests : FunctionalTestBase
    {
        [Fact(Skip = "Fails when delay signed")]
        public void DbContextInfo_works_under_partial_trust()
        {
            var contextInfo = new DbContextInfo(
                typeof(AdvancedPatternsMasterContext),
                ProviderRegistry.Sql2008_ProviderInfo);

            using (var context = contextInfo.CreateInstance())
            {
                Assert.NotNull(context);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void DbPropertyValues_ToObject_for_an_entity_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingClone = (Building)context.Entry(building).CurrentValues.ToObject();

                Assert.Equal("Building One", buildingClone.Name);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void DbPropertyValues_ToObject_for_a_complex_type_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var addressClone =
                    (Address)context.Entry(building).CurrentValues.GetValue<DbPropertyValues>("Address").ToObject();

                Assert.Equal("Redmond", addressClone.City);
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Non_generic_DbSet_creation_works_under_partial_trust()
        {
            using (var context = new EmptyContext())
            {
                var set = context.Set(typeof(Product));

                Assert.NotNull(set);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void DbEntityEntry_Member_works_for_collections_under_partial_trust()
        {
            using (var context = new SimpleModelContext())
            {
                var category = context.Categories.First();

                var collection = context.Entry(category).Member<ICollection<Product>>("Products");

                Assert.NotNull(collection);
                Assert.IsType<DbCollectionEntry<Category, Product>>(collection);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void Non_generic_DbSet_Create_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Set(typeof(Building)).Create(typeof(Building));

                Assert.NotNull(building);
                Assert.IsAssignableFrom<Building>(building);
                Assert.IsNotType<Building>(building);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void DbPropertyValues_SetValues_for_an_entity_wih_complex_objects_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var newBuilding = new Building
                {
                    BuildingId = new Guid(building.BuildingId.ToString()),
                    Name = "Bag End",
                    Value = building.Value,
                    Address = new Address
                    {
                        Street = "The Hill",
                        City = "Hobbiton",
                        State = "WF",
                        ZipCode = "00001",
                        SiteInfo = new SiteInfo
                        {
                            Zone = 3,
                            Environment = "Comfortable"
                        }
                    },
                };

                context.Entry(building).CurrentValues.SetValues(newBuilding);

                Assert.Equal("Bag End", building.Name);
                Assert.Equal("Hobbiton", building.Address.City);
                Assert.Equal("Comfortable", building.Address.SiteInfo.Environment);
            }
        }

        public class PartialTrustSetsContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void DbContext_set_initialization_works_under_partial_trust()
        {
            Database.SetInitializer<PartialTrustSetsContext>(null);

            using (var context = new PartialTrustSetsContext())
            {
                Assert.NotNull(context.Products);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void Non_generic_store_query_works_under_partial_trust()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Database.SqlQuery(typeof(int), "select Id from Products").ToList<int>();

                Assert.Equal(7, products.Count);
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void SelectMany_works_under_partial_trust()
        {
            using (var context = new SimpleModelForLinq())
            {
                var parameter = 1;
                var query = from n in context.Numbers
                    from p in context.Products
                    where n.Value > p.UnitsInStock && n.Value == parameter
                    select
                        new LinqTests.NumberProductProjectionClass
                        {
                            Value = n.Value,
                            UnitsInStock = p.UnitsInStock
                        };
                Assert.IsType<DbQuery<LinqTests.NumberProductProjectionClass>>(query);
                query.Load();
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void Setting_current_value_of_reference_nav_prop_works_under_partial_trust()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);
                Assert.Null(product.Category);

                var newCategory = new Category("BeanBags");
                context.Entry(product).Reference(p => p.Category).CurrentValue = newCategory;

                Assert.Equal("BeanBags", product.CategoryId);
                Assert.Same(newCategory, product.Category);
            }
        }

        public class ClassWithContextField
        {
            private SimpleModelContext _context;

            public List<IQueryable<int>> Test()
            {
                using (_context = new SimpleModelContext())
                {
                    return _context.Products.Select(p => _context.Products.Select(p2 => p2.Id)).ToList();
                }
            }
        }

        [ExtendedFact(SkipForLocalDb = true, Justification = "Creating new instance of Local Db requires permissions that are not availabe in partial trust")]
        public void Query_with_top_level_nested_query_obtained_from_context_field_in_select_works_under_partial_trust()
        {
            var results = new ClassWithContextField().Test();

            Assert.Equal(7, results.Count);
        }

        // Dev11 216491
        [Fact(Skip = "Fails when delay signed")]
        [FullTrust] // Bespoke test with setup that requires full trust
        public void IsAspNetEnvironment_swallows_security_exception_when_System_Web_is_considered_non_APTCA()
        {
            using (var sandbox = new PartialTrustSandbox(grantReflectionPermission: true))
            {
                var withReflectionPermission = sandbox.CreateInstance<PartialTrustTests>();

                withReflectionPermission.InvokeIsAspNetEnvironment();
            }
        }

        private static readonly Type _aspProxy =
            typeof(ObjectContext).Assembly().GetType("System.Data.Entity.Core.Metadata.Edm.AspProxy");

        public void InvokeIsAspNetEnvironment()
        {
            var aspProxy = Activator.CreateInstance(_aspProxy, nonPublic: true);
            var isAspNetEnvironment = _aspProxy.GetDeclaredMethod("IsAspNetEnvironment");

            // Before fixing Dev11 216491 this would throw a SecurityException
            Assert.False((bool)isAspNetEnvironment.Invoke(aspProxy, new object[0]));
        }

        public class ProxiesContext : DbContext
        {
            static ProxiesContext()
            {
                Database.SetInitializer<ProxiesContext>(null);
            }

            public DbSet<MeLazyLoad> MeLazyLoads { get; set; }
            public DbSet<MeTrackChanges> MeTrackChanges { get; set; }
        }

        [Serializable]
        [DataContract]
        public class MeTrackChanges
        {
            [DataMember]
            public virtual int Id { get; set; }

            [DataMember]
            public virtual ICollection<MeLazyLoad> MeLazyLoad { get; set; }
        }

        [Serializable]
        [DataContract]
        public class MeLazyLoad
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public virtual MeTrackChanges MeTrackChanges { get; set; }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Lazy_loading_proxy_can_be_created_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.IsNotType<MeLazyLoad>(proxy);
                Assert.False(proxy is IEntityWithChangeTracker);
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Change_tracking_proxy_can_be_created_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                Assert.IsNotType<MeTrackChanges>(proxy);
                Assert.True(proxy is IEntityWithChangeTracker);
            }
        }

        public class FullTrustProxiesContext : DbContext
        {
            static FullTrustProxiesContext()
            {
                Database.SetInitializer<FullTrustProxiesContext>(null);
            }

            public DbSet<MeISerializable> MeISerializables { get; set; }
        }

        [Serializable]
        public class MeISerializable : ISerializable
        {
            public virtual int Id { get; set; }

            public MeISerializable()
            {
            }

            protected MeISerializable(SerializationInfo info, StreamingContext context)
            {
            }

            [SecurityCritical]
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
            }
        }

        [Fact]
        [FullTrust]
        public void Proxy_for_ISerializable_entity_can_be_created_under_full_trust_and_is_ISerializable()
        {
            using (var context = new FullTrustProxiesContext())
            {
                var proxy = context.MeISerializables.Create();
                Assert.IsNotType<MeISerializable>(proxy);
                Assert.True(proxy is ISerializable);
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Resolve_handler_is_not_added_for_assembly_when_running_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                Assert.Null(Type.GetType(context.MeLazyLoads.Create().GetType().AssemblyQualifiedName));
            }
        }

        [Fact]
        [FullTrust]
        public void Resolve_handler_is_added_for_assembly_when_running_under_full_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.Same(proxy.GetType(), Type.GetType(proxy.GetType().AssemblyQualifiedName));
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Change_tracking_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeTrackChanges.Create();
                Assert.True(proxy is IEntityWithRelationships);

                proxy.Id = 77;

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(
                    typeof(MeTrackChanges), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeTrackChanges)serializer.ReadObject(stream);

                Assert.IsType<MeTrackChanges>(deserialized); // Resolver returns non-proxy type
                Assert.Equal(77, deserialized.Id);
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Lazy_loading_proxy_can_be_data_contract_deserialized_with_resolver_when_running_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.False(proxy is IEntityWithRelationships);

                proxy.Id = 77;

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(
                    typeof(MeLazyLoad), null, int.MaxValue, false, true, null, new ProxyDataContractResolver());

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeLazyLoad)serializer.ReadObject(stream);

                Assert.IsType<MeLazyLoad>(deserialized); // Resolver returns non-proxy type
                Assert.Equal(77, deserialized.Id);
            }
        }

        [Fact(Skip = "Fails when delay signed")]
        public void Lazy_loading_proxy_can_be_data_contract_deserialized_with_known_types_when_running_under_partial_trust()
        {
            using (var context = new ProxiesContext())
            {
                var proxy = context.MeLazyLoads.Create();
                Assert.False(proxy is IEntityWithRelationships);
                proxy.Id = 77;

                var otherProxy = context.MeTrackChanges.Create();

                var stream = new MemoryStream();
                var serializer = new DataContractSerializer(
                    proxy.GetType(), new[] { proxy.GetType(), otherProxy.GetType() }, int.MaxValue, false, true, null);

                serializer.WriteObject(stream, proxy);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (MeLazyLoad)serializer.ReadObject(stream);

                Assert.Same(proxy.GetType(), deserialized.GetType());
                Assert.Equal(77, deserialized.Id);
            }
        }
    }
}
