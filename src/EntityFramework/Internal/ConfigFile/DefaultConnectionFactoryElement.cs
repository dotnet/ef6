namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    /// <summary>
    ///     Represents setting the default connection factory
    /// </summary>
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