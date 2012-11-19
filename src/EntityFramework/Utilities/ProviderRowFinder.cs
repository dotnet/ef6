// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    internal class ProviderRowFinder
    {
        private readonly IEnumerable<DataRow> _dataRows;

        public ProviderRowFinder(IEnumerable<DataRow> dataRows = null)
        {
            _dataRows = dataRows ?? DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual DataRow FindRow(Type hintType, Func<DataRow, bool> selector)
        {
            DebugCheck.NotNull(selector);

            const int assemblyQualifiedNameIndex = 3;

            var assemblyHint = hintType == null ? null : new AssemblyName(hintType.Assembly.FullName);

            foreach (var row in _dataRows)
            {
                var assemblyQualifiedTypeName = (string)row[assemblyQualifiedNameIndex];

                AssemblyName rowProviderFactoryAssemblyName = null;

                // Parse the provider factory assembly qualified type name
                Type.GetType(
                    assemblyQualifiedTypeName,
                    a =>
                        {
                            rowProviderFactoryAssemblyName = a;

                            return null;
                        },
                    (_, __, ___) => null);

                if (rowProviderFactoryAssemblyName != null
                    && (hintType == null
                        || string.Equals(
                            assemblyHint.Name,
                            rowProviderFactoryAssemblyName.Name,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        if (selector(row))
                        {
                            return row;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Fail("GetFactory failed with: " + ex);
                        // Ignore bad providers.
                    }
                }
            }

            return null;
        }
    }
}
