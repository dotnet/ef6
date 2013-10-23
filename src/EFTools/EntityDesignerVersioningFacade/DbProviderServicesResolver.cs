// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal class DbProviderServicesResolver : IDbDependencyResolver
    {
        private static readonly LegacyDbProviderServicesResolver LegacyDbProviderServicesResolver =
            new LegacyDbProviderServicesResolver();

        private readonly Dictionary<string, Type> _providerServicesRegistrar = new Dictionary<string, Type>();

        public void Register(Type type, string invariantName)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(
                typeof(DbProviderServices).IsAssignableFrom(type),
                "expected type derived from DbProviderServices");
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName cannot be null or empty string");

            _providerServicesRegistrar[invariantName] = type;
        }

        public void Unregister(string invariantName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(invariantName), "invariantName cannot be null or empty string");
            _providerServicesRegistrar.Remove(invariantName);
        }

        public object GetService(Type type, object key)
        {
            var providerInvariantName = key as string;
            if (type == typeof(DbProviderServices)
                && providerInvariantName != null)
            {
                Debug.Assert(
                    providerInvariantName != "Microsoft.SqlServerCe.Client.4.0",
                    "providerInvariantName is for design-time.");

                Type providerServicesType;
                return
                    _providerServicesRegistrar.TryGetValue(providerInvariantName, out providerServicesType)
                        ? CreateProviderInstance(providerServicesType)
                        : LegacyDbProviderServicesResolver.GetService(type, key);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            var service = GetService(type, key);
            return service != null ? new[] { service } : Enumerable.Empty<object>();
        }

        private static DbProviderServices CreateProviderInstance(Type providerType)
        {
            Debug.Assert(providerType != null, "providerType != null");
            Debug.Assert(
                typeof(DbProviderServices).IsAssignableFrom(providerType),
                "expected type derived from DbProviderServices");

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var instanceMember = providerType.GetProperty("Instance", bindingFlags)
                                 ?? (MemberInfo)providerType.GetField("Instance", bindingFlags);

            if (instanceMember == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.EF6Providers_InstanceMissing,
                        providerType.AssemblyQualifiedName));
            }

            var providerInstance = GetInstanceValue(instanceMember) as DbProviderServices;
            if (providerInstance == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.EF6Providers_NotDbProviderServices,
                        providerType.AssemblyQualifiedName));
            }

            return providerInstance;
        }

        private static object GetInstanceValue(MemberInfo memberInfo)
        {
            var asPropertyInfo = memberInfo as PropertyInfo;
            if (asPropertyInfo != null)
            {
                return asPropertyInfo.GetValue(null, null);
            }
            return ((FieldInfo)memberInfo).GetValue(null);
        }
    }
}
