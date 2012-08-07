// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Resources;

    /// <summary>
    ///     Summary description for FilteredSchemaTypes.
    /// </summary>
    internal sealed class FilteredSchemaElementLookUpTable<T, S> : IEnumerable<T>, ISchemaElementLookUpTable<T>
        where T : S
        where S : SchemaElement
    {
        #region Instance Fields

        private readonly SchemaElementLookUpTable<S> _lookUpTable;

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="lookUpTable"> </param>
        public FilteredSchemaElementLookUpTable(SchemaElementLookUpTable<S> lookUpTable)
        {
            _lookUpTable = lookUpTable;
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _lookUpTable.GetFilteredEnumerator<T>();
        }

        /// <summary>
        /// </summary>
        /// <returns> </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _lookUpTable.GetFilteredEnumerator<T>();
        }

        /// <summary>
        /// </summary>
        public int Count
        {
            get
            {
                var count = 0;
                foreach (SchemaElement element  in _lookUpTable)
                {
                    if (element is T)
                    {
                        ++count;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="key"> </param>
        /// <returns> </returns>
        public bool ContainsKey(string key)
        {
            if (!_lookUpTable.ContainsKey(key))
            {
                return false;
            }
            return _lookUpTable[key] as T != null;
        }

        /// <summary>
        /// </summary>
        public T this[string key]
        {
            get
            {
                var element = _lookUpTable[key];
                if (element == null)
                {
                    return null;
                }
                var elementAsT = element as T;
                if (elementAsT != null)
                {
                    return elementAsT;
                }
                throw new InvalidOperationException(Strings.UnexpectedTypeInCollection(element.GetType(), key));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="key"> </param>
        /// <returns> </returns>
        public T LookUpEquivalentKey(string key)
        {
            return _lookUpTable.LookUpEquivalentKey(key) as T;
        }

        #endregion
    }
}
