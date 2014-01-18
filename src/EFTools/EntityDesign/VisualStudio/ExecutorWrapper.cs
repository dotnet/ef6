// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    // <summary>
    //     Used for design-time scenarios where the user's code needs to be executed inside
    //     of an isolated, runtime-like <see cref="AppDomain" />.
    // </summary>
    internal class ExecutorWrapper
    {
        private const string EntityFrameworkAssemblyName = "EntityFramework";
        private const string ExecutorTypeFullName = "System.Data.Entity.Infrastructure.Design.Executor";

        private const BindingFlags DefaultBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

        private readonly AppDomain _domain;
        private readonly Lazy<object> _executor;

        // <summary>
        //     Initializes a new instance of the <see cref="ExecutorWrapper" /> class.  Consider using
        //     <see cref="ProjectExecutionContext" /> instead.
        // </summary>
        // <param name="domain">
        //     A runtime-like <see cref="AppDomain" /> used to execute the user's code.
        // </param>
        // <param name="assemblyFile">The path for the assembly containing the user's code.</param>
        public ExecutorWrapper(AppDomain domain, string assemblyFile)
        {
            Debug.Assert(domain != null, "domain is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(assemblyFile), "assemblyFile is null or empty.");

            _domain = domain;

            _executor = new Lazy<object>(
                () => domain.CreateInstanceAndUnwrap(
                    EntityFrameworkAssemblyName,
                    ExecutorTypeFullName,
                    false,
                    DefaultBindingFlags,
                    null,
                    new[] { assemblyFile, null },
                    null,
                    null));
        }

        // <summary>
        //     Gets the DbProviderServices type name for the specified provider.
        // </summary>
        // <param name="invariantName">The provider's invariant name.</param>
        // <returns>The assembly-qualified name</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string GetProviderServices(string invariantName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName is null or empty.");

            string providerServicesTypeName = null;
            try
            {
                providerServicesTypeName = Invoke<string>("GetProviderServices", new[] { invariantName });
            }
            catch
            {
            }

            return providerServicesTypeName;
        }

        private TResult Invoke<TResult>(string operation, IEnumerable<object> args, object anonymousArguments = null)
        {
            Debug.Assert(args != null, "args is null.");

            var handler = new ResultHandler<TResult>();
            Invoke(operation, handler, args, anonymousArguments);

            return handler.Result;
        }

        private void Invoke(string operation, HandlerBase handler, IEnumerable<object> args, object anonymousArguments)
        {
            Debug.Assert(handler != null, "handler is null.");
            Debug.Assert(args != null, "args is null.");

            var realArgs = new List<object>();
            realArgs.Add(_executor.Value);
            realArgs.Add(handler);
            realArgs.AddRange(args);
            realArgs.Add(ConvertAnonymousObjectToDictionary(anonymousArguments));

            _domain.CreateInstance(
                EntityFrameworkAssemblyName,
                ExecutorTypeFullName + "+" + operation,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                realArgs.ToArray(),
                null,
                null);
        }

        private static IDictionary<string, object> ConvertAnonymousObjectToDictionary(object anonymousObject)
        {
            var dictionary = new Dictionary<string, object>();

            if (anonymousObject != null)
            {
                foreach (var p in anonymousObject.GetType().GetProperties())
                {
                    dictionary.Add(p.Name, p.GetValue(anonymousObject, null));
                }
            }

            return dictionary;
        }

        private class ResultHandler<T> : HandlerBase, IResultHandler
        {
            private T _result;

            public T Result
            {
                get { return _result; }
            }

            void IResultHandler.SetResult(object value)
            {
                _result = (T)(value ?? default(T));
            }
        }
    }
}
