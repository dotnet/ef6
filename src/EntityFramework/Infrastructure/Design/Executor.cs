// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;

    // <summary>
    // Used for design-time scenarios where the user's code needs to be executed inside
    // of an isolated, runtime-like <see cref="AppDomain" />.
    // 
    // Instances of this class should be created inside of the guest domain.
    // Handlers should be created inside of the host domain. To invoke operations,
    // create instances of the nested classes inside 
    // </summary>
    internal class Executor : MarshalByRefObject
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly Assembly _assembly;

        // <summary>
        // Initializes a new instance of the <see cref="Executor" /> class. Do this inside of the guest
        // domain.
        // </summary>
        // <param name="assemblyFile">The path for the assembly containing the user's code.</param>
        // <param name="anonymousArguments">The parameter is not used.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "anonymousArguments")]
        public Executor(string assemblyFile, IDictionary<string, object> anonymousArguments)
        {
            Check.NotEmpty(assemblyFile, "assemblyFile");

            _assembly = Assembly.Load(
                AssemblyName.GetAssemblyName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyFile)));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal virtual string GetProviderServicesInternal(string invariantName)
        {
            DebugCheck.NotEmpty(invariantName);

            string providerServicesTypeName = null;
            try
            {
                DbConfiguration.LoadConfiguration(_assembly);
                var providerServices = DbConfiguration.DependencyResolver.GetService<DbProviderServices>(invariantName);
                providerServicesTypeName = providerServices.GetType().AssemblyQualifiedName;
            }
            catch
            {
            }

            return providerServicesTypeName;
        }

        // <summary>
        // Used to get the assembly-qualified name of the DbProviderServices type for the
        // specified provider invariant name.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class GetProviderServices : MarshalByRefObject
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
            {
                Check.NotNull(executor, "executor");
                Check.NotNull(handler, "handler");
                Check.NotEmpty(invariantName, "invariantName");

                var wrappedHandler = new WrappedHandler(handler);

                var providerServicesTypeName = executor.GetProviderServicesInternal(invariantName);
                wrappedHandler.SetResult(providerServicesTypeName);
            }
        }
    }
}
