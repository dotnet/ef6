// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Design;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Used for design-time scenarios where the user's code needs to be executed inside
    /// of an isolated, runtime-like <see cref="AppDomain" />.
    ///
    /// Instances of this class should be created inside of the guest domain.
    /// Handlers should be created inside of the host domain. To invoke operations,
    /// create instances of the nested classes inside
    /// </summary>
    public class Executor : MarshalByRefObject
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly Assembly _assembly;
        private readonly Reporter _reporter;
        private readonly string _language;
        private readonly string _rootNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="Executor" /> class. Do this inside of the guest
        /// domain.
        /// </summary>
        /// <param name="assemblyFile">The path for the assembly containing the user's code.</param>
        /// <param name="anonymousArguments">The parameter is not used.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "anonymousArguments")]
        public Executor(string assemblyFile, IDictionary<string, object> anonymousArguments)
        {
            Check.NotEmpty(assemblyFile, "assemblyFile");

            _reporter = new Reporter(new WrappedReportHandler(anonymousArguments?["reportHandler"]));
            _language = (string)anonymousArguments?["language"];
            _rootNamespace = (string)anonymousArguments?["rootNamespace"];

            _assembly = Assembly.Load(
                AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFile)));
        }

        private Assembly LoadAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return null;
            }

            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (FileNotFoundException ex)
            {
                throw new MigrationsException(
                    Strings.ToolingFacade_AssemblyNotFound(ex.FileName),
                    ex);
            }
        }

        private string GetContextTypeInternal(string contextTypeName, string contextAssemblyName)
        {
            var contextAssembly = LoadAssembly(contextAssemblyName) ?? _assembly;
            var contextType = new TypeFinder(contextAssembly).FindType(
                typeof(DbContext),
                contextTypeName,
                types => types.Where(t => !typeof(HistoryContext).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType),
                Error.EnableMigrations_NoContext,
                (assembly, types) =>
                {
                    var message = new StringBuilder();
                    message.Append(Strings.EnableMigrations_MultipleContexts(assembly));

                    foreach (var type in types)
                    {
                        message.AppendLine();
                        message.Append(Strings.EnableMigrationsForContext(type.FullName));
                    }

                    return new MigrationsException(message.ToString());
                },
                Error.EnableMigrations_NoContextWithName,
                Error.EnableMigrations_MultipleContextsWithName);

            return contextType.FullName;
        }

        public class GetContextType : OperationBase
        {
            public GetContextType(Executor executor, object resultHandler, IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(resultHandler, nameof(resultHandler));
                Check.NotNull(args, nameof(args));

                var contextTypeName = (string)args["contextTypeName"];
                var contextAssemblyName = (string)args["contextAssemblyName"];

                Execute(() => executor.GetContextTypeInternal(contextTypeName, contextAssemblyName));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal virtual string GetProviderServicesInternal(string invariantName)
        {
            DebugCheck.NotEmpty(invariantName);

            DbConfiguration.LoadConfiguration(_assembly);
            var dependencyResolver = DbConfiguration.DependencyResolver;

            DbProviderServices providerServices = null;
            try
            {
                providerServices = dependencyResolver.GetService<DbProviderServices>(invariantName);
            }
            catch
            {
            }
            if (providerServices == null)
            {
                return null;
            }

            return providerServices.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Used to get the assembly-qualified name of the DbProviderServices type for the
        /// specified provider invariant name.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        internal class GetProviderServices : OperationBase
        {
            // <summary>
            // Initializes a new instance of the <see cref="GetProviderServices" /> class. Do this inside of
            // the guest domain.
            // </summary>
            // <param name="executor">The executor used to execute this operation.</param>
            // <param name="handler">An object to handle callbacks during the operation.</param>
            // <param name="invariantName">The provider's invariant name.</param>
            // <param name="anonymousArguments">The parameter is not used.</param>
            // <seealso cref="HandlerBase" />
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "anonymousArguments")]
            public GetProviderServices(
                Executor executor,
                object handler,
                string invariantName,
                IDictionary<string, object> anonymousArguments)
                : base(handler)
            {
                Check.NotNull(executor, "executor");
                Check.NotEmpty(invariantName, "invariantName");

                Execute(() => executor.GetProviderServicesInternal(invariantName));
            }
        }

        internal virtual IDictionary ScaffoldInitialCreateInternal(
            DbConnectionInfo connectionInfo,
            string contextTypeName,
            string contextAssemblyName,
            string migrationsNamespace,
            bool auto,
            string migrationsDir)
        {
            var contextAssembly = LoadAssembly(contextAssemblyName) ?? _assembly;
            var configuration = new DbMigrationsConfiguration
            {
                ContextType = contextAssembly.GetType(contextTypeName, throwOnError: true),
                MigrationsAssembly = _assembly,
                MigrationsNamespace = migrationsNamespace,
                AutomaticMigrationsEnabled = auto,
                MigrationsDirectory = migrationsDir
            };

            if (connectionInfo != null)
            {
                configuration.TargetDatabase = connectionInfo;
            }
            if (string.Equals(_language, "VB", StringComparison.OrdinalIgnoreCase)
                && configuration.CodeGenerator is CSharpMigrationCodeGenerator)
            {
                // If the user hasn't set their own generator and he/she is using a VB project then switch in the default VB one
                configuration.CodeGenerator = new VisualBasicMigrationCodeGenerator();
            }

            var scaffolder = new MigrationScaffolder(configuration);

            var @namespace = configuration.MigrationsNamespace;

            // Need to strip project namespace when generating code for VB projects
            // (The VB compiler automatically prefixes the project namespace)
            if (string.Equals(_language, "VB", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(_rootNamespace))
            {
                if (_rootNamespace.EqualsIgnoreCase(@namespace))
                {
                    @namespace = null;
                }
                else if (@namespace != null
                         && @namespace.StartsWith(_rootNamespace + ".", StringComparison.OrdinalIgnoreCase))
                {
                    @namespace = @namespace.Substring(_rootNamespace.Length + 1);
                }
                else
                {
                    throw Error.MigrationsNamespaceNotUnderRootNamespace(@namespace, _rootNamespace);
                }
            }

            scaffolder.Namespace = @namespace;

            var result = scaffolder.ScaffoldInitialCreate();

            return result == null
                ? null
                : new Hashtable
                {
                    ["MigrationId"] = result.MigrationId,
                    ["UserCode"] = result.UserCode,
                    ["DesignerCode"] = result.DesignerCode,
                    ["Language"] = result.Language,
                    ["Directory"] = result.Directory,
                    ["Resources"] = result.Resources,
                    ["IsRescaffold"] = result.IsRescaffold
                };
        }

        public class ScaffoldInitialCreate : OperationBase
        {
            public ScaffoldInitialCreate(Executor executor, object resultHandler, IDictionary args)
                : base(resultHandler)
            {
                Check.NotNull(executor, nameof(executor));
                Check.NotNull(resultHandler, nameof(resultHandler));
                Check.NotNull(args, nameof(args));

                var connectionStringName = (string)args["connectionStringName"];
                var connectionString = (string)args["connectionString"];
                var connectionProviderName = (string)args["connectionProviderName"];
                var contextTypeName = (string)args["contextTypeName"];
                var contextAssemblyName = (string)args["contextAssemblyName"];
                var migrationsNamespace = (string)args["migrationsNamespace"];
                var auto = (bool)args["auto"];
                var migrationsDir = (string)args["migrationsDir"];

                Execute(
                    () => executor.ScaffoldInitialCreateInternal(
                        CreateConnectionInfo(connectionStringName, connectionString, connectionProviderName),
                        contextTypeName,
                        contextAssemblyName,
                        migrationsNamespace,
                        auto,
                        migrationsDir));
            }
        }

        /// <summary>
        ///     Represents an operation.
        /// </summary>
        public abstract class OperationBase : MarshalByRefObject
        {
            private readonly WrappedResultHandler _handler;

            /// <summary>
            /// Initializes a new instance of the <see cref="OperationBase" /> class.
            /// </summary>
            /// <param name="handler">An object to handle callbacks during the operation.</param>
            protected OperationBase(object handler)
            {
                Check.NotNull(handler, nameof(handler));

                _handler = new WrappedResultHandler(handler);
            }

            protected static DbConnectionInfo CreateConnectionInfo(
                string connectionStringName,
                string connectionString,
                string connectionProviderName)
            {
                if (!string.IsNullOrWhiteSpace(connectionStringName))
                {
                    return new DbConnectionInfo(connectionStringName);
                }
                else if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    return new DbConnectionInfo(connectionString, connectionProviderName);
                }

                return null;
            }

            /// <summary>
            ///     Executes an action passing exceptions to the handler.
            /// </summary>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute(Action action)
            {
                Check.NotNull(action, nameof(action));

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (!_handler.SetError(ex.GetType().FullName, ex.Message, ex.ToString()))
                    {
                        throw;
                    }
                }
            }

            /// <summary>
            ///     Executes an action passing the result or exceptions to the handler.
            /// </summary>
            /// <typeparam name="T"> The result type. </typeparam>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute<T>(Func<T> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _handler.SetResult(action()));
            }

            /// <summary>
            ///     Executes an action passing results or exceptions to the handler.
            /// </summary>
            /// <typeparam name="T"> The type of results. </typeparam>
            /// <param name="action"> The action to execute. </param>
            protected virtual void Execute<T>(Func<IEnumerable<T>> action)
            {
                Check.NotNull(action, nameof(action));

                Execute(() => _handler.SetResult(action().ToArray()));
            }
        }
    }
}
