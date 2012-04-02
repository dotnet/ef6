namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    /// <summary>
    ///     Represents setting the database initializer for a specific context type
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class DatabaseInitializerElement : ConfigurationElement
    {
        private const string _typeKey = "type";
        private const string _parametersKey = "parameters";

        [ConfigurationProperty(_typeKey, IsRequired = true)]
        public string InitializerTypeName
        {
            get { return (string)this[_typeKey]; }
            set { this[_typeKey] = value; }
        }

        [ConfigurationProperty(_parametersKey)]
        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)base[_parametersKey]; }
        }

        public Type GetInitializerType()
        {
            return Type.GetType(InitializerTypeName, throwOnError: true);
        }
    }
}
