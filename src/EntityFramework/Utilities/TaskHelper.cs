namespace System.Data.Entity.Utilities
{
    using System.Threading.Tasks;

    internal static class TaskHelper
    {
        static internal Task<T> FromException<T>(Exception ex)
        {
            var completion = new TaskCompletionSource<T>();
            completion.SetException(ex);
            return completion.Task;
        }

        static internal Task<T> FromCancellation<T>()
        {
            var completion = new TaskCompletionSource<T>();
            completion.SetCanceled();
            return completion.Task;
        }
    }
}