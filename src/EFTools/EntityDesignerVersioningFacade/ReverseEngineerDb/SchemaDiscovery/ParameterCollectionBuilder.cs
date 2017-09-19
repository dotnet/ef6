// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    // Abstracts building collections of parameters with or without duplicate parameter value de-duplication
    internal class ParameterCollectionBuilder
    {
        private EntityParameterCollection _parameterCollection;
        private Dictionary<string, string> _parameterMap = null;

        public ParameterCollectionBuilder(EntityParameterCollection parameterCollection, bool optimizeParameters)
        {
            _parameterCollection = parameterCollection;
            if (optimizeParameters)
            {
                _parameterMap = new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        // used for testing
        public ParameterCollectionBuilder() : this(new EntityCommand().Parameters, true)
        {
        }

        private bool IsOptimized
        {
            get
            {
                return _parameterMap != null;
            }
        }

        public EntityParameterCollection ParameterCollection
        {
            get
            {
                return _parameterCollection;
            }
        }

        public string GetOrAdd(string parameterValue)
        {
            string parameterName = null;
            if (!IsOptimized || IsOptimized && !_parameterMap.TryGetValue(parameterValue, out parameterName))
            {
                parameterName = GetNextParameterName();
                _parameterCollection.AddWithValue(parameterName, parameterValue);
                if (IsOptimized)
                {
                    _parameterMap.Add(parameterValue, parameterName);
                }
            }
            return parameterName;
        }

        private string GetNextParameterName()
        {
            return "p" + _parameterCollection.Count.ToString(CultureInfo.InvariantCulture);
        }
    }
}
