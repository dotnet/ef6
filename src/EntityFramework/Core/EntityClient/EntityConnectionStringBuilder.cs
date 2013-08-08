// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

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
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> class.
        /// </summary>
        public EntityConnectionStringBuilder()
        {
            // Everything just defaults to null
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> class using the supplied connection string.
        /// </summary>
        /// <param name="connectionString">A provider-specific connection string to the underlying data source.</param>
        public EntityConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>Gets or sets the name of a section as defined in a configuration file.</summary>
        /// <returns>The name of a section in a configuration file.</returns>
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

        /// <summary>Gets or sets the name of the underlying .NET Framework data provider in the connection string.</summary>
        /// <returns>The invariant name of the underlying .NET Framework data provider.</returns>
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

        /// <summary>Gets or sets the metadata locations in the connection string.</summary>
        /// <returns>Gets or sets the metadata locations in the connection string.</returns>
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

        /// <summary>Gets or sets the inner, provider-specific connection string.</summary>
        /// <returns>The inner, provider-specific connection string.</returns>
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
        /// Gets a value that indicates whether the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// has a fixed size.
        /// </summary>
        /// <returns>
        /// Returns true in every case, because the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// supplies a fixed-size collection of keyword/value pairs.
        /// </returns>
        public override bool IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.ICollection" /> that contains the keys in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </returns>
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

        /// <summary>Gets or sets the value associated with the specified key. In C#, this property is the indexer.</summary>
        /// <returns>The value associated with the specified key. </returns>
        /// <param name="keyword">The key of the item to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Tried to add a key that does not exist in the available keys.</exception>
        /// <exception cref="T:System.FormatException">Invalid value in the connection string (specifically, a Boolean or numeric value was expected but not supplied).</exception>
        public override object this[string keyword]
        {
            get
            {
                Check.NotNull(keyword, "keyword");

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
                Check.NotNull(keyword, "keyword");

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
        /// Clears the contents of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> instance.
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
        /// Determines whether the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> contains a specific key.
        /// </summary>
        /// <returns>
        /// Returns true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" /> contains an element that has the specified key; otherwise, false.
        /// </returns>
        /// <param name="keyword">
        /// The key to locate in the <see cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />.
        /// </param>
        public override bool ContainsKey(string keyword)
        {
            Check.NotNull(keyword, "keyword");

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
        /// Retrieves a value corresponding to the supplied key from this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </summary>
        /// <returns>Returns true if  keyword  was found in the connection string; otherwise, false.</returns>
        /// <param name="keyword">The key of the item to retrieve.</param>
        /// <param name="value">The value corresponding to  keyword. </param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  contains a null value (Nothing in Visual Basic).</exception>
        public override bool TryGetValue(string keyword, out object value)
        {
            Check.NotNull(keyword, "keyword");

            if (ContainsKey(keyword))
            {
                value = this[keyword];
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Removes the entry with the specified key from the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// instance.
        /// </summary>
        /// <returns>Returns true if the key existed in the connection string and was removed; false if the key did not exist.</returns>
        /// <param name="keyword">
        /// The key of the keyword/value pair to be removed from the connection string in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder" />
        /// .
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> keyword  is null (Nothing in Visual Basic)</exception>
        public override bool Remove(string keyword)
        {
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
