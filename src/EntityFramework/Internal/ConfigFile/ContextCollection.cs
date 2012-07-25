// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents the configuration for a series of contexts
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ContextCollection : ConfigurationElementCollection
    {
        private const string _contextKey = "context";

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContextElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContextElement)element).ContextTypeName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return _contextKey; }
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            var key = GetElementKey(element);
            if (BaseGet(key) != null)
            {
                throw Error.ContextConfiguredMultipleTimes(key);
            }

            base.BaseAdd(element);
        }

        protected override void BaseAdd(int index, ConfigurationElement element)
        {
            var key = GetElementKey(element);
            if (BaseGet(key) != null)
            {
                throw Error.ContextConfiguredMultipleTimes(key);
            }

            base.BaseAdd(index, element);
        }

        /// <summary>
        ///     Adds a new context to the collection
        ///     Used for unit testing
        /// </summary>
        internal ContextElement NewElement(string contextTypeName)
        {
            var element = (ContextElement)CreateNewElement();
            base.BaseAdd(element);
            element.ContextTypeName = contextTypeName;
            return element;
        }
    }
}
