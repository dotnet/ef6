namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), ConfigurationProperty(_valueKey, IsRequired = true)]
        public string ValueString
        {
            get { return (string)this[_valueKey]; }
            set { this[_valueKey] = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), ConfigurationProperty(_typeKey, DefaultValue = "System.String")]
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
