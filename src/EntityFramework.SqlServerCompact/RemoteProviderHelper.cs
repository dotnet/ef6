// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    internal static class RemoteProviderHelper
    {
        private const string RemoteDataProviderAssembly = "Microsoft.SqlServerCe.Client";

        private static bool _isFirst = true;

        private static Assembly _rdp;

        /// <summary>
        /// Convinience method to load the type specific to Remote Provider
        /// </summary>
        internal static Type GetRemoteProviderType(string typeName)
        {
            if (_isFirst)
            {
                try
                {
                    var assemblyName = ConstructFullAssemblyName(RemoteDataProviderAssembly);
                    _rdp = Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                    //user should not be informed about this error.
                }
                catch (FileLoadException)
                {
                    //user should not be informed about this error.
                }
                catch (BadImageFormatException)
                {
                    //user should not be informed about this error.
                }

                _isFirst = false;
            }

            return (null == _rdp) ? null : _rdp.GetType(typeName, false);
        }

        /// <summary>
        /// Compare whether the object obj is of the type typeName
        /// </summary>
        internal static bool CompareObjectEqualsToType(Object obj, string typeName)
        {
            var type = GetRemoteProviderType(typeName);
            return (null != type && obj.GetType() == type);
        }

        /// <summary>
        /// Create an instance of the Remote Provider specific type
        /// </summary>
        internal static object CreateRemoteProviderType(string typeName)
        {
            var type = GetRemoteProviderType(typeName);

            if (null != type)
            {
                return Activator.CreateInstance(type);
            }

            // we should never come here.
            //
            throw ADP1.InvalidOperation(typeName);
        }

        /// <summary>
        /// Returns whether the Remote Provider is loaded or not.
        /// </summary>
        internal static bool IsRemoteProviderLoaded
        {
            get { return (null != _rdp); }
        }

        /// <summary>
        /// Use current assembly information to construct the full
        /// assembly name for an assembly with name assemblyName.
        /// This is called for Microsoft.SqlServerCe.Client.dll only.
        /// </summary>
        private static string ConstructFullAssemblyName(string assemblyName)
        {
            DebugCheck.NotNull(assemblyName);

            //we'll use the name to construct full assembly name. If what we got ends with
            //.DLL , we should get rid of it, as file extension is not part of assembly name
            if (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                assemblyName = Path.GetFileNameWithoutExtension(assemblyName);
            }

            Debug.Assert(!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));

            //construct full target assembly name. 
            //All parts of its strong name except for the assembly name should be exactly the same as ours
            //
            var ourAssembly = Assembly.GetExecutingAssembly();
            Debug.Assert(ourAssembly != null);

            var fullName = ourAssembly.FullName;
            fullName = fullName.Replace(ourAssembly.GetName().Name, assemblyName);

            return fullName;
        }

        internal static object GetRemoteSqlCeEngine(string connectionString, out Type rdpType)
        {
            var engine = CreateRemoteProviderType(RemoteProvider.SqlCeEngine);
            rdpType = GetRemoteProviderType(RemoteProvider.SqlCeEngine);

            // Set the connection string property of SqlCeEngine.
            var connString = rdpType.GetProperty("LocalConnectionString", BindingFlags.Public | BindingFlags.Instance);
            connString.SetValue(engine, connectionString, new object[] { });

            return engine;
        }
    }

    internal static class RemoteProvider
    {
        internal static string NameSpace = "Microsoft.SqlServerCe.Client";
        internal static string SqlCeConnection = "Microsoft.SqlServerCe.Client.SqlCeConnection";
        internal static string SqlCeCommand = "Microsoft.SqlServerCe.Client.SqlCeCommand";
        internal static string SqlCeParameter = "Microsoft.SqlServerCe.Client.SqlCeParameter";
        internal static string SqlCeEngine = "Microsoft.SqlServerCe.Client.SqlCeEngine";
        internal static string SqlCeTransaction = "Microsoft.SqlServerCe.Client.SqlCeTransaction";
    }
}
