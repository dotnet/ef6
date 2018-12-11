// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Collections.Generic;
    using System.Data.Entity.SqlServer.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class SqlTypesAssemblyLoader
    {
        private const string AssemblyNameTemplate 
            = "Microsoft.SqlServer.Types, Version={0}.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
        
        private static readonly SqlTypesAssemblyLoader _instance = new SqlTypesAssemblyLoader();

        public static SqlTypesAssemblyLoader DefaultInstance
        {
            get { return _instance; }
        }

        private readonly IEnumerable<string> _preferredSqlTypesAssemblies;

        private readonly Lazy<SqlTypesAssembly> _latestVersion;

        public SqlTypesAssemblyLoader(IEnumerable<string> assemblyNames = null)
        {
            if (assemblyNames != null)
            {
                _preferredSqlTypesAssemblies = assemblyNames;
            }
            else
            {
                // Put the two we always knew about first to avoid breaking if they are available.
                var knownAssemblyNames = new List<string>
                {
                    GenerateSqlServerTypesAssemblyName(11),
                    GenerateSqlServerTypesAssemblyName(10),
                };

                for (var version = 20; version > 11; version--)
                {
                    knownAssemblyNames.Add(GenerateSqlServerTypesAssemblyName(version));
                }

                _preferredSqlTypesAssemblies = knownAssemblyNames.ToList();
            }

            _latestVersion = new Lazy<SqlTypesAssembly>(BindToLatest, isThreadSafe: true);
        }

        private static string GenerateSqlServerTypesAssemblyName(int version)
        {
            return string.Format(CultureInfo.InvariantCulture, AssemblyNameTemplate, version);
        }

        // <summary>
        // Used to create an instance of <see cref="SqlTypesAssemblyLoader"/> for a specific SQL Types assembly
        // such that it can be used for converting EF spatial types backed by one version to those backed by
        // the version actually in use in this app domain.
        // </summary>
        public SqlTypesAssemblyLoader(SqlTypesAssembly assembly)
        {
            _latestVersion = new Lazy<SqlTypesAssembly>(() => assembly, isThreadSafe: true);
        }

        // <summary>
        // Returns the highest available version of the Microsoft.SqlServer.Types assembly that could be
        // located using Assembly.Load; may return <c>null</c> if no version of the assembly could be found.
        // </summary>
        public virtual SqlTypesAssembly TryGetSqlTypesAssembly()
        {
            return _latestVersion.Value;
        }

        public virtual SqlTypesAssembly GetSqlTypesAssembly()
        {
            var value = _latestVersion.Value;
            if (value == null)
            {
                throw new InvalidOperationException(Strings.SqlProvider_SqlTypesAssemblyNotFound);
            }
            return value;
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
            var candidateAssemblies =
                SqlProviderServices.SqlServerTypesAssemblyName != null
                    ? new[] { SqlProviderServices.SqlServerTypesAssemblyName }
                    : _preferredSqlTypesAssemblies;

            foreach (var assemblyFullName in candidateAssemblies)
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
