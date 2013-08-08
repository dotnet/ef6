// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Summary description for SchemaElementLookUpTableEnumerator.
    /// </summary>
    internal sealed class SchemaElementLookUpTableEnumerator<T, S> : IEnumerator<T>
        where T : S
        where S : SchemaElement
    {
        #region Instance Fields

        private readonly Dictionary<string, S> _data;
        private List<string>.Enumerator _enumerator;

        #endregion

        #region Public Methods

        public SchemaElementLookUpTableEnumerator(Dictionary<string, S> data, List<string> keysInOrder)
        {
            DebugCheck.NotNull(data);
            DebugCheck.NotNull(keysInOrder);

            _data = data;
            _enumerator = keysInOrder.GetEnumerator();
        }

        #endregion

        #region IEnumerator Members

        public void Reset()
        {
            // it is implemented explicitly
            ((IEnumerator)_enumerator).Reset();
        }

        public T Current
        {
            get
            {
                var key = _enumerator.Current;
                return _data[key] as T;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                var key = _enumerator.Current;
                return _data[key] as T;
            }
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (Current != null)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
