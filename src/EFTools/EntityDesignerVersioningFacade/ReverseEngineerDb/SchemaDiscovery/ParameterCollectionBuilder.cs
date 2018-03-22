// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Globalization;

    // Abstracts building collections of parameters for schema queries 
    // with or without parameter value de-duplication
    internal class ParameterCollectionBuilder
    {
        private EntityParameterCollection _collection;
        private Dictionary<string, string> _map = null;

        public ParameterCollectionBuilder(EntityParameterCollection collection, bool optimized)
        {
            _collection = collection;
            if (optimized)
            {
                _map = new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        public int Count
        {
            get
            {
                return _collection.Count;
            }
        }

        public EntityParameter this[int index]
        {
            get
            {
                return _collection[index];
            }
        }

        public EntityParameter this[string parameterName]
        {
            get
            {
                return _collection[parameterName];
            }
        }

        public string GetOrAdd(string parameterValue)
        {
            string parameterName = null;
            if (_map == null || !_map.TryGetValue(parameterValue, out parameterName))
            {
                parameterName = GetNextParameterName();
                _collection.AddWithValue(parameterName, parameterValue);
                if (_map != null)
                {
                    _map.Add(parameterValue, parameterName);
                }
            }
            return parameterName;
        }

        private string GetNextParameterName()
        {
            return "p" + _collection.Count.ToString(CultureInfo.InvariantCulture);
        }
    }
}
