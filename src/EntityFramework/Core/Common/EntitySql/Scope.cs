// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a scope of key-value pairs.
    /// </summary>
    internal sealed class Scope : IEnumerable<KeyValuePair<string, ScopeEntry>>
    {
        private readonly Dictionary<string, ScopeEntry> _scopeEntries;

        /// <summary>
        ///     Initialize using a given key comparer.
        /// </summary>
        internal Scope(IEqualityComparer<string> keyComparer)
        {
            _scopeEntries = new Dictionary<string, ScopeEntry>(keyComparer);
        }

        /// <summary>
        ///     Add new key to the scope. If key already exists - throw.
        /// </summary>
        internal Scope Add(string key, ScopeEntry value)
        {
            _scopeEntries.Add(key, value);
            return this;
        }

        /// <summary>
        ///     Remove an entry from the scope.
        /// </summary>
        internal void Remove(string key)
        {
            Debug.Assert(Contains(key));
            _scopeEntries.Remove(key);
        }

        internal void Replace(string key, ScopeEntry value)
        {
            Debug.Assert(Contains(key));
            _scopeEntries[key] = value;
        }

        /// <summary>
        ///     Returns true if the key belongs to the scope.
        /// </summary>
        internal bool Contains(string key)
        {
            return _scopeEntries.ContainsKey(key);
        }

        /// <summary>
        ///     Search item by key. Returns true in case of success and false otherwise.
        /// </summary>
        internal bool TryLookup(string key, out ScopeEntry value)
        {
            return (_scopeEntries.TryGetValue(key, out value));
        }

        #region GetEnumerator

        public Dictionary<string, ScopeEntry>.Enumerator GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, ScopeEntry>> IEnumerable<KeyValuePair<string, ScopeEntry>>.GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _scopeEntries.GetEnumerator();
        }

        #endregion
    }
}
