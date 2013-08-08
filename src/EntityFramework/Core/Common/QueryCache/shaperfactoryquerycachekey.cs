// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    internal class ShaperFactoryQueryCacheKey<T> : QueryCacheKey
    {
        private readonly string _columnMapKey;
        private readonly MergeOption _mergeOption;
        private readonly bool _isValueLayer;
        private readonly bool _streaming;

        internal ShaperFactoryQueryCacheKey(string columnMapKey, MergeOption mergeOption, bool streaming, bool isValueLayer)
        {
            DebugCheck.NotNull(columnMapKey);
            _columnMapKey = columnMapKey;
            _mergeOption = mergeOption;
            _isValueLayer = isValueLayer;
            _streaming = streaming;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ShaperFactoryQueryCacheKey<T>;
            if (null == other)
            {
                return false;
            }
            return _columnMapKey.Equals(other._columnMapKey, _stringComparison)
                   && _mergeOption == other._mergeOption
                   && _isValueLayer == other._isValueLayer
                   && _streaming == other._streaming;
        }

        public override int GetHashCode()
        {
            return _columnMapKey.GetHashCode();
        }
    }
}
