// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System;
    using System.Collections.Generic;

    // All user data that are stored in transaction context must implement this interface.
    internal interface ITransactionContextItem
    {
    }

    /// <summary>
    ///     Context object that allows clients to associated user data with the transaction.
    /// </summary>
    internal class EfiTransactionContext
    {
        #region Fields

        private readonly Dictionary<string, ITransactionContextItem> _contextInfo;

        #endregion

        #region Constructor

        public EfiTransactionContext()
        {
            _contextInfo = new Dictionary<string, ITransactionContextItem>();
        }

        #endregion

        /// <summary>
        ///     Lookup the value associated with a specified key in the transaction context.
        /// </summary>
        /// <typeparam name="T">The expected type of the value associated with the specified key</typeparam>
        /// <param name="key">The key to lookup in the context</param>
        /// <param name="value">Receives the value associated with the specified key, or the default value for T if the key is not found</param>
        /// <returns>Whether or not the context contained a value with the specified key.</returns>
        public bool TryGetValue<T>(string key, out T value) where T : ITransactionContextItem
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ITransactionContextItem result;
            if (_contextInfo.TryGetValue(key, out result)
                && result is T)
            {
                value = (T)result;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <summary>
        ///     Associate a value with a particular key in the transaction context.
        /// </summary>
        /// <param name="key">The key to associate the value with. Cannot be null.</param>
        /// <param name="value">The value to be associated with the key. Can be null.</param>
        /// <remarks>Suggested best practice is to use GUIDs for the keys to ensure uniqueness</remarks>
        public void Add(string key, ITransactionContextItem value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _contextInfo[key] = value;
        }

        /// <summary>
        ///     Remove the association between the specified key and its value from the transaction
        ///     context, if any.
        /// </summary>
        /// <param name="key">The key of the {key,value} pair to be removed</param>
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (_contextInfo != null)
            {
                _contextInfo.Remove(key);
            }
        }

        /// <summary>
        ///     Query whether the specified key has been associated with a value in this transaction
        ///     context
        /// </summary>
        /// <param name="key">The key to lookup. Cannot be null.</param>
        /// <returns>True if the key is found, else false.</returns>
        public bool Contains(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return _contextInfo != null && _contextInfo.ContainsKey(key);
        }
    }
}
