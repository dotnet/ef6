// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.QueryCache
{
    /// <summary> 
    /// Represents the abstract base class for all cache entry values in the query cache 
    /// </summary> 
    internal class QueryCacheEntry
    {
        #region Fields

        /// <summary>
        /// querycachekey for this entry
        /// </summary>
        private readonly QueryCacheKey _queryCacheKey;

        /// <summary> 
        /// strong reference to the target object 
        /// </summary> 
        protected readonly object _target;

        #endregion

        #region Constructors

        /// <summary> 
        /// cache entry constructor 
        /// </summary> 
        /// <param name="queryCacheKey"></param> 
        /// <param name="target"></param> 
        internal QueryCacheEntry(QueryCacheKey queryCacheKey, object target)
        {
            _queryCacheKey = queryCacheKey;
            _target = target;
        }

        #endregion

        #region Methods and Properties

        /// <summary> 
        /// The payload of this cache entry.
        /// </summary> 
        internal virtual object GetTarget()
        {
            return _target;
        }

        /// <summary>
        /// Returns the query cache key
        /// </summary>
        internal QueryCacheKey QueryCacheKey
        {
            get { return _queryCacheKey; }
        }

        #endregion
    }
}
