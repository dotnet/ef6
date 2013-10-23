// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class DependencyResolver : IDbDependencyResolver
    {
        public static readonly DependencyResolver Instance = new DependencyResolver();

        private static readonly EnglishPluralizationService PluralizationService = new EnglishPluralizationService();
        private static readonly DbProviderServicesResolver ProviderServicesResolver = new DbProviderServicesResolver();

        private DependencyResolver()
        {
        }

        public static T GetService<T>(object key = null) where T : class
        {
            return (T)Instance.GetService(typeof(T), key);
        }

        public object GetService(Type type, object key)
        {
            if (type == typeof(IPluralizationService))
            {
                return PluralizationService;
            }

            return ProviderServicesResolver.GetService(type, key);
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key);
            return service != null ? new[] { service } : Enumerable.Empty<object>();
        }

        public static void RegisterProvider(Type type, string invariantName)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(
                typeof(DbProviderServices).IsAssignableFrom(type),
                "expected type derived from DbProviderServices");
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName cannot be null or empty string");

            ProviderServicesResolver.Register(type, invariantName);
        }

        public static void UnregisterProvider(string invariantName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName cannot be null or empty string");

            ProviderServicesResolver.Unregister(invariantName);
        }

        public static void EnsureProvider(string invariantName, Type type)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName is null or empty.");

            if (type == null)
            {
                UnregisterProvider(invariantName);
            }
            else
            {
                RegisterProvider(type, invariantName);
            }
        }
    }
}
