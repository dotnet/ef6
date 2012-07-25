// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="keysInOrder"></param>
        public SchemaElementLookUpTableEnumerator(Dictionary<string, S> data, List<string> keysInOrder)
        {
            Debug.Assert(data != null, "data parameter is null");
            Debug.Assert(keysInOrder != null, "keysInOrder parameter is null");

            _data = data;
            _enumerator = keysInOrder.GetEnumerator();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            // it is implemented explicitly
            ((IEnumerator)_enumerator).Reset();
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
