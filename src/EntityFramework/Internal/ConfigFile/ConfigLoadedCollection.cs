// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Linq;

    internal class ConfigLoadedCollection : ConfigurationElementCollection
    {
        private const string HandlerKey = "handler";
        private int _nextKey;

        protected override ConfigurationElement CreateNewElement()
        {
            var element = new ConfigLoadedHandlerElement(_nextKey);
            _nextKey++;
            return element;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConfigLoadedHandlerElement)element).Key;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return HandlerKey; }
        }

        public virtual IEnumerable<EventHandler<DbConfigurationLoadedEventArgs>> RegisteredHandlers
        {
            get { return this.OfType<ConfigLoadedHandlerElement>().Select(e => e.CreateHandlerDelegate()).ToList(); }
        }

        public void AddElement(ConfigLoadedHandlerElement element)
        {
            base.BaseAdd(element);
        }
    }
}
