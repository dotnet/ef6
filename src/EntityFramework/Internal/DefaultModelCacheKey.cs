// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal sealed class DefaultModelCacheKey : IDbModelCacheKey
    {
        private readonly Type _contextType;
        private readonly string _providerName;
        private readonly Type _providerType;
        private readonly string _customKey;

        public DefaultModelCacheKey(Type contextType, string providerName, Type providerType, string customKey)
        {
            DebugCheck.NotNull(contextType);
            Debug.Assert(typeof(DbContext).IsAssignableFrom(contextType));
            DebugCheck.NotEmpty(providerName);
            DebugCheck.NotNull(providerType);

            _contextType = contextType;
            _providerName = providerName;
            _providerType = providerType;
            _customKey = customKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var modelCacheKey = obj as DefaultModelCacheKey;

            return (modelCacheKey != null) && Equals(modelCacheKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_contextType.GetHashCode() * 397)
                       ^ _providerName.GetHashCode()
                       ^ _providerType.GetHashCode()
                       ^ (!string.IsNullOrWhiteSpace(_customKey) ? _customKey.GetHashCode() : 0);
            }
        }

        private bool Equals(DefaultModelCacheKey other)
        {
            DebugCheck.NotNull(other);

            return _contextType == other._contextType
                   && string.Equals(_providerName, other._providerName)
                   && string.Equals(_providerType, other._providerType)
                   && string.Equals(_customKey, other._customKey);
        }
    }
}
