// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.SqlServer.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class SqlTypesAssemblyLoader
    {
        private readonly IEnumerable<string> _preferredSqlTypesAssemblies;

        private readonly Lazy<SqlTypesAssembly> _latestVersion;

        public SqlTypesAssemblyLoader(IEnumerable<string> assemblyNames = null)
        {
            _preferredSqlTypesAssemblies
                = assemblyNames
                  ?? new[]
                         {
                             "Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                             "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                         };

            _latestVersion = new Lazy<SqlTypesAssembly>(BindToLatest, isThreadSafe: true);
        }

        /// <summary>
        ///     Returns the highest available version of the Microsoft.SqlServer.Types assembly that could be
        ///     located using Assembly.Load; may return <c>null</c> if no version of the assembly could be found.
        /// </summary>
        public virtual SqlTypesAssembly TryGetSqlTypesAssembly()
        {
            return _latestVersion.Value;
        }

        public virtual SqlTypesAssembly GetSqlTypesAssembly()
        {
            if (_latestVersion.Value == null)
            {
                throw new InvalidOperationException(Strings.SqlProvider_SqlTypesAssemblyNotFound);
            }
            return _latestVersion.Value;
        }

        public virtual bool TryGetSqlTypesAssembly(Assembly assembly, out SqlTypesAssembly sqlAssembly)
        {
            if (IsKnownAssembly(assembly))
            {
                sqlAssembly = new SqlTypesAssembly(assembly);
                return true;
            }
            sqlAssembly = null;
            return false;
        }

        private SqlTypesAssembly BindToLatest()
        {
            Assembly sqlTypesAssembly = null;
            foreach (var assemblyFullName in _preferredSqlTypesAssemblies)
            {
                var asmName = new AssemblyName(assemblyFullName);
                try
                {
                    sqlTypesAssembly = Assembly.Load(asmName);
                    break;
                }
                catch (FileNotFoundException)
                {
                }
                catch (FileLoadException)
                {
                }
            }

            if (sqlTypesAssembly != null)
            {
                return new SqlTypesAssembly(sqlTypesAssembly);
            }
            return null;
        }

        private bool IsKnownAssembly(Assembly assembly)
        {
            foreach (var knownAssemblyFullName in _preferredSqlTypesAssemblies)
            {
                if (AssemblyNamesMatch(assembly.FullName, new AssemblyName(knownAssemblyFullName)))
                {
                    return true;
                }
            }
            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool AssemblyNamesMatch(string infoRowProviderAssemblyName, AssemblyName targetAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(infoRowProviderAssemblyName))
            {
                return false;
            }

            AssemblyName assemblyName;
            try
            {
                assemblyName = new AssemblyName(infoRowProviderAssemblyName);
            }
            catch (Exception)
            {
                return false;
            }

            // Match the provider assembly details
            if (!string.Equals(targetAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (targetAssemblyName.Version == null
                || assemblyName.Version == null)
            {
                return false;
            }

            if (targetAssemblyName.Version.Major != assemblyName.Version.Major
                || targetAssemblyName.Version.Minor != assemblyName.Version.Minor)
            {
                return false;
            }

            var targetPublicKeyToken = targetAssemblyName.GetPublicKeyToken();
            return targetPublicKeyToken != null && targetPublicKeyToken.SequenceEqual(assemblyName.GetPublicKeyToken());
        }
    }
}
