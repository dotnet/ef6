namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;

    internal sealed class DefaultModelCacheKey : IDbModelCacheKey
    {
        private readonly Type _contextType;
        private readonly string _providerName;
        private readonly string _schema;

        public DefaultModelCacheKey(Type contextType, string providerName, string schema)
        {
            Contract.Requires(contextType != null);
            Contract.Requires(typeof(DbContext).IsAssignableFrom(contextType));
            Contract.Requires(!string.IsNullOrWhiteSpace(providerName));

            _contextType = contextType;
            _providerName = providerName;
            _schema = schema;
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
                       ^ (!string.IsNullOrWhiteSpace(_schema) ? _schema.GetHashCode() : 0);
            }
        }

        private bool Equals(DefaultModelCacheKey other)
        {
            Contract.Requires(other != null);

            return _contextType == other._contextType
                   && string.Equals(_providerName, other._providerName)
                   && string.Equals(_schema, other._schema);
        }
    }
}
