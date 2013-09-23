// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Represents all Entity Framework related configuration
    // </summary>
    internal class EntityFrameworkSection : ConfigurationSection
    {
        private const string DefaultConnectionFactoryKey = "defaultConnectionFactory";
        private const string ContextsKey = "contexts";
        private const string ProviderKey = "providers";
        private const string ConfigurationTypeKey = "codeConfigurationType";

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(DefaultConnectionFactoryKey)]
        public virtual DefaultConnectionFactoryElement DefaultConnectionFactory
        {
            get { return (DefaultConnectionFactoryElement)this[DefaultConnectionFactoryKey]; }
            set { this[DefaultConnectionFactoryKey] = value; }
        }

        [ConfigurationProperty(ConfigurationTypeKey)]
        public virtual string ConfigurationTypeName
        {
            get { return (string)this[ConfigurationTypeKey]; }
            set { this[ConfigurationTypeKey] = value; }
        }

        [ConfigurationProperty(ProviderKey)]
        public virtual ProviderCollection Providers
        {
            get { return (ProviderCollection)base[ProviderKey]; }
        }

        [ConfigurationProperty(ContextsKey)]
        public virtual ContextCollection Contexts
        {
            get { return (ContextCollection)base[ContextsKey]; }
        }
    }
}
