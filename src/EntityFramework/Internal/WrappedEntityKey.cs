namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     A wrapper around EntityKey that allows key/values pairs that have null values to
    ///     be used.  This allows Added entities with null key values to be searched for in
    ///     the ObjectStateManager.
    /// </summary>
    internal class WrappedEntityKey
    {
        #region Constructors and fields

        /// The key name/key value pairs, where some key values may be null
        private readonly IEnumerable<KeyValuePair<string, object>> _keyValuePairs;

        // An actual EntityKey, which is null if some key values are null
        private readonly EntityKey _key;

        /// <summary>
        ///     Creates a new WrappedEntityKey instance.
        /// </summary>
        /// <param name = "entitySet">The entity set that the key belongs to.</param>
        /// <param name = "entitySetName">The fully qualified name of the given entity set.</param>
        /// <param name = "keyValues">The key values, which may be null or contain null values.</param>
        /// <param name = "keyValuesParamName">The name of the parameter passed for keyValue by the user, which is used when throwing exceptions.</param>
        public WrappedEntityKey(
            EntitySet entitySet, string entitySetName, object[] keyValues, string keyValuesParamName)
        {
            // Treat a null array as an array with a single null value since the common case for this is Find(null)
            if (keyValues == null)
            {
                keyValues = new object[] { null };
            }

            var keyNames = entitySet.ElementType.KeyMembers.Select(m => m.Name).ToList();
            if (keyNames.Count
                != keyValues.Length)
            {
                throw new ArgumentException(Strings.DbSet_WrongNumberOfKeyValuesPassed, keyValuesParamName);
            }

            _keyValuePairs = keyNames.Zip(keyValues, (name, value) => new KeyValuePair<string, object>(name, value));

            // Can only create a real EntityKey if all key values are null.
            if (keyValues.All(v => v != null))
            {
                _key = new EntityKey(entitySetName, KeyValuePairs);
            }
        }

        #endregion

        #region Key and key values access

        /// <summary>
        ///     True if any of the key values are null, which means that the EntityKey will also be null.
        /// </summary>
        public bool HasNullValues
        {
            get { return _key == null; }
        }

        /// <summary>
        ///     An actual EntityKey, or null if any of the key values are null.
        /// </summary>
        public EntityKey EntityKey
        {
            get { return _key; }
        }

        /// <summary>
        ///     The key name/key value pairs of the key, in which some of the key values may be null.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> KeyValuePairs
        {
            get { return _keyValuePairs; }
        }

        #endregion
    }
}
