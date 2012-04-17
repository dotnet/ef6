namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityModel.SchemaObjectModel;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Class for representing a collection of items in Edm space.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class EdmItemCollection : ItemCollection
    {
        /// <summary>
        /// constructor that loads the metadata files from the specified xmlReaders
        /// </summary>
        /// <param name="xmlReaders">xmlReaders where the CDM schemas are loaded</param>
        /// <param name="filePaths">Paths (URIs)to the CSDL files or resources</param>
        internal EdmItemCollection(
            IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> filePaths)
            : base(DataSpace.CSpace)
        {
            Init(xmlReaders, filePaths, true /*throwOnErrors*/);
        }

        /// <summary>
        /// Public constructor that loads the metadata files from the specified XmlReaders
        /// </summary>
        /// <param name="xmlReaders">XmlReader objects where the EDM schemas are loaded</param>
        public EdmItemCollection(IEnumerable<XmlReader> xmlReaders)
            : base(DataSpace.CSpace)
        {
            Contract.Requires(xmlReaders != null);
            EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");

            var composite = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);

            Init(
                composite.GetReaders(),
                composite.GetPaths(),
                true /*throwOnError*/);
        }

        /// <summary>
        /// Constructs the new instance of EdmItemCollection
        /// with the list of CDM files provided.
        /// </summary>
        /// <param name="paths">paths where the CDM schemas are loaded</param>
        /// <exception cref="ArgumentException"> Thrown if path name is not valid</exception>
        /// <exception cref="System.ArgumentNullException">thrown if paths argument is null</exception>
        /// <exception cref="System.Data.Entity.Core.MetadataException">For errors related to invalid schemas.</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but we do not create the file paths in this method 
        public EdmItemCollection(params string[] filePaths)
            : base(DataSpace.CSpace)
        {
            Contract.Requires(filePaths != null);

            // Wrap the file paths in instances of the MetadataArtifactLoader class, which provides
            // an abstraction and a uniform interface over a diverse set of metadata artifacts.
            //
            MetadataArtifactLoader composite = null;
            List<XmlReader> readers = null;
            try
            {
                composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(filePaths, XmlConstants.CSpaceSchemaExtension);
                readers = composite.CreateReaders(DataSpace.CSpace);
                Init(
                    readers,
                    composite.GetPaths(DataSpace.CSpace),
                    true /*throwOnError*/);
            }
            finally
            {
                if (readers != null)
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }

        // the most basic initialization
        private void Init()
        {
            // Load the EDM primitive types
            LoadEdmPrimitiveTypesAndFunctions();
        }

        /// <summary>
        /// Public constructor that loads the metadata files from the specified XmlReaders, and
        /// returns the list of errors encountered during load as the out parameter 'errors'.
        /// </summary>
        /// <param name="xmlReaders">XmlReader objects where the EDM schemas are loaded</param>
        /// <param name="filePaths">Paths (URIs) to the CSDL files or resources</param>
        /// <param name="throwOnError">A flag to indicate whether to throw if LoadItems returns errors</param>
        private IList<EdmSchemaError> Init(
            IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> filePaths,
            bool throwOnError)
        {
            Contract.Requires(xmlReaders != null);

            // do the basic initialization
            Init();

            var errors = LoadItems(
                xmlReaders, filePaths, SchemaDataModelOption.EntityDataModel,
                MetadataItem.EdmProviderManifest, this, throwOnError);

            return errors;
        }

        #region Fields

        // Cache for primitive type maps for Edm to provider
        private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();

        private Double _edmVersion = XmlConstants.UndefinedVersion;

        /// <summary>
        /// Gets canonical versions of InitializerMetadata instances. This avoids repeatedly
        /// compiling delegates for materialization.
        /// </summary>
        private Memoizer<InitializerMetadata, InitializerMetadata> _getCanonicalInitializerMetadataMemoizer;

        /// <summary>
        /// Manages user defined function definitions.
        /// </summary>
        private Memoizer<EdmFunction, DbLambda> _getGeneratedFunctionDefinitionsMemoizer;

        private readonly OcAssemblyCache _conventionalOcCache = new OcAssemblyCache();

        #endregion

        #region Properties

        /// <summary>
        /// Version of the EDM that this ItemCollection represents.
        /// </summary>
        public Double EdmVersion
        {
            get { return _edmVersion; }
            internal set { _edmVersion = value; }
        }

        /// <summary>
        /// conventional oc mapping cache, the locking mechanism is provided by AsssemblyCache
        /// </summary>
        internal OcAssemblyCache ConventionalOcCache
        {
            get { return _conventionalOcCache; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Given an InitializerMetadata instance, returns the canonical version of that instance.
        /// This allows us to avoid compiling materialization delegates repeatedly for the same
        /// pattern.
        /// </summary>
        internal InitializerMetadata GetCanonicalInitializerMetadata(InitializerMetadata metadata)
        {
            if (null == _getCanonicalInitializerMetadataMemoizer)
            {
                // We memoize the identity function because the first evaluation of the function establishes
                // the canonical 'reference' for the initializer metadata with a particular 'value'.
                Interlocked.CompareExchange(
                    ref _getCanonicalInitializerMetadataMemoizer, new Memoizer<InitializerMetadata, InitializerMetadata>(
                        m => m, EqualityComparer<InitializerMetadata>.Default), null);
            }

            // check if an equivalent has already been registered
            var canonical = _getCanonicalInitializerMetadataMemoizer.Evaluate(metadata);
            return canonical;
        }

        internal static bool IsSystemNamespace(DbProviderManifest manifest, string namespaceName)
        {
            if (manifest == MetadataItem.EdmProviderManifest)
            {
                return (namespaceName == EdmConstants.TransientNamespace ||
                        namespaceName == EdmConstants.EdmNamespace ||
                        namespaceName == EdmConstants.ClrPrimitiveTypeNamespace);
            }
            else
            {
                return (namespaceName == EdmConstants.TransientNamespace ||
                        namespaceName == EdmConstants.EdmNamespace ||
                        namespaceName == EdmConstants.ClrPrimitiveTypeNamespace ||
                        (manifest != null && namespaceName == manifest.NamespaceName));
            }
        }

        /// <summary>
        /// Load stuff from xml readers - this now includes XmlReader instances created over embedded
        /// resources. See the remarks section below for some useful information.
        /// </summary>
        /// <param name="xmlReaders">A list of XmlReader instances</param>
        /// <param name="dataModelOption">whether this is a entity data model or provider data model</param>
        /// <param name="providerManifest">provider manifest from which the primitive type definition comes from</param>
        /// <param name="itemCollection">item collection to add the item after loading</param>
        /// <param name="computeFilePaths">Indicates whether the method should bother with the file paths; see remarks below</param>
        /// <remarks>
        /// In order to accommodate XmlReaders over artifacts embedded as resources in assemblies, the
        /// notion of a filepath had to be abstracted into a URI. In reality, however, a res:// URI that
        /// points to an embedded resource does not constitute a valid URI (i.e., one that can be parsed
        /// by the System.Uri class in the .NET framework). In such cases, we need to supply a list of
        /// "filepaths" (which includes res:// URIs), instead of having this method create the collection.
        /// This distinction is made by setting the 'computeFilePaths' flags appropriately.
        /// </remarks>
        internal static IList<EdmSchemaError> LoadItems(
            IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> sourceFilePaths,
            SchemaDataModelOption dataModelOption,
            DbProviderManifest providerManifest,
            ItemCollection itemCollection,
            bool throwOnError)
        {
            IList<Schema> schemaCollection = null;

            // Parse and validate all the schemas - since we support using now,
            // we need to parse them as a group
            var errorCollection = SchemaManager.ParseAndValidate(
                xmlReaders, sourceFilePaths,
                dataModelOption, providerManifest, out schemaCollection);

            // Try to initialize the metadata if there are no errors
            if (MetadataHelper.CheckIfAllErrorsAreWarnings(errorCollection))
            {
                var errors = LoadItems(providerManifest, schemaCollection, itemCollection);
                foreach (var error in errors)
                {
                    errorCollection.Add(error);
                }
            }
            if (!MetadataHelper.CheckIfAllErrorsAreWarnings(errorCollection) && throwOnError)
            {
                //Future Enhancement: if there is an error, we throw exception with error and warnings.
                //Otherwise the user has no clue to know about warnings.
                throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(errorCollection));
            }
            return errorCollection;
        }

        internal static List<EdmSchemaError> LoadItems(
            DbProviderManifest manifest, IList<Schema> somSchemas,
            ItemCollection itemCollection)
        {
            var errors = new List<EdmSchemaError>();
            // Convert the schema, if model schema, then we use the EDM provider manifest, otherwise use the
            // store provider manifest
            var newGlobalItems = LoadSomSchema(somSchemas, manifest, itemCollection);
            var tempCTypeFunctionIdentity = new List<string>();

            // No errors, so go ahead and add the types and make them readonly
            foreach (var globalItem in newGlobalItems)
            {
                // If multiple function parameter and return types expressed in SSpace map to the same
                // CSpace type (e.g., SqlServer.decimal and SqlServer.numeric both map to Edm.Decimal),
                // we need to guard against attempts to insert duplicate functions into the collection.
                //
                if (globalItem.BuiltInTypeKind == BuiltInTypeKind.EdmFunction
                    && globalItem.DataSpace == DataSpace.SSpace)
                {
                    var function = (EdmFunction)globalItem;

                    var sb = new StringBuilder();
                    EdmFunction.BuildIdentity(
                        sb,
                        function.FullName,
                        function.Parameters,
                        // convert function parameters to C-side types
                        (param) => MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(param.TypeUsage),
                        (param) => param.Mode);
                    var cTypeFunctionIdentity = sb.ToString();

                    // Validate identity
                    if (tempCTypeFunctionIdentity.Contains(cTypeFunctionIdentity))
                    {
                        errors.Add(
                            new EdmSchemaError(
                                Strings.DuplicatedFunctionoverloads(
                                    function.FullName, cTypeFunctionIdentity.Substring(function.FullName.Length)).Trim() /*parameters*/,
                                (int)ErrorCode.DuplicatedFunctionoverloads,
                                EdmSchemaErrorSeverity.Error));
                        continue;
                    }

                    tempCTypeFunctionIdentity.Add(cTypeFunctionIdentity);
                }
                globalItem.SetReadOnly();
                itemCollection.AddInternal(globalItem);
            }
            return errors;
        }

        /// <summary>
        /// Load metadata from a SOM schema directly
        /// </summary>
        /// <param name="somSchema">The SOM schema to load from</param>
        /// <param name="providerManifest">The provider manifest used for loading the type</param>
        /// <param name="itemCollection">item collection in which primitive types are present</param>
        /// <returns>The newly created items</returns>
        internal static IEnumerable<GlobalItem> LoadSomSchema(
            IList<Schema> somSchemas,
            DbProviderManifest providerManifest,
            ItemCollection itemCollection)
        {
            var newGlobalItems = Converter.ConvertSchema(
                somSchemas,
                providerManifest, itemCollection);
            return newGlobalItems;
        }

        /// <summary>
        /// Get the list of primitive types for the given space
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes()
        {
            return _primitiveTypeMaps.GetTypes();
        }

        /// <summary>
        /// Get the list of primitive types for the given version of Edm
        /// </summary>
        /// <param name="edmVersion">The version of edm to use</param>
        /// <returns></returns>
        public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(double edmVersion)
        {
            if (edmVersion == XmlConstants.EdmVersionForV1 || edmVersion == XmlConstants.EdmVersionForV1_1
                || edmVersion == XmlConstants.EdmVersionForV2)
            {
                return _primitiveTypeMaps.GetTypes().Where(type => !Helper.IsSpatialType(type)).ToList().AsReadOnly();
            }
            else if (edmVersion == XmlConstants.EdmVersionForV3)
            {
                return _primitiveTypeMaps.GetTypes();
            }
            else
            {
                throw new ArgumentException(Strings.InvalidEDMVersion(edmVersion.ToString(CultureInfo.CurrentCulture)));
            }
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind">canonical primitive type</param>
        /// <returns>The mapped scalar type</returns>
        internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            PrimitiveType type = null;
            _primitiveTypeMaps.TryGetType(primitiveTypeKind, null, out type);
            return type;
        }

        private void LoadEdmPrimitiveTypesAndFunctions()
        {
            var providerManifest = EdmProviderManifest.Instance;
            var primitiveTypes = providerManifest.GetStoreTypes();
            for (var i = 0; i < primitiveTypes.Count; i++)
            {
                AddInternal(primitiveTypes[i]);
                _primitiveTypeMaps.Add(primitiveTypes[i]);
            }
            var functions = providerManifest.GetStoreFunctions();
            for (var i = 0; i < functions.Count; i++)
            {
                AddInternal(functions[i]);
            }
        }

        /// <summary>
        /// Generates function definition or returns a cached one.
        /// Guarantees type match of declaration and generated parameters.
        /// Guarantees return type match.
        /// Throws internal error for functions without definition.
        /// Passes thru exceptions occured during definition generation.
        /// </summary>
        internal DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
        {
            if (null == _getGeneratedFunctionDefinitionsMemoizer)
            {
                Interlocked.CompareExchange(
                    ref _getGeneratedFunctionDefinitionsMemoizer,
                    new Memoizer<EdmFunction, DbLambda>(GenerateFunctionDefinition, null),
                    null);
            }

            return _getGeneratedFunctionDefinitionsMemoizer.Evaluate(function);
        }

        /// <summary>
        /// Generates function definition or returns a cached one.
        /// Guarantees type match of declaration and generated parameters.
        /// Guarantees return type match.
        /// Throws internal error for functions without definition.
        /// Passes thru exceptions occured during definition generation.
        /// </summary>
        internal DbLambda GenerateFunctionDefinition(EdmFunction function)
        {
            Debug.Assert(function.IsModelDefinedFunction, "Function definition can be requested only for user-defined model functions.");
            if (!function.HasUserDefinedBody)
            {
                throw new InvalidOperationException(Strings.Cqt_UDF_FunctionHasNoDefinition(function.Identity));
            }

            DbLambda generatedDefinition;

            // Generate the body
            generatedDefinition = ExternalCalls.CompileFunctionDefinition(
                function.CommandTextAttribute,
                function.Parameters,
                this);

            // Ensure the result type of the generated definition matches the result type of the edm function (the declaration)
            if (!TypeSemantics.IsStructurallyEqual(function.ReturnParameter.TypeUsage, generatedDefinition.Body.ResultType))
            {
                throw new InvalidOperationException(Strings.Cqt_UDF_FunctionDefinitionResultTypeMismatch(
                    function.ReturnParameter.TypeUsage.ToString(),
                    function.FullName,
                    generatedDefinition.Body.ResultType.ToString()));
            }

            Debug.Assert(generatedDefinition != null, "generatedDefinition != null");

            return generatedDefinition;
        }

        #endregion
    }

//---- ItemCollection
}

//---- 
