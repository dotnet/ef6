namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;

    internal interface IDbEnumerator<out T> : IDbAsyncEnumerator<T>, IEnumerator<T>
    {
        new T Current { get; }
    }
}
