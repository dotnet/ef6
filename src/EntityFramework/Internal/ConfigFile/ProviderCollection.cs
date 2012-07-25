// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    internal class ProviderCollection : ConfigurationElementCollection
    {
        private const string ProviderKey = "provider";

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderElement)element).InvariantName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return ProviderKey; }
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            var key = GetElementKey(element);
            if (BaseGet(key) != null)
            {
                throw new InvalidOperationException(Strings.ProviderInvariantRepeatedInConfig(key));
            }

            base.BaseAdd(element);
        }

        protected override void BaseAdd(int index, ConfigurationElement element)
        {
            var key = GetElementKey(element);
            if (BaseGet(key) != null)
            {
                throw new InvalidOperationException(Strings.ProviderInvariantRepeatedInConfig(key));
            }

            base.BaseAdd(index, element);
        }

        public ProviderElement AddProvider(string invariantName, string providerTypeName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(invariantName));
            Contract.Requires(!string.IsNullOrWhiteSpace(providerTypeName));

            var element = (ProviderElement)CreateNewElement();
            base.BaseAdd(element);
            element.InvariantName = invariantName;
            element.ProviderTypeName = providerTypeName;
            return element;
        }
    }
}
