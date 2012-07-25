// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    ///     Represents a parameter to be passed to a method
    /// </summary>
    internal class ParameterElement : ConfigurationElement
    {
        private const string _valueKey = "value";
        private const string _typeKey = "type";

        public ParameterElement(int key)
        {
            Key = key;
        }

        internal int Key { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(_valueKey, IsRequired = true)]
        public string ValueString
        {
            get { return (string)this[_valueKey]; }
            set { this[_valueKey] = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(_typeKey, DefaultValue = "System.String")]
        public string TypeName
        {
            get { return (string)this[_typeKey]; }
            set { this[_typeKey] = value; }
        }

        public object GetTypedParameterValue()
        {
            var type = Type.GetType(TypeName, throwOnError: true);

            return Convert.ChangeType(ValueString, type, CultureInfo.InvariantCulture);
        }
    }
}
