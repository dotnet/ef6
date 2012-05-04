namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal static class MetdataItemExtensions
    {
        public static T GetMetadataPropertyValue<T>(this MetadataItem item, string propertyName)
        {
            var property = item.MetadataProperties.FirstOrDefault(p => p.Name == propertyName);
            return property == null ? default(T) : (T)property.Value;
        }
    }
}