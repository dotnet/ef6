// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.QueryCache
{
    // <summary>
    // represents an abstract cache key
    // </summary>
    internal abstract class QueryCacheKey
    {
        #region Constants

        protected const int EstimatedParameterStringSize = 20;

        #endregion

        #region Fields

        // <summary>
        // entry hit counter
        // </summary>
        private uint _hitCount;

        // <summary>
        // default string comparison kind - Ordinal
        // </summary>
        protected static StringComparison _stringComparison = StringComparison.Ordinal;

        #endregion

        #region Constructor

        protected QueryCacheKey()
        {
            _hitCount = 1;
        }

        #endregion

        #region Abstract Methods

        // <summary>
        // Determines whether two instances of QueryCacheContext are equal.
        // Equality is value based.
        // </summary>
        public abstract override bool Equals(object obj);

        // <summary>
        // Returns QueryCacheContext instance HashCode
        // </summary>
        public abstract override int GetHashCode();

        #endregion

        #region Internal API

        // <summary>
        // Cache entry hit count
        // </summary>
        internal uint HitCount
        {
            get { return _hitCount; }

            set { _hitCount = value; }
        }

        // <summary>
        // Gets/Sets Aging index for cache entry
        // </summary>
        internal int AgingIndex { get; set; }

        // <summary>
        // Updates hit count
        // </summary>
        internal void UpdateHit()
        {
            if (uint.MaxValue != _hitCount)
            {
                unchecked
                {
                    _hitCount++;
                }
            }
        }

        // <summary>
        // default string comparer
        // </summary>
        protected virtual bool Equals(string s, string t)
        {
            return String.Equals(s, t, _stringComparison);
        }

        #endregion
    }
}
