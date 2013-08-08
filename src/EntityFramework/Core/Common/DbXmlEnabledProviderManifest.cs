// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Core.SchemaObjectModel;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    /// <summary>
    /// A specialization of the ProviderManifest that accepts an XmlReader
    /// </summary>
    public abstract class DbXmlEnabledProviderManifest : DbProviderManifest
    {
        private string _namespaceName;

        private ReadOnlyCollection<PrimitiveType> _primitiveTypes;

        private readonly Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>> _facetDescriptions =
            new Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>>();

        private ReadOnlyCollection<EdmFunction> _functions;

        private readonly Dictionary<string, PrimitiveType> _storeTypeNameToEdmPrimitiveType = new Dictionary<string, PrimitiveType>();
        private readonly Dictionary<string, PrimitiveType> _storeTypeNameToStorePrimitiveType = new Dictionary<string, PrimitiveType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Common.DbXmlEnabledProviderManifest" /> class.
        /// </summary>
        /// <param name="reader">
        /// An <see cref="T:System.Xml.XmlReader" /> object that provides access to the XML data in the provider manifest file.
        /// </param>
        protected DbXmlEnabledProviderManifest(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ProviderIncompatibleException(Strings.IncorrectProviderManifest, new ArgumentNullException("reader"));
            }

            Load(reader);
        }

        #region Protected Properties For Fields

        /// <summary>Gets the namespace name supported by this provider manifest.</summary>
        /// <returns>The namespace name supported by this provider manifest.</returns>
        public override string NamespaceName
        {
            get { return _namespaceName; }
        }

        /// <summary>Gets the best mapped equivalent Entity Data Model (EDM) type for a specified storage type name.</summary>
        /// <returns>The best mapped equivalent EDM type for a specified storage type name.</returns>
        protected Dictionary<string, PrimitiveType> StoreTypeNameToEdmPrimitiveType
        {
            get { return _storeTypeNameToEdmPrimitiveType; }
        }

        /// <summary>Gets the best mapped equivalent storage primitive type for a specified storage type name.</summary>
        /// <returns>The best mapped equivalent storage primitive type for a specified storage type name.</returns>
        protected Dictionary<string, PrimitiveType> StoreTypeNameToStorePrimitiveType
        {
            get { return _storeTypeNameToStorePrimitiveType; }
        }

        #endregion

        /// <summary>Returns the list of facet descriptions for the specified Entity Data Model (EDM) type.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains the list of facet descriptions for the specified EDM type.
        /// </returns>
        /// <param name="edmType">
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> for which the facet descriptions are to be retrieved.
        /// </param>
        public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType)
        {
            Debug.Assert(edmType is PrimitiveType, "DbXmlEnabledProviderManifest.GetFacetDescriptions(): Argument is not a PrimitiveType");
            return GetReadOnlyCollection(edmType as PrimitiveType, _facetDescriptions, Helper.EmptyFacetDescriptionEnumerable);
        }

        /// <summary>Returns the list of primitive types supported by the storage provider.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains the list of primitive types supported by the storage provider.
        /// </returns>
        public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            return _primitiveTypes;
        }

        /// <summary>Returns the list of provider-supported functions.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains the list of provider-supported functions.
        /// </returns>
        public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            return _functions;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void Load(XmlReader reader)
        {
            Schema schema;
            var errors = SchemaManager.LoadProviderManifest(
                reader, reader.BaseURI.Length > 0 ? reader.BaseURI : null, true /*checkForSystemNamespace*/, out schema);

            if (errors.Count != 0)
            {
                throw new ProviderIncompatibleException(Strings.IncorrectProviderManifest + Helper.CombineErrorMessage(errors));
            }

            _namespaceName = schema.Namespace;

            var listOfPrimitiveTypes = new List<PrimitiveType>();
            foreach (var schemaType in schema.SchemaTypes)
            {
                var typeElement = schemaType as TypeElement;
                if (typeElement != null)
                {
                    var type = typeElement.PrimitiveType;
                    type.ProviderManifest = this;
                    type.DataSpace = DataSpace.SSpace;
                    type.SetReadOnly();
                    listOfPrimitiveTypes.Add(type);

                    _storeTypeNameToStorePrimitiveType.Add(type.Name.ToLowerInvariant(), type);
                    _storeTypeNameToEdmPrimitiveType.Add(
                        type.Name.ToLowerInvariant(), EdmProviderManifest.Instance.GetPrimitiveType(type.PrimitiveTypeKind));

                    ReadOnlyCollection<FacetDescription> descriptions;
                    if (EnumerableToReadOnlyCollection(typeElement.FacetDescriptions, out descriptions))
                    {
                        _facetDescriptions.Add(type, descriptions);
                    }
                }
            }
            _primitiveTypes = Array.AsReadOnly(listOfPrimitiveTypes.ToArray());

            // load the functions
            ItemCollection collection = new EmptyItemCollection();
            var items = Converter.ConvertSchema(schema, this, collection);
            if (!EnumerableToReadOnlyCollection(items, out _functions))
            {
                _functions = Helper.EmptyEdmFunctionReadOnlyCollection;
            }
            //SetReadOnly on all the Functions
            foreach (var function in _functions)
            {
                function.SetReadOnly();
            }
        }

        private static ReadOnlyCollection<T> GetReadOnlyCollection<T>(
            PrimitiveType type, Dictionary<PrimitiveType, ReadOnlyCollection<T>> typeDictionary, ReadOnlyCollection<T> useIfEmpty)
        {
            ReadOnlyCollection<T> collection;
            if (typeDictionary.TryGetValue(type, out collection))
            {
                return collection;
            }
            else
            {
                return useIfEmpty;
            }
        }

        private static bool EnumerableToReadOnlyCollection<Target, BaseType>(
            IEnumerable<BaseType> enumerable, out ReadOnlyCollection<Target> collection) where Target : BaseType
        {
            var list = new List<Target>();
            foreach (var item in enumerable)
            {
                if (typeof(Target) == typeof(BaseType)
                    || item is Target)
                {
                    list.Add((Target)item);
                }
            }

            if (list.Count != 0)
            {
                collection = list.AsReadOnly();
                return true;
            }

            collection = null;
            return false;
        }

        private class EmptyItemCollection : ItemCollection
        {
            public EmptyItemCollection()
                : base(DataSpace.SSpace)
            {
            }
        }
    }
}
