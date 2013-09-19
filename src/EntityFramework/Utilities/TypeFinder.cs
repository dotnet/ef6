// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    internal class TypeFinder
    {
        private readonly Assembly _assembly;

        public TypeFinder(Assembly assembly)
        {
            DebugCheck.NotNull(assembly);

            _assembly = assembly;
        }

        public Type FindType(
            Type baseType,
            string typeName,
            Func<IEnumerable<Type>, IEnumerable<Type>> filter,
            Func<string, Exception> noType = null,
            Func<string, IEnumerable<Type>, Exception> multipleTypes = null,
            Func<string, string, Exception> noTypeWithName = null,
            Func<string, string, Exception> multipleTypesWithName = null)
        {
            DebugCheck.NotNull(baseType);
            DebugCheck.NotNull(filter);

            var typeNameSpecified = !string.IsNullOrWhiteSpace(typeName);

            Type type = null;

            // Try for a fully-qualified match
            if (typeNameSpecified)
            {
                type = _assembly.GetType(typeName);
            }

            // Otherwise, search for it
            if (type == null)
            {
                var assemblyName = _assembly.GetName().Name;
                var types = _assembly.GetAccessibleTypes()
                                     .Where(t => baseType.IsAssignableFrom(t));

                if (typeNameSpecified)
                {
                    types = types
                        .Where(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Disambiguate using case
                    if (types.Count() > 1)
                    {
                        types = types
                            .Where(t => string.Equals(t.Name, typeName, StringComparison.Ordinal))
                            .ToList();
                    }

                    if (!types.Any())
                    {
                        if (noTypeWithName != null)
                        {
                            throw noTypeWithName(typeName, assemblyName);
                        }
                        return null;
                    }

                    if (types.Count() > 1)
                    {
                        if (multipleTypesWithName != null)
                        {
                            throw multipleTypesWithName(typeName, assemblyName);
                        }
                        return null;
                    }
                }
                else
                {
                    // Filter out unusable types
                    types = filter(types);

                    if (!types.Any())
                    {
                        if (noType != null)
                        {
                            throw noType(assemblyName);
                        }
                        return null;
                    }

                    if (types.Count() > 1)
                    {
                        if (multipleTypes != null)
                        {
                            throw multipleTypes(assemblyName, types);
                        }
                        return null;
                    }
                }

                Debug.Assert(types.Count() == 1);
                type = types.Single();
            }

            Debug.Assert(type != null);

            return type;
        }
    }
}
