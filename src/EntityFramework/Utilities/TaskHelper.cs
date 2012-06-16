namespace System.Data.Entity.Utilities
{
    using System.Threading.Tasks;

    internal static class TaskHelper
    {
        internal static Task<T> FromException<T>(Exception ex)
        {
            var completion = new TaskCompletionSource<T>();
            completion.SetException(ex);
            return completion.Task;
        }

        internal static Task<T> FromCancellation<T>()
        {
            var completion = new TaskCompletionSource<T>();
            completion.SetCanceled();
            return completion.Task;
        }
    }
}
