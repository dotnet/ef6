namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbEdmPropertyMappingExtensions
    {
        public static void SyncNullabilityCSSpace(this DbEdmPropertyMapping propertyMapping)
        {
            Contract.Requires(propertyMapping != null);

            var isNullable = propertyMapping.PropertyPath.Last().PropertyType.IsNullable;

            if (isNullable != null)
            {
                propertyMapping.Column.IsNullable = (bool)isNullable;
            }
        }
    }
}
