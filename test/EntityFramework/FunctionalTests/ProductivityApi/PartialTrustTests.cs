namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using AdvancedPatternsModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Tests that run various things in a partial trust sandbox.
    /// </summary>
    public class PartialTrustTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        private static readonly PartialTrustCode PartialTrustCodeInstance =
            (PartialTrustCode)
            PartialTrustHelpers.CreatePartialTrustSandbox().CreateInstanceAndUnwrap(
                typeof(PartialTrustCode).Assembly.FullName, typeof(PartialTrustCode).FullName);

        #endregion

        #region Partial trust tests

        [Fact]
        public void DbPropertyValues_ToObject_for_an_entity_works_under_partial_trust()
        {
            PartialTrustCodeInstance.DbPropertyValues_ToObject_for_an_entity_works_under_partial_trust();
        }

        [Fact]
        public void DbContextInfo_works_under_partial_trust()
        {
            PartialTrustCodeInstance.DbContextInfo_works_under_partial_trust();
        }

        [Fact]
        public void DbPropertyValues_ToObject_for_a_complex_type_works_under_partial_trust()
        {
            PartialTrustCodeInstance.DbPropertyValues_ToObject_for_a_complex_type_works_under_partial_trust();
        }

        [Fact]
        public void Non_generic_DbSet_creation_works_under_partial_trust()
        {
            PartialTrustCodeInstance.Non_generic_DbSet_creation_works_under_partial_trust();
        }

        [Fact]
        public void DbEntityEntry_Member_works_for_collections_under_partial_trust()
        {
            PartialTrustCodeInstance.DbEntityEntry_Member_works_for_collections_under_partial_trust();
        }

        [Fact]
        public void Non_generic_DbSet_Create_works_under_partial_trust()
        {
            PartialTrustCodeInstance.Non_generic_DbSet_Create_works_under_partial_trust();
        }

        [Fact]
        public void DbPropertyValues_SetValues_for_an_entity_wih_complex_objects_works_under_partial_trust()
        {
            PartialTrustCodeInstance.
                DbPropertyValues_SetValues_for_an_entity_wih_complex_objects_works_under_partial_trust();
        }

        [Fact]
        public void DbContext_set_initialization_works_under_partial_trust()
        {
            PartialTrustCodeInstance.DbContext_set_initialization_works_under_partial_trust();
        }

        [Fact]
        public void Non_generic_store_query_works_under_partial_trust()
        {
            PartialTrustCodeInstance.Non_generic_store_query_works_under_partial_trust();
        }

        [Fact]
        public void SelectMany_works_under_partial_trust()
        {
            PartialTrustCodeInstance.SelectMany_works_under_partial_trust();
        }

        [Fact]
        public void Setting_current_value_of_reference_nav_prop_works_under_partial_trust()
        {
            PartialTrustCodeInstance.Setting_current_value_of_reference_nav_prop_works_under_partial_trust();
        }

        [Fact]
        public void Query_with_top_level_nested_query_obtained_from_context_field_in_select_works_under_partial_trust()
        {
            PartialTrustCodeInstance.
                Query_with_top_level_nested_query_obtained_from_context_field_in_select_works_under_partial_trust();
        }

        [Fact]
        public void PropertyConstraintException_can_be_serialized_and_deserialized_under_partial_trust()
        {
            try
            {
                // Exception is thrown in partial trust and must be serialized across the app-domain boundry
                // to get back here.
                PartialTrustCodeInstance.
                    PropertyConstraintException_can_be_serialized_and_deserialized_under_partial_trust();
                Assert.True(false);
            }
            catch (PropertyConstraintException ex)
            {
                Assert.Equal("Message", ex.Message);
                Assert.Equal("Property", ex.PropertyName);
                Assert.Equal("Inner", ex.InnerException.Message);
            }
        }

        // Dev11 216491
        [Fact(Skip = "Not fixed in Dev 11 beta. Dev11 347808")]
        public void IsAspNetEnvironment_swallows_security_exception_when_System_Web_is_considered_non_APTCA()
        {
            var withReflectionPermission = (PartialTrustCode)PartialTrustHelpers
                                                                 .CreatePartialTrustSandbox(
                                                                     grantReflectionPermission: true)
                                                                 .CreateInstanceAndUnwrap(
                                                                     typeof(PartialTrustCode).Assembly.FullName,
                                                                     typeof(PartialTrustCode).FullName);

            withReflectionPermission.
                IsAspNetEnvironment_swallows_security_exception_when_System_Web_is_considered_non_APTCA();
        }

        #endregion
    }

    /// <summary>
    /// This class contains the actual test code that runs under partial trust.
    /// </summary>
    public class PartialTrustCode : MarshalByRefObject
    {
        #region Partial trust tests

        static PartialTrustCode()
        {
            SqlConnection.ClearAllPools();

            // This is normally done using entries in app.config, but when running in this
            // app domain we need to make sure it is set anyway since the app.config is not used.
            Database.SetInitializer(new AdvancedPatternsInitializer());
            Database.SetInitializer(new SimpleModelInitializer());
        }

        public void DbContextInfo_works_under_partial_trust()
        {
            var contextInfo = new DbContextInfo(typeof(AdvancedPatternsMasterContext),
                                                ProviderRegistry.Sql2008_ProviderInfo);

            var context = contextInfo.CreateInstance();

            Assert.NotNull(context);
        }

        public void DbPropertyValues_ToObject_for_an_entity_works_under_partial_trust()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                var buildingClone = (Building)context.Entry(building).CurrentValues.ToObject();

                Assert.Equal("Building One", buildingClone.Name);
            }
        }

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

        public void Non_generic_DbSet_creation_works_under_partial_trust()
        {
            using (var context = new EmptyContext())
            {
                var set = context.Set(typeof(Product));

                Assert.NotNull(set);
            }
        }

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

        public void DbContext_set_initialization_works_under_partial_trust()
        {
            Database.SetInitializer<PartialTrustSetsContext>(null);

            using (var context = new PartialTrustSetsContext())
            {
                Assert.NotNull(context.Products);
            }
        }

        public void Non_generic_store_query_works_under_partial_trust()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Database.SqlQuery(typeof(int), "select Id from Products").ToList<int>();

                Assert.Equal(7, products.Count);
            }
        }

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
                                { Value = n.Value, UnitsInStock = p.UnitsInStock };
                Assert.IsType<DbQuery<LinqTests.NumberProductProjectionClass>>(query);
                query.Load();
            }
        }

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

        public void Query_with_top_level_nested_query_obtained_from_context_field_in_select_works_under_partial_trust()
        {
            var results = new ClassWithContextField().Test();

            Assert.Equal(7, results.Count);
        }


        public void PropertyConstraintException_can_be_serialized_and_deserialized_under_partial_trust()
        {
            // Serialization is tested by throwing across the app-domain boundry.
            throw new PropertyConstraintException("Message", "Property", new InvalidOperationException("Inner"));
        }

        private static readonly Type AspProxy =
            typeof(ObjectContext).Assembly.GetType("System.Data.Metadata.Edm.AspProxy");

        // Dev11 216491
        public void IsAspNetEnvironment_swallows_security_exception_when_System_Web_is_considered_non_APTCA()
        {
            var aspProxy = Activator.CreateInstance(AspProxy, nonPublic: true);
            var isAspNetEnvironment = AspProxy.GetMethod("IsAspNetEnvironment",
                                                         BindingFlags.Instance | BindingFlags.NonPublic);

            // Before fixing Dev11 216491 this would throw a SecurityException
            Assert.False((bool)isAspNetEnvironment.Invoke(aspProxy, new object[0]));
        }

        #endregion
    }
}