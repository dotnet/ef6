namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Information about a key that is part of an EntityKey.
    /// A key member contains the key name and value.
    /// </summary>
    [DataContract]
    [Serializable]
    public class EntityKeyMember
    {
        private string _keyName;
        private object _keyValue;

        /// <summary>
        /// Creates an empty EntityKeyMember. This constructor is used by serialization.
        /// </summary>
        public EntityKeyMember()
        {
        }

        /// <summary>
        /// Creates a new EntityKeyMember with the specified key name and value.
        /// </summary>
        /// <param name="keyName">The key name</param>
        /// <param name="keyValue">The key value</param>
        public EntityKeyMember(string keyName, object keyValue)
        {
            Contract.Requires(keyName != null);
            Contract.Requires(keyValue != null);
            _keyName = keyName;
            _keyValue = keyValue;
        }

        /// <summary>
        /// The key name
        /// </summary>
        [DataMember]
        public string Key
        {
            get { return _keyName; }
            set
            {
                Contract.Requires(value != null);

                ValidateWritable(_keyName);
                _keyName = value;
            }
        }

        /// <summary>
        /// The key value
        /// </summary>
        [DataMember]
        public object Value
        {
            get { return _keyValue; }
            set
            {
                Contract.Requires(value != null);

                ValidateWritable(_keyValue);
                _keyValue = value;
            }
        }

        /// <summary>
        /// Returns a string representation of the EntityKeyMember
        /// </summary>
        /// <returns>A string representation of the EntityKeyMember</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "[{0}, {1}]", _keyName, _keyValue);
        }

        /// <summary>
        /// Ensures that the instance can be written to (value must be null)
        /// </summary>
        private static void ValidateWritable(object instance)
        {
            if (instance != null)
            {
                throw new InvalidOperationException(Strings.EntityKey_CannotChangeKey);
            }
        }
    }
}
