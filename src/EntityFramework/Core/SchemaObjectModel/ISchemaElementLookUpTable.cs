// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;

    /// <summary>
    /// Summary description for ISchemaElementLookUpTable.
    /// </summary>
    internal interface ISchemaElementLookUpTable<T>
        where T : SchemaElement
    {
        int Count { get; }

        bool ContainsKey(string key);

        T this[string key] { get; }

        IEnumerator<T> GetEnumerator();

        /// <summary>
        /// Look up a name case insensitively
        /// </summary>
        /// <param name="key"> the key to look up </param>
        /// <returns> the element or null </returns>
        T LookUpEquivalentKey(string key);
    }
}
