namespace System.Data.Entity.Core.Common.QueryCache
{
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics;

    internal class ShaperFactoryQueryCacheKey<T> : QueryCacheKey
    {
        private readonly string _columnMapKey;
        private readonly MergeOption _mergeOption;
        private readonly bool _isValueLayer;

        internal ShaperFactoryQueryCacheKey(string columnMapKey, MergeOption mergeOption, bool isValueLayer)
        {
            Debug.Assert(null != columnMapKey, "null columnMapKey");
            _columnMapKey = columnMapKey;
            _mergeOption = mergeOption;
            _isValueLayer = isValueLayer;
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
                   && _isValueLayer == other._isValueLayer;
        }

        public override int GetHashCode()
        {
            return _columnMapKey.GetHashCode();
        }
    }
}
