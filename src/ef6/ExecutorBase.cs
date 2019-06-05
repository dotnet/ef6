// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Data.Entity.Tools.Migrations.Design;

namespace System.Data.Entity.Tools
{
    internal abstract class ExecutorBase
    {
        protected const string ExecutorTypeName = "System.Data.Entity.Infrastructure.Design.Executor";

        public string GetContextType(string contextTypeName, string contextAssemblyName)
            => Invoke<string>(
                "GetContextType",
                new Hashtable
                {
                    ["contextTypeName"] = contextTypeName,
                    ["contextAssemblyName"] = contextAssemblyName
                });

        public ScaffoldedMigration ScaffoldInitialCreate(
                string connectionStringName,
                string connectionString,
                string connectionProviderName,
                string contextTypeName,
                string contextAssemblyName,
                string migrationsNamespace,
                bool auto,
                string migrationsDir)
        {
            var result = Invoke<IDictionary>(
                "ScaffoldInitialCreate",
                new Hashtable
                {
                    ["connectionStringName"] = connectionStringName,
                    ["connectionString"] = connectionString,
                    ["connectionProviderName"] = connectionProviderName,
                    ["contextTypeName"] = contextTypeName,
                    ["contextAssemblyName"] = contextAssemblyName,
                    ["migrationsNamespace"] = migrationsNamespace,
                    ["auto"] = auto,
                    ["migrationsDir"] = migrationsDir
                });
            if (result == null)
            {
                return null;
            }

            var scaffoldedMigration = new ScaffoldedMigration
            {
                MigrationId = (string)result["MigrationId"],
                UserCode = (string)result["UserCode"],
                DesignerCode = (string)result["DesignerCode"],
                Language = (string)result["Language"],
                Directory = (string)result["Directory"],
                IsRescaffold = (bool)result["IsRescaffold"]
            };

            foreach (DictionaryEntry entry in (IDictionary)result["Resources"])
            {
                scaffoldedMigration.Resources.Add((string)entry.Key, entry.Value);
            }

            return scaffoldedMigration;
        }

        protected abstract dynamic CreateResultHandler();
        protected abstract void Execute(string operation, object resultHandler, IDictionary args);

        private TResult Invoke<TResult>(string operation, IDictionary args)
            => (TResult)InvokeImpl(operation, args);

        private object InvokeImpl(string operation, IDictionary args)
        {
            var resultHandler = CreateResultHandler();

            Execute(operation, resultHandler, args);

            if (resultHandler.ErrorType != null)
            {
                throw new WrappedException(
                    resultHandler.ErrorType,
                    resultHandler.ErrorMessage,
                    resultHandler.ErrorStackTrace);
            }

            return resultHandler.Result;
        }
    }
}
