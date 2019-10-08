// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
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

            return ToScaffoldedMigration(result);
        }

        private static ScaffoldedMigration ToScaffoldedMigration(IDictionary result)
        {
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

        public ScaffoldedMigration Scaffold(
                string name,
                string connectionStringName,
                string connectionString,
                string connectionProviderName,
                string migrationsConfigurationName,
                bool ignoreChanges)
        {
            var result = Invoke<IDictionary>(
                "Scaffold",
                new Hashtable
                {
                    ["name"] = name,
                    ["connectionStringName"] = connectionStringName,
                    ["connectionString"] = connectionString,
                    ["connectionProviderName"] = connectionProviderName,
                    ["migrationsConfigurationName"] = migrationsConfigurationName,
                    ["ignoreChanges"] = ignoreChanges
                });

            return ToScaffoldedMigration(result);
        }

        public IEnumerable<string> GetDatabaseMigrations(
                string connectionStringName,
                string connectionString,
                string connectionProviderName,
                string migrationsConfigurationName)
            => Invoke<IEnumerable<string>>(
                "GetDatabaseMigrations",
                new Hashtable
                {
                    ["connectionStringName"] = connectionStringName,
                    ["connectionString"] = connectionString,
                    ["connectionProviderName"] = connectionProviderName,
                    ["migrationsConfigurationName"] = migrationsConfigurationName
                });

        public string ScriptUpdate(
                string sourceMigration,
                string targetMigration,
                bool force,
                string connectionStringName,
                string connectionString,
                string connectionProviderName,
                string migrationsConfigurationName)
            => Invoke<string>(
                "ScriptUpdate",
                new Hashtable
                {
                    ["sourceMigration"] = sourceMigration,
                    ["targetMigration"] = targetMigration,
                    ["force"] = force,
                    ["connectionStringName"] = connectionStringName,
                    ["connectionString"] = connectionString,
                    ["connectionProviderName"] = connectionProviderName,
                    ["migrationsConfigurationName"] = migrationsConfigurationName
                });

        public void Update(
                string targetMigration,
                bool force,
                string connectionStringName,
                string connectionString,
                string connectionProviderName,
                string migrationsConfigurationName)
            => Invoke<object>(
                "Update",
                new Hashtable
                {
                    ["targetMigration"] = targetMigration,
                    ["force"] = force,
                    ["connectionStringName"] = connectionStringName,
                    ["connectionString"] = connectionString,
                    ["connectionProviderName"] = connectionProviderName,
                    ["migrationsConfigurationName"] = migrationsConfigurationName
                });

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
