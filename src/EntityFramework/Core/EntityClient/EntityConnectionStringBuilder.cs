namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class representing a connection string builder for the entity client provider
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "EntityConnectionStringBuilder follows the naming convention of DbConnectionStringBuilder.")]
    [SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers",
        Justification = "There is no applicable strongly-typed implementation of CopyTo.")]
    public sealed class EntityConnectionStringBuilder : DbConnectionStringBuilder
    {
        // Names of parameters to look for in the connection string
        internal const string NameParameterName = "name";
        internal const string MetadataParameterName = "metadata";
        internal const string ProviderParameterName = "provider";
        internal const string ProviderConnectionStringParameterName = "provider connection string";

        // An array to hold the keywords
        private static readonly string[] _validKeywords = new[]
            {
                NameParameterName,
                MetadataParameterName,
                ProviderParameterName,
                ProviderConnectionStringParameterName
            };

        private static Hashtable _synonyms;

        // Information and data used by the connection
        private string _namedConnectionName;
        private string _providerName;
        private string _metadataLocations;
        private string _storeProviderConnectionString;

        /// <summary>
        /// Constructs the EntityConnectionStringBuilder object
        /// </summary>
        public EntityConnectionStringBuilder()
        {
            // Everything just defaults to null
        }

        /// <summary>
        /// Constructs the EntityConnectionStringBuilder object with a connection string
        /// </summary>
        /// <param name="connectionString">The connection string to initialize this builder</param>
        public EntityConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the named connection name in the connection string
        /// </summary>
        [DisplayName("Name")]
        [EntityResCategory(EntityRes.EntityDataCategory_NamedConnectionString)]
        [EntityResDescription(EntityRes.EntityConnectionString_Name)]
        [RefreshProperties(RefreshProperties.All)]
        public string Name
        {
            get { return _namedConnectionName ?? ""; }
            set
            {
                _namedConnectionName = value;
                base[NameParameterName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the underlying .NET Framework data provider in the connection string
        /// </summary>
        [DisplayName("Provider")]
        [EntityResCategory(EntityRes.EntityDataCategory_Source)]
        [EntityResDescription(EntityRes.EntityConnectionString_Provider)]
        [RefreshProperties(RefreshProperties.All)]
        public string Provider
        {
            get { return _providerName ?? ""; }
            set
            {
                _providerName = value;
                base[ProviderParameterName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the metadata locations in the connection string, which is a pipe-separated sequence
        /// of paths to folders and individual files
        /// </summary>
        [DisplayName("Metadata")]
        [EntityResCategory(EntityRes.EntityDataCategory_Context)]
        [EntityResDescription(EntityRes.EntityConnectionString_Metadata)]
        [RefreshProperties(RefreshProperties.All)]
        public string Metadata
        {
            get { return _metadataLocations ?? ""; }
            set
            {
                _metadataLocations = value;
                base[MetadataParameterName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the inner connection string in the connection string
        /// </summary>
        [DisplayName("Provider Connection String")]
        [EntityResCategory(EntityRes.EntityDataCategory_Source)]
        [EntityResDescription(EntityRes.EntityConnectionString_ProviderConnectionString)]
        [RefreshProperties(RefreshProperties.All)]
        public string ProviderConnectionString
        {
            get { return _storeProviderConnectionString ?? ""; }
            set
            {
                _storeProviderConnectionString = value;
                base[ProviderConnectionStringParameterName] = value;
            }
        }

        /// <summary>
        /// Gets whether the EntityConnectionStringBuilder has a fixed size
        /// </summary>
        public override bool IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a collection of all keywords used by EntityConnectionStringBuilder
        /// </summary>
        public override ICollection Keys
        {
            get { return new ReadOnlyCollection<string>(_validKeywords); }
        }

        /// <summary>
        /// Returns a hash table object containing all the valid keywords. This is really the same as the Keys
        /// property, it's just that the returned object is a hash table.
        /// </summary>
        internal static Hashtable Synonyms
        {
            get
            {
                // Build the synonyms table if we don't have one
                if (_synonyms == null)
                {
                    var table = new Hashtable(_validKeywords.Length);
                    foreach (var keyword in _validKeywords)
                    {
                        table.Add(keyword, keyword);
                    }
                    _synonyms = table;
                }
                return _synonyms;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the keyword
        /// </summary>
        public override object this[string keyword]
        {
            get
            {
                Contract.Requires(keyword != null);

                // Just access the properties to get the value since the fields, which the properties will be accessing, will
                // have already been set when the connection string is set
                if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Metadata;
                }
                else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ProviderConnectionString;
                }
                else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Name;
                }
                else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Provider;
                }

                throw new ArgumentException(Strings.EntityClient_KeywordNotSupported(keyword));
            }
            set
            {
                Contract.Requires(keyword != null);

                // If a null value is set, just remove the parameter and return
                if (value == null)
                {
                    Remove(keyword);
                    return;
                }

                // Since all of our parameters must be string value, perform the cast here and check
                var stringValue = value as string;
                if (stringValue == null)
                {
                    throw new ArgumentException(Strings.EntityClient_ValueNotString, "value");
                }

                // Just access the properties to get the value since the fields, which the properties will be accessing, will
                // have already been set when the connection string is set
                if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Metadata = stringValue;
                }
                else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ProviderConnectionString = stringValue;
                }
                else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Name = stringValue;
                }
                else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Provider = stringValue;
                }
                else
                {
                    throw new ArgumentException(Strings.EntityClient_KeywordNotSupported(keyword));
                }
            }
        }

        /// <summary>
        /// Clear all the parameters in the connection string
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _namedConnectionName = null;
            _providerName = null;
            _metadataLocations = null;
            _storeProviderConnectionString = null;
        }

        /// <summary>
        /// Determine if this connection string builder contains a specific key
        /// </summary>
        /// <param name="keyword">The keyword to find in this connection string builder</param>
        /// <returns>True if this connections string builder contains the specific key</returns>
        public override bool ContainsKey(string keyword)
        {
            Contract.Requires(keyword != null);

            foreach (var validKeyword in _validKeywords)
            {
                if (validKeyword.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the value of the given keyword, returns false if there isn't a value with the given keyword
        /// </summary>
        /// <param name="keyword">The keyword specifying the name of the parameter to retrieve</param>
        /// <param name="value">The value retrieved</param>
        /// <returns>True if the value is retrieved</returns>
        public override bool TryGetValue(string keyword, out object value)
        {
            Contract.Requires(keyword != null);

            if (ContainsKey(keyword))
            {
                value = this[keyword];
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Removes a parameter from the builder
        /// </summary>
        /// <param name="keyword">The keyword specifying the name of the parameter to remove</param>
        /// <returns>True if the parameter is removed</returns>
        public override bool Remove(string keyword)
        {
            Contract.Requires(keyword != null);

            // Convert the given object into a string
            if (string.Compare(keyword, MetadataParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _metadataLocations = null;
            }
            else if (string.Compare(keyword, ProviderConnectionStringParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _storeProviderConnectionString = null;
            }
            else if (string.Compare(keyword, NameParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _namedConnectionName = null;
            }
            else if (string.Compare(keyword, ProviderParameterName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _providerName = null;
            }

            return base.Remove(keyword);
        }
    }
}
