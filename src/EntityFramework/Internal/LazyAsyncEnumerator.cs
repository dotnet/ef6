// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // Used to wrap ObjectResult and defer async query execution until first call to MoveNextAsyc is completed. 
    // </summary>
    // <typeparam name="T">The element type of the wrapped ObjectResult</typeparam>
    // <remarks>This class is not thread safe.</remarks>
    internal class LazyAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly Func<CancellationToken, Task<ObjectResult<T>>> _getObjectResultAsync;
        private IDbAsyncEnumerator<T> _objectResultAsyncEnumerator;

        public LazyAsyncEnumerator(Func<CancellationToken, Task<ObjectResult<T>>> getObjectResultAsync)
        {
            DebugCheck.NotNull(getObjectResultAsync);
            _getObjectResultAsync = getObjectResultAsync;
        }

        public T Current
        {
            get
            {
                return _objectResultAsyncEnumerator == null
                    ? default(T)
                    : _objectResultAsyncEnumerator.Current;
            }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            if (_objectResultAsyncEnumerator != null)
            {
                _objectResultAsyncEnumerator.Dispose();
            }
        }

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_objectResultAsyncEnumerator != null)
            {
                return _objectResultAsyncEnumerator.MoveNextAsync(cancellationToken);
            }

            return FirstMoveNextAsync(cancellationToken);
        }

        private async Task<bool> FirstMoveNextAsync(CancellationToken cancellationToken)
        {
            var objectResult = await _getObjectResultAsync(cancellationToken).WithCurrentCulture();
            DebugCheck.NotNull(objectResult); // await _getObjectResultAsync should never return null 
            try
            {
                _objectResultAsyncEnumerator = ((IDbAsyncEnumerable<T>)objectResult).GetAsyncEnumerator();
            }
            catch
            {
                // if there is a problem creating the enumerator, we should dispose
                // the enumerable (if there is no problem, the enumerator will take 
                // care of the dispose)
                objectResult.Dispose();
                throw;
            }
            return await _objectResultAsyncEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture();
        }
    }
}

#endif
