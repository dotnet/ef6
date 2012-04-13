namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Spatial.Internal;

    internal static class ExtensionMethods
    {
        internal static void CheckNull<T>(this T value, string argumentName) where T : class
        {
            if (value == null)
            {
                throw SpatialExceptions.ArgumentNull(argumentName);
            }
        }
    }
}
