namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    /// <summary>
    ///     Represents the configuration for a specific context type
    /// </summary>
    internal class ContextElement : ConfigurationElement
    {
        private const string _typeKey = "type";
        private const string _disableDatabaseInitializationKey = "disableDatabaseInitialization";
        private const string _databaseInitializerKey = "databaseInitializer";

        [ConfigurationProperty(_typeKey, IsRequired = true)]
        public string ContextTypeName
        {
            get { return (string)this[_typeKey]; }
            set { this[_typeKey] = value; }
        }

        [ConfigurationProperty(_disableDatabaseInitializationKey, DefaultValue = false)]
        public bool IsDatabaseInitializationDisabled
        {
            get { return (bool)this[_disableDatabaseInitializationKey]; }
            set { this[_disableDatabaseInitializationKey] = value; }
        }

        [ConfigurationProperty(_databaseInitializerKey)]
        public DatabaseInitializerElement DatabaseInitializer
        {
            get { return (DatabaseInitializerElement)this[_databaseInitializerKey]; }
            set { this[_databaseInitializerKey] = value; }
        }

        public Type GetContextType()
        {
            return Type.GetType(ContextTypeName, throwOnError: true);
        }
    }
}
