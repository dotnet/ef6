namespace System.Data.Entity.Core.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.EntityModel.SchemaObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
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

        protected DbXmlEnabledProviderManifest(XmlReader reader)
        {
            if (reader == null)
            {
                throw EntityUtil.ProviderIncompatible(Strings.IncorrectProviderManifest, new ArgumentNullException("reader"));
            }

            Load(reader);
        }

        #region Protected Properties For Fields

        public override string NamespaceName
        {
            get { return _namespaceName; }
        }

        protected Dictionary<string, PrimitiveType> StoreTypeNameToEdmPrimitiveType
        {
            get { return _storeTypeNameToEdmPrimitiveType; }
        }

        protected Dictionary<string, PrimitiveType> StoreTypeNameToStorePrimitiveType
        {
            get { return _storeTypeNameToStorePrimitiveType; }
        }

        #endregion

        /// <summary>
        /// Returns all the FacetDescriptions for a particular edmType
        /// </summary>
        /// <param name="edmType">the edmType to return FacetDescriptions for.</param>
        /// <returns>The FacetDescriptions for the edmType given.</returns>
        public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType)
        {
            Debug.Assert(edmType is PrimitiveType, "DbXmlEnabledProviderManifest.GetFacetDescriptions(): Argument is not a PrimitiveType");
            return GetReadOnlyCollection(edmType as PrimitiveType, _facetDescriptions, Helper.EmptyFacetDescriptionEnumerable);
        }

        public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            return _primitiveTypes;
        }

        /// <summary>
        /// Returns all the edm functions supported by the provider manifest.
        /// </summary>
        /// <returns>A collection of edm functions.</returns>
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
                throw EntityUtil.ProviderIncompatible(Strings.IncorrectProviderManifest + Helper.CombineErrorMessage(errors));
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
