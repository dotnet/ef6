// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// Class for representing a collection of items in Store space.
    /// </summary>
    public partial class StoreItemCollection : ItemCollection
    {
        private double _schemaVersion = XmlConstants.UndefinedVersion;

        // Cache for primitive type maps for Edm to provider
        private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();
        private readonly Memoizer<EdmFunction, EdmFunction> _cachedCTypeFunction;

        private readonly DbProviderManifest _providerManifest;
        private readonly string _providerInvariantName;
        private readonly string _providerManifestToken;
        private readonly DbProviderFactory _providerFactory;

        // Storing the query cache manager in the store item collection since all queries are currently bound to the
        // store. So storing it in StoreItemCollection makes sense. Also, since query cache requires version and other
        // stuff of the provider, we can assume that the connection is always open and we have the store metadata.
        // Also we can use the same cache manager both for Entity Client and Object Query, since query cache has
        // no reference to any metadata in OSpace. Also we assume that ObjectMaterializer loads the assembly
        // before it tries to do object materialization, since we might not have loaded an assembly in another workspace
        // where this store item collection is getting reused
        private readonly QueryCacheManager _queryCacheManager = QueryCacheManager.Create();

        // <summary>
        // For testing purposes only.
        // </summary>
        internal StoreItemCollection()
            : base(DataSpace.SSpace)
        {
        }

        // used by EntityStoreSchemaGenerator to start with an empty (primitive types only) StoreItemCollection and 
        // add types discovered from the database
        internal StoreItemCollection(
            DbProviderFactory factory, DbProviderManifest manifest, string providerInvariantName, string providerManifestToken)
            : base(DataSpace.SSpace)
        {
            DebugCheck.NotNull(factory);
            DebugCheck.NotNull(manifest);

            _providerFactory = factory;
            _providerManifest = manifest;
            _providerInvariantName = providerInvariantName;
            _providerManifestToken = providerManifestToken;
            _cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);
            LoadProviderManifest(_providerManifest);
        }

        // <summary>
        // constructor that loads the metadata files from the specified xmlReaders, and returns the list of errors
        // encountered during load as the out parameter errors.
        // </summary>
        // <param name="xmlReaders">xmlReaders where the CDM schemas are loaded</param>
        // <param name="filePaths">the paths where the files can be found that match the xml readers collection</param>
        // <param name="resolver"> </param>
        // <param name="errors">An out parameter to return the collection of errors encountered while loading</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        private StoreItemCollection(
            IEnumerable<XmlReader> xmlReaders,
            ReadOnlyCollection<string> filePaths,
            IDbDependencyResolver resolver,
            out IList<EdmSchemaError> errors)
            : base(DataSpace.SSpace)
        {
            DebugCheck.NotNull(xmlReaders);

            errors = Init(
                xmlReaders, filePaths, /* throwOnError */ false, resolver,
                out _providerManifest,
                out _providerFactory,
                out _providerInvariantName,
                out _providerManifestToken,
                out _cachedCTypeFunction);
        }

        // <summary>
        // constructor that loads the metadata files from the specified xmlReaders, and returns the list of errors
        // encountered during load as the out parameter errors.
        // </summary>
        // <param name="xmlReaders"> xmlReaders where the CDM schemas are loaded </param>
        // <param name="filePaths"> the paths where the files can be found that match the xml readers collection </param>
        internal StoreItemCollection(
            IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> filePaths)
            : base(DataSpace.SSpace)
        {
            DebugCheck.NotNull(filePaths);
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");

            Init(
                xmlReaders, filePaths, /* throwOnError */ true, /* resolver */ null,
                out _providerManifest,
                out _providerFactory,
                out _providerInvariantName,
                out _providerManifestToken,
                out _cachedCTypeFunction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" /> class using the specified XMLReader.
        /// </summary>
        /// <param name="xmlReaders">The XMLReader used to create metadata.</param>
        public StoreItemCollection(IEnumerable<XmlReader> xmlReaders)
            : base(DataSpace.SSpace)
        {
            Check.NotNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");

            var composite = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);
            Init(
                composite.GetReaders(),
                composite.GetPaths(),
                /* throwOnError */ true,
                /* resolver */ null,
                out _providerManifest,
                out _providerFactory,
                out _providerInvariantName,
                out _providerManifestToken,
                out _cachedCTypeFunction);
        }

        /// <summary>Initializes a new instances of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" /> class.</summary>
        /// <param name="model">The model of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" />.</param>
        public StoreItemCollection(EdmModel model)
            : base(DataSpace.SSpace)
        {
            Check.NotNull(model, "model");
            DebugCheck.NotNull(model.ProviderInfo);
            DebugCheck.NotNull(model.ProviderManifest);

            _providerManifest = model.ProviderManifest;
            _providerInvariantName = model.ProviderInfo.ProviderInvariantName;
            _providerFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(_providerInvariantName);
            _providerManifestToken = model.ProviderInfo.ProviderManifestToken;
            _cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);

            LoadProviderManifest(_providerManifest);

            _schemaVersion = model.SchemaVersion;

            model.Validate();

            foreach (var globalItem in model.GlobalItems)
            {
                globalItem.SetReadOnly();

                AddInternal(globalItem);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.StoreItemCollection" /> class using the specified file paths.
        /// </summary>
        /// <param name="filePaths">The file paths used to create metadata.</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but we do not create the file paths in this method 
        public StoreItemCollection(params string[] filePaths)
            : base(DataSpace.SSpace)
        {
            Check.NotNull(filePaths, "filePaths");
            IEnumerable<string> enumerableFilePaths = filePaths;
            EntityUtil.CheckArgumentEmpty(ref enumerableFilePaths, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");

            // Wrap the file paths in instances of the MetadataArtifactLoader class, which provides
            // an abstraction and a uniform interface over a diverse set of metadata artifacts.
            //
            MetadataArtifactLoader composite = null;
            List<XmlReader> readers = null;
            try
            {
                composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(enumerableFilePaths, XmlConstants.SSpaceSchemaExtension);
                readers = composite.CreateReaders(DataSpace.SSpace);
                var ieReaders = readers.AsEnumerable();
                EntityUtil.CheckArgumentEmpty(ref ieReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");

                Init(
                    readers,
                    composite.GetPaths(DataSpace.SSpace), /* throwOnError */ true, /* resolver */ null,
                    out _providerManifest,
                    out _providerFactory,
                    out _providerInvariantName,
                    out _providerManifestToken,
                    out _cachedCTypeFunction);
            }
            finally
            {
                if (readers != null)
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }

        private IList<EdmSchemaError> Init(
            IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> filePaths,
            bool throwOnError,
            IDbDependencyResolver resolver,
            out DbProviderManifest providerManifest,
            out DbProviderFactory providerFactory,
            out string providerInvariantName,
            out string providerManifestToken,
            out Memoizer<EdmFunction, EdmFunction> cachedCTypeFunction)
        {
            DebugCheck.NotNull(xmlReaders);
            // 'filePaths' can be null

            cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);

            var loader = new Loader(xmlReaders, filePaths, throwOnError, resolver);
            providerFactory = loader.ProviderFactory;
            providerManifest = loader.ProviderManifest;
            providerManifestToken = loader.ProviderManifestToken;
            providerInvariantName = loader.ProviderInvariantName;

            // load the items into the colleciton
            if (!loader.HasNonWarningErrors)
            {
                LoadProviderManifest(loader.ProviderManifest /* check for system namespace */);
                var errorList = EdmItemCollection.LoadItems(_providerManifest, loader.Schemas, this);
                foreach (var error in errorList)
                {
                    loader.Errors.Add(error);
                }

                if (throwOnError && errorList.Count != 0)
                {
                    loader.ThrowOnNonWarningErrors();
                }
            }

            return loader.Errors;
        }

        // <summary>
        // Returns the query cache manager
        // </summary>
        internal QueryCacheManager QueryCacheManager
        {
            get { return _queryCacheManager; }
        }

        /// <summary>Gets the provider factory of the StoreItemCollection.</summary>
        /// <returns>The provider factory of the StoreItemCollection.</returns>
        public virtual DbProviderFactory ProviderFactory
        {
            get { return _providerFactory; }
        }

        /// <summary>Gets the provider manifest of the StoreItemCollection.</summary>
        /// <returns>The provider manifest of the StoreItemCollection.</returns>
        public virtual DbProviderManifest ProviderManifest
        {
            get { return _providerManifest; }
        }

        /// <summary>Gets the manifest token of the StoreItemCollection.</summary>
        /// <returns>The manifest token of the StoreItemCollection.</returns>
        public virtual string ProviderManifestToken
        {
            get { return _providerManifestToken; }
        }

        /// <summary>Gets the invariant name of the StoreItemCollection.</summary>
        /// <returns>The invariant name of the StoreItemCollection.</returns>
        public virtual string ProviderInvariantName
        {
            get { return _providerInvariantName; }
        }

        /// <summary>Gets the version of the store schema for this collection.</summary>
        /// <returns>The version of the store schema for this collection.</returns>
        public Double StoreSchemaVersion
        {
            get { return _schemaVersion; }
            internal set { _schemaVersion = value; }
        }

        /// <summary>
        /// Returns a collection of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.PrimitiveType" /> objects.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> object that represents the collection of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.PrimitiveType" />
        /// objects.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes()
        {
            return _primitiveTypeMaps.GetTypes();
        }

        // <summary>
        // Given the canonical primitive type, get the mapping primitive type in the given dataspace
        // </summary>
        // <param name="primitiveTypeKind"> canonical primitive type </param>
        // <returns> The mapped scalar type </returns>
        internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            PrimitiveType type = null;
            _primitiveTypeMaps.TryGetType(primitiveTypeKind, null, out type);
            return type;
        }

        // <summary>
        // checks if the schemaKey refers to the provider manifest schema key
        // and if true, loads the provider manifest
        // </summary>
        // <param name="storeManifest"> The store manifest </param>
        private void LoadProviderManifest(DbProviderManifest storeManifest)
        {
            foreach (var primitiveType in storeManifest.GetStoreTypes())
            {
                //Add it to the collection and the primitive type maps
                AddInternal(primitiveType);
                _primitiveTypeMaps.Add(primitiveType);
            }

            foreach (var function in storeManifest.GetStoreFunctions())
            {
                AddInternal(function);
            }
        }

        // <summary>
        // Get all the overloads of the function with the given name, this method is used for internal perspective
        // </summary>
        // <param name="functionName"> The full name of the function </param>
        // <param name="ignoreCase"> true for case-insensitive lookup </param>
        // <returns> A collection of all the functions with the given name in the given data space </returns>
        // <exception cref="System.ArgumentNullException">Thrown if functionaName argument passed in is null</exception>
        internal ReadOnlyCollection<EdmFunction> GetCTypeFunctions(string functionName, bool ignoreCase)
        {
            ReadOnlyCollection<EdmFunction> functionOverloads;

            if (FunctionLookUpTable.TryGetValue(functionName, out functionOverloads))
            {
                functionOverloads = ConvertToCTypeFunctions(functionOverloads);
                if (ignoreCase)
                {
                    return functionOverloads;
                }

                return GetCaseSensitiveFunctions(functionOverloads, functionName);
            }

            return Helper.EmptyEdmFunctionReadOnlyCollection;
        }

        private ReadOnlyCollection<EdmFunction> ConvertToCTypeFunctions(
            ReadOnlyCollection<EdmFunction> functionOverloads)
        {
            var cTypeFunctions = new List<EdmFunction>();
            foreach (var sTypeFunction in functionOverloads)
            {
                cTypeFunctions.Add(ConvertToCTypeFunction(sTypeFunction));
            }
            return new ReadOnlyCollection<EdmFunction>(cTypeFunctions);
        }

        internal EdmFunction ConvertToCTypeFunction(EdmFunction sTypeFunction)
        {
            return _cachedCTypeFunction.Evaluate(sTypeFunction);
        }

        // <summary>
        // Convert the S type function parameters and returnType to C types.
        // </summary>
        private EdmFunction ConvertFunctionSignatureToCType(EdmFunction sTypeFunction)
        {
            Debug.Assert(sTypeFunction.DataSpace == DataSpace.SSpace, "sTypeFunction.DataSpace == Edm.DataSpace.SSpace");

            if (sTypeFunction.IsFromProviderManifest)
            {
                return sTypeFunction;
            }

            FunctionParameter returnParameter = null;
            if (sTypeFunction.ReturnParameter != null)
            {
                var edmTypeUsageReturnParameter =
                    MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(sTypeFunction.ReturnParameter.TypeUsage);

                returnParameter =
                    new FunctionParameter(
                        sTypeFunction.ReturnParameter.Name,
                        edmTypeUsageReturnParameter,
                        sTypeFunction.ReturnParameter.GetParameterMode());
            }

            var parameters = new List<FunctionParameter>();
            if (sTypeFunction.Parameters.Count > 0)
            {
                foreach (var parameter in sTypeFunction.Parameters)
                {
                    var edmTypeUsage = MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(parameter.TypeUsage);

                    var edmTypeParameter = new FunctionParameter(parameter.Name, edmTypeUsage, parameter.GetParameterMode());
                    parameters.Add(edmTypeParameter);
                }
            }

            var returnParameters =
                returnParameter == null ? new FunctionParameter[0] : new[] { returnParameter };
            var edmFunction = new EdmFunction(
                sTypeFunction.Name,
                sTypeFunction.NamespaceName,
                DataSpace.CSpace,
                new EdmFunctionPayload
                    {
                        Schema = sTypeFunction.Schema,
                        StoreFunctionName = sTypeFunction.StoreFunctionNameAttribute,
                        CommandText = sTypeFunction.CommandTextAttribute,
                        IsAggregate = sTypeFunction.AggregateAttribute,
                        IsBuiltIn = sTypeFunction.BuiltInAttribute,
                        IsNiladic = sTypeFunction.NiladicFunctionAttribute,
                        IsComposable = sTypeFunction.IsComposableAttribute,
                        IsFromProviderManifest = sTypeFunction.IsFromProviderManifest,
                        IsCachedStoreFunction = true,
                        IsFunctionImport = sTypeFunction.IsFunctionImport,
                        ReturnParameters = returnParameters,
                        Parameters = parameters.ToArray(),
                        ParameterTypeSemantics = sTypeFunction.ParameterTypeSemanticsAttribute,
                    });

            edmFunction.SetReadOnly();

            return edmFunction;
        }

        /// <summary>
        /// Factory method that creates a <see cref="StoreItemCollection" />.
        /// </summary>
        /// <param name="xmlReaders">
        /// SSDL artifacts to load. Must not be <c>null</c>.
        /// </param>
        /// <param name="filePaths">
        /// Paths to SSDL artifacts. Used in error messages. Can be <c>null</c> in which case
        /// the base Uri of the XmlReader will be used as a path.
        /// </param>
        /// <param name="resolver">
        /// Custom resolver. Currently used to resolve DbProviderServices implementation. If <c>null</c>
        /// the default resolver will be used.
        /// </param>
        /// <param name="errors">
        /// The collection of errors encountered while loading.
        /// </param>
        /// <returns>
        /// <see cref="StoreItemCollection" /> instance if no errors encountered. Otherwise <c>null</c>.
        /// </returns>
        public static StoreItemCollection Create(
            IEnumerable<XmlReader> xmlReaders,
            ReadOnlyCollection<string> filePaths,
            IDbDependencyResolver resolver,
            out IList<EdmSchemaError> errors)
        {
            Check.NotNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReaders");

            var storeItemCollection = new StoreItemCollection(xmlReaders, filePaths, resolver, out errors);

            return errors != null && errors.Count > 0 ? null : storeItemCollection;
        }
    }
}
