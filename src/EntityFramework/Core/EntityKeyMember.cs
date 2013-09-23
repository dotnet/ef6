// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
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
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKeyMember" /> class.
        /// </summary>
        public EntityKeyMember()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityKeyMember" /> class with the specified entity key pair.
        /// </summary>
        /// <param name="keyName">The name of the key.</param>
        /// <param name="keyValue">The key value.</param>
        public EntityKeyMember(string keyName, object keyValue)
        {
            Check.NotNull(keyName, "keyName");
            Check.NotNull(keyValue, "keyValue");
            _keyName = keyName;
            _keyValue = keyValue;
        }

        /// <summary>Gets or sets the name of the entity key.</summary>
        /// <returns>The key name.</returns>
        [DataMember]
        public string Key
        {
            get { return _keyName; }
            set
            {
                Check.NotNull(value, "value");

                ValidateWritable(_keyName);
                _keyName = value;
            }
        }

        /// <summary>Gets or sets the value of the entity key.</summary>
        /// <returns>The key value.</returns>
        [DataMember]
        public object Value
        {
            get { return _keyValue; }
            set
            {
                Check.NotNull(value, "value");

                ValidateWritable(_keyValue);
                _keyValue = value;
            }
        }

        /// <summary>Returns a string representation of the entity key.</summary>
        /// <returns>A string representation of the entity key.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "[{0}, {1}]", _keyName, _keyValue);
        }

        // <summary>
        // Ensures that the instance can be written to (value must be null)
        // </summary>
        private static void ValidateWritable(object instance)
        {
            if (instance != null)
            {
                throw new InvalidOperationException(Strings.EntityKey_CannotChangeKey);
            }
        }
    }
}
