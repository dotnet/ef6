// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    internal class ConfigLoadedHandlerElement : ConfigurationElement
    {
        private const string TypeKey = "type";
        private const string MethodKey = "method";

        public ConfigLoadedHandlerElement(int key)
        {
            Key = key;
        }

        internal int Key { get; private set; }

        [ConfigurationProperty(MethodKey, IsRequired = true)]
        public string MethodName
        {
            get { return (string)this[MethodKey]; }
            set { this[MethodKey] = value; }
        }

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public string TypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }

        public virtual EventHandler<DbConfigurationLoadedEventArgs> CreateHandlerDelegate()
        {
            var providerType = Type.GetType(TypeName, throwOnError: false);

            if (providerType == null)
            {
                throw new InvalidOperationException(Strings.ConfigEventTypeNotFound(TypeName));
            }

            var methodInfo = providerType.GetDeclaredMethod(MethodName, typeof(object), typeof(DbConfigurationLoadedEventArgs));

            if (methodInfo == null
                || !methodInfo.IsStatic)
            {
                throw new InvalidOperationException(Strings.ConfigEventBadMethod(MethodName, TypeName));
            }

            try
            {
                return (EventHandler<DbConfigurationLoadedEventArgs>)Delegate.CreateDelegate(
                    typeof(EventHandler<DbConfigurationLoadedEventArgs>), methodInfo, throwOnBindFailure: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ConfigEventCannotBind(MethodName, TypeName), ex);
            }
        }
    }
}
