namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    internal static class DbProviderManifestExtensions
    {
        public static string GetStoreTypeName(
            this DbProviderManifest providerManifest, PrimitiveTypeKind primitiveTypeKind)
        {
            Contract.Requires(providerManifest != null);

            var edmTypeUsage =
                TypeUsage.CreateDefaultTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(primitiveTypeKind));

            return providerManifest.GetStoreType(edmTypeUsage).EdmType.Name;
        }
    }
}
