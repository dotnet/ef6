namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Diagnostics;

    /// <summary>
    /// Represents an utility for creating anonymous IDisposable implementations.
    /// </summary>
    internal class Disposer : IDisposable
    {
        private readonly Action _action;

        internal Disposer(Action action)
        {
            Debug.Assert(action != null, "action != null");
            _action = action;
        }

        public void Dispose()
        {
            _action();
            GC.SuppressFinalize(this);
        }
    }
}
