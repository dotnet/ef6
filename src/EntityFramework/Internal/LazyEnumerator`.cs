// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    // <summary>
    // Used to wrap ObjectResult and defer query execution until first call to MoveNext. 
    // </summary>
    // <typeparam name="T">The element type of the wrapped ObjectResult</typeparam>
    // <remarks>This class is not thread safe.</remarks>
    internal class LazyEnumerator<T> : IEnumerator<T>
    {
        private readonly Func<ObjectResult<T>> _getObjectResult;
        private IEnumerator<T> _objectResultEnumerator;

        public LazyEnumerator(Func<ObjectResult<T>> getObjectResult)
        {
            DebugCheck.NotNull(getObjectResult);
            _getObjectResult = getObjectResult;
        }

        public T Current
        {
            get
            {
                return _objectResultEnumerator == null 
                    ? default(T) 
                    : _objectResultEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
        
        public void Dispose()
        {
            if (_objectResultEnumerator != null)
            {
                _objectResultEnumerator.Dispose();
            }
        }

        public bool MoveNext()
        {
            if (_objectResultEnumerator == null)
            {
                var objectResult = _getObjectResult();
                DebugCheck.NotNull(objectResult); // _getObjectResult should never return null
                try
                {
                    _objectResultEnumerator = objectResult.GetEnumerator();
                }
                catch
                {
                    // if there is a problem creating the enumerator, we should dispose
                    // the enumerable (if there is no problem, the enumerator will take 
                    // care of the dispose)
                    objectResult.Dispose();
                    throw;
                }
            }
            return _objectResultEnumerator.MoveNext();
        }

        public void Reset()
        {
            // no-op if we haven't started enumerating
            if (_objectResultEnumerator != null)
            {
                _objectResultEnumerator.Reset();
            }
        }
    }
}
