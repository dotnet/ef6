namespace System.Data.Entity.Spatial
{
    internal static class ExtensionMethods
    {
        internal static void CheckNull<T>(this T value, string argumentName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
