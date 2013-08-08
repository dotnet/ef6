// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using Microsoft.Win32;

    /// <summary>
    /// Acts as a proxy for a <see cref="RegistryKey" /> instance such that uses of the RegistryKey can be mocked.
    /// </summary>
    internal class RegistryKeyProxy : IDisposable
    {
        // The underlying key; will be null when this class is mocked
        private readonly RegistryKey _key;

        /// <summary>
        /// For mocking.
        /// </summary>
        protected RegistryKeyProxy()
        {
        }

        /// <summary>
        /// Constructs a proxy around a real <see cref="RegistryKey" />. The given key
        /// may be null if the underlying registry key doesn't exist.
        /// </summary>
        public RegistryKeyProxy(RegistryKey key)
        {
            _key = key;
        }

        /// <summary>
        /// Allows implicit conversion of a real <see cref="RegistryKey" /> to a proxy.
        /// </summary>
        public static implicit operator RegistryKeyProxy(RegistryKey key)
        {
            return new RegistryKeyProxy(key);
        }

        /// <summary>
        /// Gets the count of sub keys, returning 0 if this key doesn't exist.
        /// </summary>
        public virtual int SubKeyCount
        {
            get { return _key == null ? 0 : _key.SubKeyCount; }
        }

        /// <summary>
        /// Gets the names of the sub keys, returnin an empty array if this key doesn't exist.
        /// </summary>
        public virtual string[] GetSubKeyNames()
        {
            return _key == null ? new string[0] : _key.GetSubKeyNames();
        }

        /// <summary>
        /// Opens the sub key with the given name and always returns a RegistryKeyProxy even
        /// if this key or the sub key does not exist.
        /// </summary>
        public virtual RegistryKeyProxy OpenSubKey(string name)
        {
            return new RegistryKeyProxy(_key == null ? null : _key.OpenSubKey(name));
        }

        /// <summary>
        /// Disposes the underlying <see cref="RegistryKey" />, if it exists.
        /// </summary>
        public virtual void Dispose()
        {
            if (_key != null)
            {
                _key.Dispose();
            }
        }
    }
}
