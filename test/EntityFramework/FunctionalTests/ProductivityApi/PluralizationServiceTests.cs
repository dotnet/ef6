namespace ProductivityApiTests
{
    using FunctionalTests.TestHelpers;
    using SimpleModel;
    using System.Data.Entity;
    using Xunit;

    /// <summary>
    /// Tests for validating the work of pluralization service in a DbContext.
    /// </summary>
    public class PluralizationServiceTests : FunctionalTestBase
    {
        [Fact]
        public void PluralizationService_pluralize_names_taken_from_DbSet_property_type_names()
        {
            var previousPluralizationService = DefaultPluralizationServiceResolver.Instance.PluralizationService;

            try
            {
                // This mocked pluralization service add a 'z' character 
                // at the end of the word, but, only if the last letter is not 'z'
                DefaultPluralizationServiceResolver.Instance.PluralizationService =
                    new FakePluralizationService();

                using (var context = new PluralizationServiceContext())
                {
                    context.Database.Initialize(false);

                    Assert.Equal("Productz", GetEntitySetTableName(context, typeof(Product)));
                    Assert.Equal("Categoryz", GetEntitySetTableName(context, typeof(Category)));
                }
            }
            finally
            {
                DefaultPluralizationServiceResolver.Instance.PluralizationService = previousPluralizationService;
            }
        }

        private class PluralizationServiceContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }

            public PluralizationServiceContext() { }
        }
    }
}
