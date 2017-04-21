// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Linq;

    internal class InterceptorsCollection : ConfigurationElementCollection
    {
        private const string ElementKey = "interceptor";
        private int _nextKey;

        protected override ConfigurationElement CreateNewElement()
        {
            return new InterceptorElement(_nextKey++);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InterceptorElement)element).Key;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return ElementKey; }
        }

        public void AddElement(InterceptorElement element)
        {
            base.BaseAdd(element);
        }

        public virtual IEnumerable<IDbInterceptor> Interceptors
        {
            get { return this.OfType<InterceptorElement>().Select(e => e.CreateInterceptor()); }
        }
    }
}
