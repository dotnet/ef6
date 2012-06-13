namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;
    using AdvancedPatternsModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Tests that run various things in a partial trust sandbox.
    /// </summary>
    [PartialTrustFixture]
    public class PartialTrustTests : FunctionalTestBase
    {
        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void DbContextInfo_works_under_partial_trust()
        {
            var contextInfo = new DbContextInfo(typeof(AdvancedPatternsMasterContext),
                                                ProviderRegistry.Sql2008_ProviderInfo);

            var context = contextInfo.CreateInstance();

            Assert.NotNull(context);
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void DbPropertyValues_ToObject_for_an_entity_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingClone = (Building)context.Entry(building).CurrentValues.ToObject();

                Assert.Equal("Building One", buildingClone.Name);
            }
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
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

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void Non_generic_DbSet_creation_works_under_partial_trust()
        {
            using (var context = new EmptyContext())
            {
                var set = context.Set(typeof(Product));

                Assert.NotNull(set);
            }
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
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

        [Fact(Skip = "SDE Merge - No partial trust yet")]
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

        [Fact(Skip = "SDE Merge - No partial trust yet")]
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
                                                    SiteInfo = new SiteInfo { Zone = 3, Environment = "Comfortable" }
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

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void DbContext_set_initialization_works_under_partial_trust()
        {
            Database.SetInitializer<PartialTrustSetsContext>(null);

            using (var context = new PartialTrustSetsContext())
            {
                Assert.NotNull(context.Products);
            }
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void Non_generic_store_query_works_under_partial_trust()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Database.SqlQuery(typeof(int), "select Id from Products").ToList<int>();

                Assert.Equal(7, products.Count);
            }
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void SelectMany_works_under_partial_trust()
        {
            using (var context = new SimpleModelForLinq())
            {
                var parameter = 1;
                var query = from n in context.Numbers
                            from p in context.Products
                            where n.Value > p.UnitsInStock && n.Value == parameter
                            select
                                new LinqTests.NumberProductProjectionClass { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<LinqTests.NumberProductProjectionClass>>(query);
                query.Load();
            }
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
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

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void Query_with_top_level_nested_query_obtained_from_context_field_in_select_works_under_partial_trust()
        {
            var results = new ClassWithContextField().Test();

            Assert.Equal(7, results.Count);
        }

        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void PropertyConstraintException_can_be_serialized_and_deserialized_under_partial_trust()
        {
            try
            {
                // Exception is thrown in partial trust and must be serialized across the app-domain boundry
                // to get back here.
                PartialTrustSandbox.Default
                    .CreateInstance<PartialTrustTests>()
                    .ThrowPropertyConstraintException();
                Assert.True(false);
            }
            catch (PropertyConstraintException ex)
            {
                Assert.Equal("Message", ex.Message);
                Assert.Equal("Property", ex.PropertyName);
                Assert.Equal("Inner", ex.InnerException.Message);
            }
        }

        private void ThrowPropertyConstraintException()
        {
            // Serialization is tested by throwing across the app-domain boundry.
            throw new PropertyConstraintException("Message", "Property", new InvalidOperationException("Inner"));
        }

        // Dev11 216491
        [Fact(Skip = "SDE Merge - No partial trust yet")]
        public void IsAspNetEnvironment_swallows_security_exception_when_System_Web_is_considered_non_APTCA()
        {
            using (var sandbox = new PartialTrustSandbox(grantReflectionPermission: true))
            {
                var withReflectionPermission = sandbox.CreateInstance<PartialTrustTests>();

                withReflectionPermission.InvokeIsAspNetEnvironment();
            }
        }

        private static readonly Type _aspProxy =
            typeof(ObjectContext).Assembly.GetType("System.Data.Entity.Core.Metadata.Edm.AspProxy");

        public void InvokeIsAspNetEnvironment()
        {
            var aspProxy = Activator.CreateInstance(_aspProxy, nonPublic: true);
            var isAspNetEnvironment = _aspProxy.GetMethod("IsAspNetEnvironment",
                                                         BindingFlags.Instance | BindingFlags.NonPublic);

            // Before fixing Dev11 216491 this would throw a SecurityException
            Assert.False((bool)isAspNetEnvironment.Invoke(aspProxy, new object[0]));
        }
    }
}