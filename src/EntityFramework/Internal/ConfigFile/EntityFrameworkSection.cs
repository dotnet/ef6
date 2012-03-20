namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    /// <summary>
    ///     Represents all Entity Framework related configuration
    /// </summary>
    internal class EntityFrameworkSection : ConfigurationSection
    {
        private const string _defaultConnectionFactoryKey = "defaultConnectionFactory";
        private const string _contextsKey = "contexts";

        [ConfigurationProperty(_defaultConnectionFactoryKey)]
        public DefaultConnectionFactoryElement DefaultConnectionFactory
        {
            get { return (DefaultConnectionFactoryElement)this[_defaultConnectionFactoryKey]; }
            set { this[_defaultConnectionFactoryKey] = value; }
        }

        [ConfigurationProperty(_contextsKey)]
        public ContextCollection Contexts
        {
            get { return (ContextCollection)base[_contextsKey]; }
        }
    }
}