// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents setting the database initializer for a specific context type
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class DatabaseInitializerElement : ConfigurationElement
    {
        private const string TypeKey = "type";
        private const string ParametersKey = "parameters";

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public virtual string InitializerTypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }

        [ConfigurationProperty(ParametersKey)]
        public virtual ParameterCollection Parameters
        {
            get { return (ParameterCollection)base[ParametersKey]; }
        }
    }
}
