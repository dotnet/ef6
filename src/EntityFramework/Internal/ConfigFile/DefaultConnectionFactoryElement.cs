// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents setting the default connection factory
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class DefaultConnectionFactoryElement : ConfigurationElement
    {
        private const string _typeKey = "type";
        private const string _parametersKey = "parameters";

        [ConfigurationProperty(_typeKey, IsRequired = true)]
        public string FactoryTypeName
        {
            get { return (string)this[_typeKey]; }
            set { this[_typeKey] = value; }
        }

        [ConfigurationProperty(_parametersKey)]
        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)base[_parametersKey]; }
        }

        public Type GetFactoryType()
        {
            return Type.GetType(FactoryTypeName, throwOnError: true);
        }
    }
}
