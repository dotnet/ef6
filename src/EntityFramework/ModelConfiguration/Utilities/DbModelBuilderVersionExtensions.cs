namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Data.Entity.Edm.Common;

    internal static class DbModelBuilderVersionExtensions
    {
        public static double GetEdmVersion(this DbModelBuilderVersion modelBuilderVersion)
        {
            switch (modelBuilderVersion)
            {
                case DbModelBuilderVersion.V4_1:
                    return DataModelVersions.Version2;
                case DbModelBuilderVersion.V5_0:
                case DbModelBuilderVersion.Latest:
                    return DataModelVersions.Version3;
                default:
                    throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }
        }
    }
}