namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    internal class ProviderElement : ConfigurationElement
    {
        private const string InvariantNameKey = "invariantName";
        private const string TypeKey = "type";

        [ConfigurationProperty(InvariantNameKey, IsRequired = true)]
        public string InvariantName
        {
            get { return (string)this[InvariantNameKey]; }
            set { this[InvariantNameKey] = value; }
        }

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public string ProviderTypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }
    }
}