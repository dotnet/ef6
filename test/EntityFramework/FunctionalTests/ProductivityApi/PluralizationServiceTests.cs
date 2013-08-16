namespace ProductivityApiTests
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using SimpleModel;
    using System;
    using System.Linq;    
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    /// <summary>
    /// Tests for validating the work of pluralization service in a DbContext.
    /// </summary>
    public class PluralizationServiceTests : FunctionalTestBase
    {
        [Fact]
        public void PluralizationService_pluralize_names_taken_from_DbSet_property_type_names()
        {
            try
            {
                // This mocked pluralization service add a 'z' character 
                // at the end of the word, but, only if the last letter is not 'z'
                MutableResolver.AddResolver<IPluralizationService>(k => new FakePluralizationService());

                using (var context = new PluralizationServiceContext())
                {
                    context.Database.Initialize(false);

                    Assert.Equal("Productz", GetEntitySetTableName(context, typeof(Product)));
                    Assert.Equal("Categoryz", GetEntitySetTableName(context, typeof(Category)));
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        private string GetEntitySetTableName(DbContext dbContext, Type clrType)
        {
            var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;

            var container = objectContext.MetadataWorkspace
                .GetItems<EntityContainer>(DataSpace.SSpace)
                .SingleOrDefault();

            if (container == null)
            {
                return null;
            }

            var entitySet = container.BaseEntitySets
                .Where(bes => bes.ElementType.Name == clrType.Name)
                .SingleOrDefault();

            if (entitySet == null)
            {
                return null;
            }

            return entitySet.Table;
        }

        private class PluralizationServiceContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }

            public PluralizationServiceContext() { }
        }

        public class FakePluralizationService : IPluralizationService
        {
            public string Pluralize(string word)
            {
                if (!word.EndsWith("z"))
                {
                    return string.Format("{0}z", word);
                }
                return word;
            }

            public string Singularize(string word)
            {
                return word;
            }
        }
    }
}
