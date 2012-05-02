namespace System.Data.Entity.SqlServer.Utilities
{
    internal static class FuncExtensions
    {
        internal static TResult NullIfNotImplemented<TResult>(this Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (NotImplementedException)
            {
                return default(TResult);
            }
        }
    }
}
