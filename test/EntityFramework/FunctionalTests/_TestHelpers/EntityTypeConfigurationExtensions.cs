namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class EntityTypeConfigurationExtensions
    {
        public static void IgnoreAllBut<TEntityType>(
            this EntityTypeConfiguration<TEntityType> entityTypeConfiguration, params string[] names)
            where TEntityType : class
        {
            var properties = typeof(TEntityType).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties.Where(p => !names.Contains(p.Name)))
            {
                var parameter = Expression.Parameter(typeof(TEntityType), "e");

                dynamic expression
                    = Expression.Lambda(
                        Expression.Property(parameter, property),
                        parameter);

                entityTypeConfiguration.Ignore(expression);
            }
        }
    }
}