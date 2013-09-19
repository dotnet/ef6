// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    /// Responsible for obtaining <see cref="DbProviderServices" /> Singleton instances.
    /// </summary>
    internal class ProviderServicesFactory
    {
        public virtual DbProviderServices TryGetInstance(string providerTypeName)
        {
            DebugCheck.NotEmpty(providerTypeName);

            var providerType = Type.GetType(providerTypeName, throwOnError: false);

            return providerType == null ? null : GetInstance(providerType);
        }

        public virtual DbProviderServices GetInstance(string providerTypeName, string providerInvariantName)
        {
            DebugCheck.NotEmpty(providerTypeName);
            DebugCheck.NotEmpty(providerInvariantName);

            var providerType = Type.GetType(providerTypeName, throwOnError: false);

            if (providerType == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_ProviderTypeMissing(providerTypeName, providerInvariantName));
            }

            return GetInstance(providerType);
        }

        private static DbProviderServices GetInstance(Type providerType)
        {
            DebugCheck.NotNull(providerType);

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var instanceMember = providerType.GetStaticProperty("Instance")
                                 ?? (MemberInfo)providerType.GetField("Instance", bindingFlags);
            if (instanceMember == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_InstanceMissing(providerType.AssemblyQualifiedName));
            }

            var providerInstance = instanceMember.GetValue() as DbProviderServices;
            if (providerInstance == null)
            {
                throw new InvalidOperationException(Strings.EF6Providers_NotDbProviderServices(providerType.AssemblyQualifiedName));
            }

            return providerInstance;
        }
    }
}
