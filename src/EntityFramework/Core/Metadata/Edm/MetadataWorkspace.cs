// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// Runtime Metadata Workspace
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class MetadataWorkspace
    {
        private Lazy<EdmItemCollection> _itemsCSpace;
        private Lazy<StoreItemCollection> _itemsSSpace;
        private Lazy<ObjectItemCollection> _itemsOSpace;
        private Lazy<StorageMappingItemCollection> _itemsCSSpace;
        private Lazy<DefaultObjectMappingItemCollection> _itemsOCSpace;

        private bool _foundAssemblyWithAttribute;
        private double _schemaVersion = XmlConstants.UndefinedVersion;
        private readonly object _schemaVersionLock = new object();
        private readonly Guid _metadataWorkspaceId = Guid.NewGuid();

        internal readonly MetadataOptimization MetadataOptimization;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> class.
        /// </summary>
        public MetadataWorkspace()
        {
            _itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);

            MetadataOptimization = new MetadataOptimization(this);
        }

        /// <summary>
        /// Constructs a <see cref="MetadataWorkspace" /> with loaders for all item collections (<see cref="ItemCollection" />)
        /// needed by EF except the o/c mapping which will be created automatically based on the given o-space and c-space
        /// loaders. The item collection delegates are executed lazily when a given collection is used for the first
        /// time. It is acceptable to pass a delegate that returns null if the collection will never be used, but this
        /// is rarely done, and any attempt by EF to use the collection in such cases will result in an exception.
        /// </summary>
        /// <param name="cSpaceLoader">Delegate to return the c-space (CSDL) item collection.</param>
        /// <param name="sSpaceLoader">Delegate to return the s-space (SSDL) item collection.</param>
        /// <param name="csMappingLoader">Delegate to return the c/s mapping (MSL) item collection.</param>
        /// <param name="oSpaceLoader">Delegate to return the o-space item collection.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public MetadataWorkspace(
            Func<EdmItemCollection> cSpaceLoader,
            Func<StoreItemCollection> sSpaceLoader,
            Func<StorageMappingItemCollection> csMappingLoader,
            Func<ObjectItemCollection> oSpaceLoader)
        {
            Check.NotNull(cSpaceLoader, "cSpaceLoader");
            Check.NotNull(sSpaceLoader, "sSpaceLoader");
            Check.NotNull(csMappingLoader, "csMappingLoader");
            Check.NotNull(oSpaceLoader, "oSpaceLoader");

            _itemsCSpace = new Lazy<EdmItemCollection>(() => LoadAndCheckItemCollection(cSpaceLoader), isThreadSafe: true);
            _itemsSSpace = new Lazy<StoreItemCollection>(() => LoadAndCheckItemCollection(sSpaceLoader), isThreadSafe: true);
            _itemsOSpace = new Lazy<ObjectItemCollection>(oSpaceLoader, isThreadSafe: true);
            _itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => LoadAndCheckItemCollection(csMappingLoader), isThreadSafe: true);
            _itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(
                () => new DefaultObjectMappingItemCollection(_itemsCSpace.Value, _itemsOSpace.Value), isThreadSafe: true);

            MetadataOptimization = new MetadataOptimization(this);
        }

        /// <summary>
        /// Constructs a <see cref="MetadataWorkspace" /> with loaders for all item collections (<see cref="ItemCollection" />)
        /// that come from traditional EDMX mapping. Default o-space and o/c mapping collections will be used.
        /// The item collection delegates are executed lazily when a given collection is used for the first
        /// time. It is acceptable to pass a delegate that returns null if the collection will never be used, but this
        /// is rarely done, and any attempt by EF to use the collection in such cases will result in an exception.
        /// </summary>
        /// <param name="cSpaceLoader">Delegate to return the c-space (CSDL) item collection.</param>
        /// <param name="sSpaceLoader">Delegate to return the s-space (SSDL) item collection.</param>
        /// <param name="csMappingLoader">Delegate to return the c/s mapping (MSL) item collection.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "c")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public MetadataWorkspace(
            Func<EdmItemCollection> cSpaceLoader,
            Func<StoreItemCollection> sSpaceLoader,
            Func<StorageMappingItemCollection> csMappingLoader)
        {
            Check.NotNull(cSpaceLoader, "cSpaceLoader");
            Check.NotNull(sSpaceLoader, "sSpaceLoader");
            Check.NotNull(csMappingLoader, "csMappingLoader");

            _itemsCSpace = new Lazy<EdmItemCollection>(() => LoadAndCheckItemCollection(cSpaceLoader), isThreadSafe: true);
            _itemsSSpace = new Lazy<StoreItemCollection>(() => LoadAndCheckItemCollection(sSpaceLoader), isThreadSafe: true);
            _itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);
            _itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => LoadAndCheckItemCollection(csMappingLoader), isThreadSafe: true);
            _itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(
                () => new DefaultObjectMappingItemCollection(_itemsCSpace.Value, _itemsOSpace.Value), isThreadSafe: true);

            MetadataOptimization = new MetadataOptimization(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> class using the specified paths and assemblies.
        /// </summary>
        /// <param name="paths">The paths to workspace metadata.</param>
        /// <param name="assembliesToConsider">The names of assemblies used to construct workspace.</param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        public MetadataWorkspace(IEnumerable<string> paths, IEnumerable<Assembly> assembliesToConsider)
        {
            // we are intentionally not checking to see if the paths enumerable is empty
            Check.NotNull(paths, "paths");
            Check.NotNull(assembliesToConsider, "assembliesToConsider");

            EntityUtil.CheckArgumentContainsNull(ref paths, "paths");
            EntityUtil.CheckArgumentContainsNull(ref assembliesToConsider, "assembliesToConsider");

            Func<AssemblyName, Assembly> resolveReference = (AssemblyName referenceName) =>
                {
                    foreach (var assembly in assembliesToConsider)
                    {
                        if (AssemblyName.ReferenceMatchesDefinition(
                            referenceName, new AssemblyName(assembly.FullName)))
                        {
                            return assembly;
                        }
                    }
                    throw new ArgumentException(
                        Strings.AssemblyMissingFromAssembliesToConsider(
                            referenceName.FullName), "assembliesToConsider");
                };

            CreateMetadataWorkspaceWithResolver(paths, () => assembliesToConsider, resolveReference);

            MetadataOptimization = new MetadataOptimization(this);
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but We do not create the file paths in this method 
        private void CreateMetadataWorkspaceWithResolver(
            IEnumerable<string> paths, Func<IEnumerable<Assembly>> wildcardAssemblies, Func<AssemblyName, Assembly> resolveReference)
        {
            var composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(
                paths.ToArray(), "", new CustomAssemblyResolver(wildcardAssemblies, resolveReference));

            _itemsOSpace = new Lazy<ObjectItemCollection>(() => new ObjectItemCollection(), isThreadSafe: true);

            using (var cSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(DataSpace.CSpace)))
            {
                if (cSpaceReaders.Any())
                {
                    var itemCollection = new EdmItemCollection(cSpaceReaders, composite.GetPaths(DataSpace.CSpace));
                    _itemsCSpace = new Lazy<EdmItemCollection>(() => itemCollection, isThreadSafe: true);
                    _itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(
                        () => new DefaultObjectMappingItemCollection(itemCollection, _itemsOSpace.Value), isThreadSafe: true);
                }
            }

            using (var sSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(DataSpace.SSpace)))
            {
                if (sSpaceReaders.Any())
                {
                    var itemCollection = new StoreItemCollection(sSpaceReaders, composite.GetPaths(DataSpace.SSpace));
                    _itemsSSpace = new Lazy<StoreItemCollection>(() => itemCollection, isThreadSafe: true);
                }
            }

            using (var csSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(DataSpace.CSSpace)))
            {
                if (csSpaceReaders.Any()
                    && _itemsCSpace != null
                    && _itemsSSpace != null)
                {
                    var mapping = new StorageMappingItemCollection(
                        _itemsCSpace.Value,
                        _itemsSSpace.Value,
                        csSpaceReaders,
                        composite.GetPaths(DataSpace.CSSpace));
                    _itemsCSSpace = new Lazy<StorageMappingItemCollection>(() => mapping, isThreadSafe: true);
                }
            }
        }

        private static IEnumerable<double> SupportedEdmVersions
        {
            get
            {
                yield return XmlConstants.UndefinedVersion;
                yield return XmlConstants.EdmVersionForV1;
                yield return XmlConstants.EdmVersionForV2;
                Debug.Assert(XmlConstants.SchemaVersionLatest == XmlConstants.EdmVersionForV3, "Did you add a new version?");
                yield return XmlConstants.EdmVersionForV3;
            }
        }

        private static readonly double _maximumEdmVersionSupported = SupportedEdmVersions.Last();

        /// <summary>
        /// The Max EDM version thats going to be supported by the runtime.
        /// </summary>
        public static double MaximumEdmVersionSupported
        {
            get { return _maximumEdmVersionSupported; }
        }

        internal virtual Guid MetadataWorkspaceId
        {
            get
            {
                return _metadataWorkspaceId;
            }
        }

        /// <summary>
        /// Creates an <see cref="T:System.Data.Entity.Core.Common.EntitySql.EntitySqlParser" /> configured to use the
        /// <see
        ///     cref="F:System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace" />
        /// data space.
        /// </summary>
        /// <returns>The created parser object.</returns>
        public virtual EntitySqlParser CreateEntitySqlParser()
        {
            return new EntitySqlParser(new ModelPerspective(this));
        }

        /// <summary>
        /// Creates a new <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQueryCommandTree" /> bound to this metadata workspace based on the specified query expression.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQueryCommandTree" /> with the specified expression as it's
        /// <see
        ///     cref="P:System.Data.Entity.Core.Common.CommandTrees.DbQueryCommandTree.Query" />
        /// property.
        /// </returns>
        /// <param name="query">
        /// A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the query.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If
        /// <paramref name="query" />
        /// is null
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If
        /// <paramref name="query" />
        /// contains metadata that cannot be resolved in this metadata workspace
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If
        /// <paramref name="query" />
        /// is not structurally valid because it contains unresolvable variable references
        /// </exception>
        public virtual DbQueryCommandTree CreateQueryCommandTree(DbExpression query)
        {
            return new DbQueryCommandTree(this, DataSpace.CSpace, query);
        }

        /// <summary>
        /// Gets <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> items.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" /> items.
        /// </returns>
        /// <param name="dataSpace">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.DataSpace" /> from which to retrieve items.
        /// </param>
        public virtual ItemCollection GetItemCollection(DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection;
        }

        /// <summary>Registers the item collection with each associated data model.</summary>
        /// <param name="collection">The output parameter collection that needs to be filled up.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [Obsolete("Construct MetadataWorkspace using constructor that accepts metadata loading delegates.")]
        public virtual void RegisterItemCollection(ItemCollection collection)
        {
            Check.NotNull(collection, "collection");

            try
            {
                switch (collection.DataSpace)
                {
                    case DataSpace.CSpace:
                        var edmCollection = (EdmItemCollection)collection;
                        if (!SupportedEdmVersions.Contains(edmCollection.EdmVersion))
                        {
                            throw new InvalidOperationException(
                                Strings.EdmVersionNotSupportedByRuntime(
                                    edmCollection.EdmVersion,
                                    Helper.GetCommaDelimitedString(
                                        SupportedEdmVersions
                                            .Where(e => e != XmlConstants.UndefinedVersion)
                                            .Select(e => e.ToString(CultureInfo.InvariantCulture)))));
                        }

                        CheckAndSetItemCollectionVersionInWorkSpace(collection);
                        _itemsCSpace = new Lazy<EdmItemCollection>(() => edmCollection, isThreadSafe: true);
                        if (_itemsOCSpace == null)
                        {
                            Debug.Assert(_itemsOSpace != null);
                            _itemsOCSpace =
                                new Lazy<DefaultObjectMappingItemCollection>(
                                    () => new DefaultObjectMappingItemCollection(edmCollection, _itemsOSpace.Value));
                        }
                        break;
                    case DataSpace.SSpace:
                        CheckAndSetItemCollectionVersionInWorkSpace(collection);
                        _itemsSSpace = new Lazy<StoreItemCollection>(() => (StoreItemCollection)collection, isThreadSafe: true);
                        break;
                    case DataSpace.OSpace:
                        _itemsOSpace = new Lazy<ObjectItemCollection>(() => (ObjectItemCollection)collection, isThreadSafe: true);
                        if (_itemsOCSpace == null
                            && _itemsCSpace != null)
                        {
                            _itemsOCSpace =
                                new Lazy<DefaultObjectMappingItemCollection>(
                                    () =>
                                    new DefaultObjectMappingItemCollection(_itemsCSpace.Value, _itemsOSpace.Value));
                        }
                        break;
                    case DataSpace.CSSpace:
                        CheckAndSetItemCollectionVersionInWorkSpace(collection);
                        _itemsCSSpace = new Lazy<StorageMappingItemCollection>(
                            () => (StorageMappingItemCollection)collection, isThreadSafe: true);
                        break;
                    default:
                        Debug.Assert(collection.DataSpace == DataSpace.OCSpace, "Invalid DataSpace Enum value: " + collection.DataSpace);
                        _itemsOCSpace = new Lazy<DefaultObjectMappingItemCollection>(
                            () => (DefaultObjectMappingItemCollection)collection, isThreadSafe: true);
                        break;
                }
            }
            catch (InvalidCastException)
            {
                throw new MetadataException(Strings.InvalidCollectionForMapping(collection.DataSpace.ToString()));
            }
        }

        private T LoadAndCheckItemCollection<T>(Func<T> itemCollectionLoader) where T : ItemCollection
        {
            DebugCheck.NotNull(itemCollectionLoader);
            var itemCollection = itemCollectionLoader();
            if (itemCollection != null)
            {
                CheckAndSetItemCollectionVersionInWorkSpace(itemCollection);
            }
            return itemCollection;
        }

        private void CheckAndSetItemCollectionVersionInWorkSpace(ItemCollection itemCollectionToRegister)
        {
            DebugCheck.NotNull(itemCollectionToRegister);
            var versionToRegister = XmlConstants.UndefinedVersion;
            string itemCollectionType = null;
            switch (itemCollectionToRegister.DataSpace)
            {
                case DataSpace.CSpace:
                    versionToRegister = ((EdmItemCollection)itemCollectionToRegister).EdmVersion;
                    itemCollectionType = "EdmItemCollection";
                    break;
                case DataSpace.SSpace:
                    versionToRegister = ((StoreItemCollection)itemCollectionToRegister).StoreSchemaVersion;
                    itemCollectionType = "StoreItemCollection";
                    break;
                case DataSpace.CSSpace:
                    versionToRegister = ((StorageMappingItemCollection)itemCollectionToRegister).MappingVersion;
                    itemCollectionType = "StorageMappingItemCollection";
                    break;
                default:
                    // we don't care about other spaces so keep the _versionToRegister to Undefined
                    break;
            }

            lock (_schemaVersionLock)
            {
                if (versionToRegister != _schemaVersion
                    && versionToRegister != XmlConstants.UndefinedVersion
                    && _schemaVersion != XmlConstants.UndefinedVersion)
                {
                    Debug.Assert(itemCollectionType != null);
                    throw new MetadataException(
                        Strings.DifferentSchemaVersionInCollection(itemCollectionType, versionToRegister, _schemaVersion));
                }
                else
                {
                    _schemaVersion = versionToRegister;
                }
            }
        }

        /// <summary>Loads metadata from the given assembly.</summary>
        /// <param name="assembly">The assembly from which the metadata will be loaded.</param>
        public virtual void LoadFromAssembly(Assembly assembly)
        {
            LoadFromAssembly(assembly, null);
        }

        /// <summary>Loads metadata from the given assembly.</summary>
        /// <param name="assembly">The assembly from which the metadata will be loaded.</param>
        /// <param name="logLoadMessage">The delegate for logging the load messages.</param>
        public virtual void LoadFromAssembly(Assembly assembly, Action<string> logLoadMessage)
        {
            Check.NotNull(assembly, "assembly");
            var collection = (ObjectItemCollection)GetItemCollection(DataSpace.OSpace);
            ExplicitLoadFromAssembly(assembly, collection, logLoadMessage);
        }

        private void ExplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection, Action<string> logLoadMessage)
        {
            ItemCollection itemCollection;
            if (!TryGetItemCollection(DataSpace.CSpace, out itemCollection))
            {
                itemCollection = null;
            }

            collection.ExplicitLoadFromAssembly(assembly, (EdmItemCollection)itemCollection, logLoadMessage);
        }

        private void ImplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection)
        {
            if (!MetadataAssemblyHelper.ShouldFilterAssembly(assembly))
            {
                ExplicitLoadFromAssembly(assembly, collection, null);
            }
        }

        // <summary>
        // Implicit loading means that we are trying to help the user find the right
        // assembly, but they didn't explicitly ask for it. Our Implicit rules require that
        // we filter out assemblies with the Ecma or MicrosoftPublic PublicKeyToken on them
        // Load metadata from the type's assembly into the OSpace ItemCollection.
        // If type comes from known source, has Ecma or Microsoft PublicKeyToken then the type's assembly is not
        // loaded, but the callingAssembly and its referenced assemblies are loaded.
        // </summary>
        // <param name="type"> The type's assembly is loaded into the OSpace ItemCollection </param>
        // <param name="callingAssembly"> The assembly and its referenced assemblies to load when type is insuffiecent </param>
        internal virtual void ImplicitLoadAssemblyForType(Type type, Assembly callingAssembly)
        {
            // this exists separately from LoadFromAssembly so that we can handle generics, like IEnumerable<Product>
            DebugCheck.NotNull(type);
            ItemCollection collection;
            if (TryGetItemCollection(DataSpace.OSpace, out collection))
            {
                // if OSpace is not loaded - don't register
                var objItemCollection = (ObjectItemCollection)collection;
                ItemCollection itemCollection;
                TryGetItemCollection(DataSpace.CSpace, out itemCollection);
                var edmItemCollection = (EdmItemCollection)itemCollection;
                if (!objItemCollection.ImplicitLoadAssemblyForType(type, edmItemCollection)
                    && null != callingAssembly)
                {
                    // only load from callingAssembly if all types were filtered
                    // then loaded referenced assemblies of calling assembly

                    // attempt automatic discovery of user types
                    // interesting code paths are ObjectQuery<object>, ObjectQuery<DbDataRecord>, ObjectQuery<IExtendedDataRecord>
                    // other interesting code paths are ObjectQuery<Nullable<Int32>>, ObjectQuery<IEnumerable<object>>
                    // when assemblies is mscorlib, System.Data or System.Data.Entity

                    // If the schema attribute is presented on the assembly or any referenced assemblies, then it is a V1 scenario that we should
                    // strictly follow the Get all referenced assemblies rules.
                    // If the attribute is not presented on the assembly, then we won't load the referenced asssembly 
                    // for this callingAssembly
                    if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(callingAssembly)
                        || (_foundAssemblyWithAttribute
                            || MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(callingAssembly).Any(
                                ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent)))
                    {
                        // cache the knowledge that we found an attribute
                        // because it can be expesive to figure out
                        _foundAssemblyWithAttribute = true;
                        objItemCollection.ImplicitLoadAllReferencedAssemblies(callingAssembly, edmItemCollection);
                    }
                    else
                    {
                        ImplicitLoadFromAssembly(callingAssembly, objItemCollection);
                    }
                }
            }
        }

        // <summary>
        // If OSpace is not loaded for the specified EntityType
        // the load metadata from the callingAssembly and its referenced assemblies.
        // </summary>
        // <param name="type"> The CSPace type to verify its OSpace counterpart is loaded </param>
        // <param name="callingAssembly"> The assembly and its referenced assemblies to load when type is insuffiecent </param>
        internal virtual void ImplicitLoadFromEntityType(EntityType type, Assembly callingAssembly)
        {
            // used by ObjectContext.*GetObjectByKey when the clr type is not available
            // so we check the OCMap to find the clr type else attempt to autoload the OSpace from callingAssembly
            DebugCheck.NotNull(type);
            MappingBase map;
            if (!TryGetMap(type, DataSpace.OCSpace, out map))
            {
                // an OCMap is not exist, attempt to load OSpace to retry
                ImplicitLoadAssemblyForType(typeof(IEntityWithKey), callingAssembly);

                // We do a check here to see if the type was actually found in the attempted load.
                var ospaceCollection = GetItemCollection(DataSpace.OSpace) as ObjectItemCollection;
                EdmType ospaceType;
                if (ospaceCollection == null
                    || !ospaceCollection.TryGetOSpaceType(type, out ospaceType))
                {
                    throw new InvalidOperationException(Strings.Mapping_Object_InvalidType(type.Identity));
                }
            }
        }

        /// <summary>Returns an item by using the specified identity and the data model.</summary>
        /// <returns>The item that matches the given identity in the specified data model.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <param name="dataSpace">The conceptual model in which the item is searched.</param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual T GetItem<T>(string identity, DataSpace dataSpace) where T : GlobalItem
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetItem<T>(identity, ignoreCase: false);
        }

        /// <summary>Returns an item by using the specified identity and the data model.</summary>
        /// <returns>true if there is an item that matches the search criteria; otherwise, false.</returns>
        /// <param name="identity">The conceptual model on which the item is searched.</param>
        /// <param name="space">The conceptual model on which the item is searched.</param>
        /// <param name="item">
        /// When this method returns, contains a <see cref="T:System.Data.Metadata.Edm.GlobalIem" /> object. This parameter is passed uninitialized.
        /// </param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public virtual bool TryGetItem<T>(string identity, DataSpace space, out T item) where T : GlobalItem
        {
            item = null;
            var collection = GetItemCollection(space, required: false);
            return (null != collection) && collection.TryGetItem(identity, false /*ignoreCase*/, out item);
        }

        /// <summary>Returns an item by using the specified identity and the data model.</summary>
        /// <returns>The item that matches the given identity in the specified data model.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the item is searched.</param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual T GetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace) where T : GlobalItem
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetItem<T>(identity, ignoreCase);
        }

        /// <summary>Returns an item by using the specified identity and the data model.</summary>
        /// <returns>true if there is an item that matches the search criteria; otherwise, false.</returns>
        /// <param name="identity">The conceptual model on which the item is searched.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the item is searched.</param>
        /// <param name="item">
        /// When this method returns, contains a <see cref="T:System.Data.Metadata.Edm.GlobalIem" /> object. This parameter is passed uninitialized.
        /// </param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public virtual bool TryGetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace, out T item) where T : GlobalItem
        {
            item = null;
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && collection.TryGetItem(identity, ignoreCase, out item);
        }

        /// <summary>Gets all the items in the specified data model.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the items in the specified data model.
        /// </returns>
        /// <param name="dataSpace">The conceptual model for which the list of items is needed.</param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual ReadOnlyCollection<T> GetItems<T>(DataSpace dataSpace) where T : GlobalItem
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetItems<T>();
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name, namespace name, and data model.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object that represents the type that matches the given type name and the namespace name in the specified data model. If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="dataSpace">The conceptual model on which the type is searched.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "GetType")]
        public virtual EdmType GetType(string name, string namespaceName, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetType(name, namespaceName, ignoreCase: false);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name, namespace name, and data model.
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="dataSpace">The conceptual model on which the type is searched.</param>
        /// <param name="type">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object. This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetType(string name, string namespaceName, DataSpace dataSpace, out EdmType type)
        {
            type = null;
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && collection.TryGetType(name, namespaceName, false /*ignoreCase*/, out type);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name, namespace name, and data model.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object.
        /// </returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the type is searched.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "GetType")]
        public virtual EdmType GetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetType(name, namespaceName, ignoreCase);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name, namespace name, and data model.
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the type is searched.</param>
        /// <param name="type">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object. This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace, out EdmType type)
        {
            type = null;
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && collection.TryGetType(name, namespaceName, ignoreCase, out type);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name and the data model.
        /// </summary>
        /// <returns>If there is no entity container, this method returns null; otherwise, it returns the first entity container.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="dataSpace">The conceptual model on which the entity container is searched.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual EntityContainer GetEntityContainer(string name, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetEntityContainer(name);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name and the data model.
        /// </summary>
        /// <returns>true if there is an entity container that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="dataSpace">The conceptual model on which the entity container is searched.</param>
        /// <param name="entityContainer">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object. If there is no entity container, this output parameter contains null; otherwise, it returns the first entity container. This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetEntityContainer(string name, DataSpace dataSpace, out EntityContainer entityContainer)
        {
            entityContainer = null;
            // null check exists in call stack, but throws for "identity" not "name"
            Check.NotNull(name, "name");
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && collection.TryGetEntityContainer(name, out entityContainer);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name and the data model.
        /// </summary>
        /// <returns>If there is no entity container, this method returns null; otherwise, it returns the first entity container.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the entity container is searched.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual EntityContainer GetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetEntityContainer(name, ignoreCase);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name and the data model.
        /// </summary>
        /// <returns>true if there is an entity container that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="dataSpace">The conceptual model on which the entity container is searched.</param>
        /// <param name="entityContainer">
        /// When this method returns, contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object. If there is no entity container, this output parameter contains null; otherwise, it returns the first entity container. This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace, out EntityContainer entityContainer)
        {
            entityContainer = null;
            // null check exists in call stack, but throws for "identity" not "name"
            Check.NotNull(name, "name");
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && collection.TryGetEntityContainer(name, ignoreCase, out entityContainer);
        }

        /// <summary>Returns all the overloads of the functions by using the specified name, namespace name, and data model.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the functions that match the specified name in a given namespace and a data model.
        /// </returns>
        /// <param name="name">The name of the function.</param>
        /// <param name="namespaceName">The namespace of the function.</param>
        /// <param name="dataSpace">The conceptual model in which the functions are searched.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace)
        {
            return GetFunctions(name, namespaceName, dataSpace, false /*ignoreCase*/);
        }

        /// <summary>Returns all the overloads of the functions by using the specified name, namespace name, and data model.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the functions that match the specified name in a given namespace and a data model.
        /// </returns>
        /// <param name="name">The name of the function.</param>
        /// <param name="namespaceName">The namespace of the function.</param>
        /// <param name="dataSpace">The conceptual model in which the functions are searched.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace, bool ignoreCase)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");
            var collection = GetItemCollection(dataSpace, required: true);

            // Get the function with this full name, which is namespace name plus name
            return collection.GetFunctions(namespaceName + "." + name, ignoreCase);
        }

        // <summary>
        // Gets the function as specified by the function key.
        // All parameters are assumed to be <see cref="ParameterMode.In" />.
        // </summary>
        // <param name="name"> name of the function </param>
        // <param name="namespaceName"> namespace of the function </param>
        // <param name="parameterTypes"> types of the parameters </param>
        // <param name="ignoreCase"> true for case-insensitive lookup </param>
        // <param name="function"> The function that needs to be returned </param>
        // <returns> The function as specified in the function key or null </returns>
        // <exception cref="System.ArgumentNullException">if name, namespaceName, parameterTypes or space argument is null</exception>
        internal virtual bool TryGetFunction(
            string name,
            string namespaceName,
            TypeUsage[] parameterTypes,
            bool ignoreCase,
            DataSpace dataSpace,
            out EdmFunction function)
        {
            function = null;
            Check.NotNull(name, "name");
            Check.NotNull(namespaceName, "namespaceName");
            var collection = GetItemCollection(dataSpace, required: false);

            // Get the function with this full name, which is namespace name plus name
            return (null != collection) && collection.TryGetFunction(namespaceName + "." + name, parameterTypes, ignoreCase, out function);
        }

        /// <summary>Returns the list of primitive types in the specified data model.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the primitive types in the specified data model.
        /// </returns>
        /// <param name="dataSpace">The data model for which you need the list of primitive types.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetItems<PrimitiveType>();
        }

        /// <summary>Gets all the items in the specified data model.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the items in the specified data model.
        /// </returns>
        /// <param name="dataSpace">The conceptual model for which the list of items is needed.</param>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public virtual ReadOnlyCollection<GlobalItem> GetItems(DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetItems<GlobalItem>();
        }

        // <summary>
        // Given the canonical primitive type, get the mapping primitive type in the given dataspace
        // </summary>
        // <param name="primitiveTypeKind"> primitive type kind </param>
        // <param name="dataSpace"> dataspace in which one needs to the mapping primitive types </param>
        // <returns> The mapped scalar type </returns>
        // <exception cref="System.ArgumentNullException">if space argument is null</exception>
        // <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        // <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        internal virtual PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return collection.GetMappedPrimitiveType(primitiveTypeKind);
        }

        // <summary>
        // Search for a Mapping metadata with the specified type key.
        // </summary>
        // <param name="typeIdentity"> type </param>
        // <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        // <param name="ignoreCase"> true for case-insensitive lookup </param>
        // <param name="mappingSpace"> space for which you want to get the mapped type </param>
        // <returns> Returns false if no match found. </returns>
        internal virtual bool TryGetMap(string typeIdentity, DataSpace typeSpace, bool ignoreCase, DataSpace mappingSpace, out MappingBase map)
        {
            map = null;
            var collection = GetItemCollection(mappingSpace, required: false);
            return (null != collection) && ((MappingItemCollection)collection).TryGetMap(typeIdentity, typeSpace, ignoreCase, out map);
        }

        // <summary>
        // Search for a Mapping metadata with the specified type key.
        // </summary>
        // <param name="identity"> typeIdentity of the type </param>
        // <param name="typeSpace"> The dataspace that the type for which map needs to be returned belongs to </param>
        // <param name="dataSpace"> space for which you want to get the mapped type </param>
        // <exception cref="ArgumentException">Thrown if mapping space is not valid</exception>
        internal virtual MappingBase GetMap(string identity, DataSpace typeSpace, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return ((MappingItemCollection)collection).GetMap(identity, typeSpace);
        }

        // <summary>
        // Search for a Mapping metadata with the specified type key.
        // </summary>
        // <param name="dataSpace"> space for which you want to get the mapped type </param>
        // <exception cref="ArgumentException">Thrown if mapping space is not valid</exception>
        internal virtual MappingBase GetMap(GlobalItem item, DataSpace dataSpace)
        {
            var collection = GetItemCollection(dataSpace, required: true);
            return ((MappingItemCollection)collection).GetMap(item);
        }

        // <summary>
        // Search for a Mapping metadata with the specified type key.
        // </summary>
        // <param name="dataSpace"> space for which you want to get the mapped type </param>
        // <returns> Returns false if no match found. </returns>
        internal virtual bool TryGetMap(GlobalItem item, DataSpace dataSpace, out MappingBase map)
        {
            map = null;
            var collection = GetItemCollection(dataSpace, required: false);
            return (null != collection) && ((MappingItemCollection)collection).TryGetMap(item, out map);
        }

        /// <summary>
        /// Tests the retrieval of <see cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" />.
        /// </summary>
        /// <returns>true if the retrieval was successful; otherwise, false.</returns>
        /// <param name="dataSpace">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.DataSpace" /> from which to attempt retrieval of
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.MetadataWorkspace" />
        /// .
        /// </param>
        /// <param name="collection">When this method returns, contains the item collection. This parameter is passed uninitialized.</param>
        public virtual bool TryGetItemCollection(DataSpace dataSpace, out ItemCollection collection)
        {
            collection = GetItemCollection(dataSpace, required: false);
            return (null != collection);
        }

        // <summary>
        // Checks if the space is valid and whether the collection is registered for the given space, and if both are valid,
        // then returns the itemcollection for the given space
        // </summary>
        // <param name="dataSpace"> The dataspace for the item collection that should be returned </param>
        // <param name="required"> if true, will throw if the collection isn't registered </param>
        // <exception cref="ArgumentException">Thrown if required and mapping space is not valid or registered</exception>
        internal virtual ItemCollection GetItemCollection(DataSpace dataSpace, bool required)
        {
            ItemCollection collection;
            switch (dataSpace)
            {
                case DataSpace.CSpace:
                    collection = _itemsCSpace == null ? null : _itemsCSpace.Value;
                    break;
                case DataSpace.OSpace:
                    Debug.Assert(_itemsOSpace != null);
                    collection = _itemsOSpace.Value;
                    break;
                case DataSpace.OCSpace:
                    collection = _itemsOCSpace == null ? null : _itemsOCSpace.Value;
                    break;
                case DataSpace.CSSpace:
                    collection = _itemsCSSpace == null ? null : _itemsCSSpace.Value;
                    break;
                case DataSpace.SSpace:
                    collection = _itemsSSpace == null ? null : _itemsSSpace.Value;
                    break;
                default:
                    if (required)
                    {
                        Debug.Fail("Invalid DataSpace Enum value: " + dataSpace);
                    }
                    collection = null;
                    break;
            }
            if (required && (null == collection))
            {
                throw new InvalidOperationException(Strings.NoCollectionForSpace(dataSpace.ToString()));
            }

            return collection;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the object space type that matches the type supplied by the parameter  edmSpaceType .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the Object space type. If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="edmSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// .
        /// </param>
        public virtual StructuralType GetObjectSpaceType(StructuralType edmSpaceType)
        {
            return GetObjectSpaceType<StructuralType>(edmSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object via the out parameter  objectSpaceType  that represents the type that matches the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// supplied by the parameter  edmSpaceType .
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="edmSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// .
        /// </param>
        /// <param name="objectSpaceType">
        /// When this method returns, contains a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the Object space type. This parameter is passed uninitialized.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public virtual bool TryGetObjectSpaceType(StructuralType edmSpaceType, out StructuralType objectSpaceType)
        {
            return TryGetObjectSpaceType<StructuralType>(edmSpaceType, out objectSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the object space type that matches the type supplied by the parameter  edmSpaceType .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the Object space type. If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="edmSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// .
        /// </param>
        public virtual EnumType GetObjectSpaceType(EnumType edmSpaceType)
        {
            return GetObjectSpaceType<EnumType>(edmSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object via the out parameter  objectSpaceType  that represents the type that matches the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// supplied by the parameter  edmSpaceType .
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="edmSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// .
        /// </param>
        /// <param name="objectSpaceType">
        /// When this method returns, contains a <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object that represents the Object space type. This parameter is passed uninitialized.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public virtual bool TryGetObjectSpaceType(EnumType edmSpaceType, out EnumType objectSpaceType)
        {
            return TryGetObjectSpaceType<EnumType>(edmSpaceType, out objectSpaceType);
        }

        // <summary>
        // Helper method returning the OSpace enum type mapped to the specified Edm Space Type.
        // If the DataSpace of the argument is not CSpace, or the mapped OSpace type
        // cannot be determined, an ArgumentException is thrown.
        // </summary>
        // <param name="edmSpaceType"> The CSpace type to look up </param>
        // <returns> The OSpace type mapped to the supplied argument </returns>
        // <typeparam name="T"> Must be StructuralType or EnumType. </typeparam>
        private T GetObjectSpaceType<T>(T edmSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                edmSpaceType == null || edmSpaceType is StructuralType || edmSpaceType is EnumType,
                "Only structural or enum type expected");

            T objectSpaceType;
            if (!TryGetObjectSpaceType(edmSpaceType, out objectSpaceType))
            {
                throw new ArgumentException(Strings.FailedToFindOSpaceTypeMapping(edmSpaceType.Identity));
            }

            return objectSpaceType;
        }

        // <summary>
        // Helper method returning the OSpace structural or enum type mapped to the specified Edm Space Type.
        // If the DataSpace of the argument is not CSpace, or if the mapped OSpace type
        // cannot be determined, the method returns false and sets the out parameter
        // to null.
        // </summary>
        // <param name="edmSpaceType"> The CSpace type to look up </param>
        // <param name="objectSpaceType"> The OSpace type mapped to the supplied argument </param>
        // <returns> true on success, false on failure </returns>
        // <typeparam name="T"> Must be StructuralType or EnumType. </typeparam>
        private bool TryGetObjectSpaceType<T>(T edmSpaceType, out T objectSpaceType)
            where T : EdmType
        {
            DebugCheck.NotNull(edmSpaceType);

            Debug.Assert(
                edmSpaceType == null || edmSpaceType is StructuralType || edmSpaceType is EnumType,
                "Only structural or enum type expected");

            if (edmSpaceType.DataSpace != DataSpace.CSpace)
            {
                throw new ArgumentException(Strings.ArgumentMustBeCSpaceType, "edmSpaceType");
            }

            objectSpaceType = null;

            MappingBase map;
            if (TryGetMap(edmSpaceType, DataSpace.OCSpace, out map))
            {
                var ocMap = map as ObjectTypeMapping;
                if (ocMap != null)
                {
                    objectSpaceType = (T)ocMap.ClrType;
                }
            }

            return objectSpaceType != null;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// that matches the type supplied by the parameter  objectSpaceType .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// . If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="objectSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> that supplies the type in the object space.
        /// </param>
        public virtual StructuralType GetEdmSpaceType(StructuralType objectSpaceType)
        {
            return GetEdmSpaceType<StructuralType>(objectSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object via the out parameter  edmSpaceType  that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// that matches the type supplied by the parameter  objectSpaceType .
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="objectSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the object space type.
        /// </param>
        /// <param name="edmSpaceType">
        /// When this method returns, contains a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// . This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetEdmSpaceType(StructuralType objectSpaceType, out StructuralType edmSpaceType)
        {
            return TryGetEdmSpaceType<StructuralType>(objectSpaceType, out edmSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// that matches the type supplied by the parameter  objectSpaceType .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// . If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="objectSpaceType">
        /// A <see cref="T:System.Data.Metadata.Edm.EnumlType" /> that supplies the type in the object space.
        /// </param>
        public virtual EnumType GetEdmSpaceType(EnumType objectSpaceType)
        {
            return GetEdmSpaceType<EnumType>(objectSpaceType);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object via the out parameter  edmSpaceType  that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// that matches the type supplied by the parameter  objectSpaceType .
        /// </summary>
        /// <returns>true on success, false on failure.</returns>
        /// <param name="objectSpaceType">
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object that represents the object space type.
        /// </param>
        /// <param name="edmSpaceType">
        /// When this method returns, contains a <see cref="T:System.Data.Entity.Core.Metadata.Edm.EnumType" /> object that represents the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// . This parameter is passed uninitialized.
        /// </param>
        public virtual bool TryGetEdmSpaceType(EnumType objectSpaceType, out EnumType edmSpaceType)
        {
            return TryGetEdmSpaceType<EnumType>(objectSpaceType, out edmSpaceType);
        }

        // <summary>
        // Helper method returning the Edm Space structural or enum type mapped to the OSpace Type parameter. If the
        // DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        // be determined, an ArgumentException is thrown.
        // </summary>
        // <param name="objectSpaceType"> The OSpace type to look up </param>
        // <returns> The CSpace type mapped to the OSpace parameter </returns>
        // <typeparam name="T"> Must be StructuralType or EnumType </typeparam>
        private T GetEdmSpaceType<T>(T objectSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only structural or enum type expected");

            T edmSpaceType;
            if (!TryGetEdmSpaceType(objectSpaceType, out edmSpaceType))
            {
                throw new ArgumentException(Strings.FailedToFindCSpaceTypeMapping(objectSpaceType.Identity));
            }

            return edmSpaceType;
        }

        // <summary>
        // Helper method returning the Edm Space structural or enum type mapped to the OSpace Type parameter. If the
        // DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        // be determined, the method returns false and sets the out parameter to null.
        // </summary>
        // <param name="objectSpaceType"> The OSpace type to look up </param>
        // <param name="edmSpaceType"> The mapped CSpace type </param>
        // <returns> true on success, false on failure </returns>
        // <typeparam name="T"> Must be StructuralType or EnumType </typeparam>
        private bool TryGetEdmSpaceType<T>(T objectSpaceType, out T edmSpaceType)
            where T : EdmType
        {
            DebugCheck.NotNull(objectSpaceType);

            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only structural or enum type expected");

            if (objectSpaceType.DataSpace != DataSpace.OSpace)
            {
                throw new ArgumentException(Strings.ArgumentMustBeOSpaceType, "objectSpaceType");
            }

            edmSpaceType = null;

            MappingBase map;
            if (TryGetMap(objectSpaceType, DataSpace.OCSpace, out map))
            {
                var ocMap = map as ObjectTypeMapping;
                if (ocMap != null)
                {
                    edmSpaceType = (T)ocMap.EdmType;
                }
            }

            return edmSpaceType != null;
        }

        ///// <summary>
        ///// Returns the update or query view for an Extent as a
        ///// command tree. For a given Extent, MetadataWorkspace will
        ///// have either a Query view or an Update view but not both.
        ///// </summary>
        ///// <param name="extent"></param>
        ///// <returns></returns>
        internal virtual DbQueryCommandTree GetCqtView(EntitySetBase extent)
        {
            return GetGeneratedView(extent).GetCommandTree();
        }

        // <summary>
        // Returns generated update or query view for the given extent.
        // </summary>
        internal virtual GeneratedView GetGeneratedView(EntitySetBase extent)
        {
            var collection = GetItemCollection(DataSpace.CSSpace, required: true);
            return ((StorageMappingItemCollection)collection).GetGeneratedView(extent, this);
        }

        // <summary>
        // Returns a TypeOf/TypeOfOnly Query for a given Extent and Type as a command tree.
        // </summary>
        internal virtual bool TryGetGeneratedViewOfType(
            EntitySetBase extent, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
        {
            var collection = GetItemCollection(DataSpace.CSSpace, required: true);
            return ((StorageMappingItemCollection)collection).TryGetGeneratedViewOfType(extent, type, includeSubtypes, out generatedView);
        }

        // <summary>
        // Returns generated function definition for the given function.
        // Guarantees type match of declaration and generated parameters.
        // Guarantees return type match.
        // Throws internal error for functions without definition.
        // Passes thru exception occured during definition generation.
        // </summary>
        internal virtual DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
        {
            var collection = GetItemCollection(DataSpace.CSpace, required: true);
            return ((EdmItemCollection)collection).GetGeneratedFunctionDefinition(function);
        }

        // <summary>
        // Determines if a target function exists for the given function import.
        // </summary>
        // <param name="functionImport"> Function import (function declared in a model entity container) </param>
        // <param name="targetFunctionMapping"> Function target mapping (function to which the import is mapped in the target store) </param>
        // <returns> true if a mapped target function exists; false otherwise </returns>
        internal virtual bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping targetFunctionMapping)
        {
            DebugCheck.NotNull(functionImport);
            var entityContainerMaps = GetItems<EntityContainerMapping>(DataSpace.CSSpace);
            foreach (var containerMapping in entityContainerMaps)
            {
                if (containerMapping.TryGetFunctionImportMapping(functionImport, out targetFunctionMapping))
                {
                    return true;
                }
            }
            targetFunctionMapping = null;
            return false;
        }

        // <summary>
        // Returns the view loader associated with this workspace,
        // creating a loader if non exists. The loader includes
        // context information used by the update pipeline when
        // processing changes to C-space extents.
        // </summary>
        internal virtual ViewLoader GetUpdateViewLoader()
        {
            return (_itemsCSSpace != null && _itemsCSSpace.Value != null) ? _itemsCSSpace.Value.GetUpdateViewLoader() : null;
        }

        // <summary>
        // Takes in a Edm space type usage and converts into an
        // equivalent O space type usage
        // </summary>
        internal virtual TypeUsage GetOSpaceTypeUsage(TypeUsage edmSpaceTypeUsage)
        {
            DebugCheck.NotNull(edmSpaceTypeUsage);
            DebugCheck.NotNull(edmSpaceTypeUsage.EdmType);

            EdmType clrType = null;
            if (Helper.IsPrimitiveType(edmSpaceTypeUsage.EdmType))
            {
                var collection = GetItemCollection(DataSpace.OSpace, required: true);
                clrType = collection.GetMappedPrimitiveType(((PrimitiveType)edmSpaceTypeUsage.EdmType).PrimitiveTypeKind);
            }
            else
            {
                // Check and throw if the OC space doesn't exist
                var collection = GetItemCollection(DataSpace.OCSpace, required: true);

                // Get the OC map
                var map = ((DefaultObjectMappingItemCollection)collection).GetMap(edmSpaceTypeUsage.EdmType);
                clrType = ((ObjectTypeMapping)map).ClrType;
            }

            Debug.Assert(
                !Helper.IsPrimitiveType(clrType) ||
                ReferenceEquals(
                    ClrProviderManifest.Instance.GetFacetDescriptions(clrType),
                    EdmProviderManifest.Instance.GetFacetDescriptions(clrType.BaseType)),
                "these are no longer equal so we can't just use the same set of facets for the new type usage");

            // Transfer the facet values
            var result = TypeUsage.Create(clrType, edmSpaceTypeUsage.Facets);

            return result;
        }

        // <summary>
        // Returns true if the item collection for the given space has already been registered else returns false
        // </summary>
        internal virtual bool IsItemCollectionAlreadyRegistered(DataSpace dataSpace)
        {
            ItemCollection itemCollection;
            return TryGetItemCollection(dataSpace, out itemCollection);
        }

        // <summary>
        // Requires: C, S and CS are registered in this and other
        // Determines whether C, S and CS are equivalent. Useful in determining whether a DbCommandTree
        // is usable within a particular entity connection.
        // </summary>
        // <param name="other"> Other workspace. </param>
        // <returns> true is C, S and CS collections are equivalent </returns>
        internal virtual bool IsMetadataWorkspaceCSCompatible(MetadataWorkspace other)
        {
            Debug.Assert(
                IsItemCollectionAlreadyRegistered(DataSpace.CSSpace) &&
                other.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace),
                "requires: C, S and CS are registered in this and other");

            var result =
                GetItemCollection(DataSpace.CSSpace, required: false)
                    .MetadataEquals(other.GetItemCollection(DataSpace.CSSpace, required: false));

            Debug.Assert(
                !result ||
                (GetItemCollection(DataSpace.CSpace, required: false)
                     .MetadataEquals(other.GetItemCollection(DataSpace.CSpace, required: false))
                 && GetItemCollection(DataSpace.SSpace, required: false)
                        .MetadataEquals(other.GetItemCollection(DataSpace.SSpace, required: false))),
                "constraint: this.CS == other.CS --> this.S == other.S && this.C == other.C");

            return result;
        }

        /// <summary>Clears all the metadata cache entries.</summary>
        public static void ClearCache()
        {
            MetadataCache.Instance.Clear();
            using (var cache = AssemblyCache.AquireLockedAssemblyCache())
            {
                cache.Clear();
            }
        }

        // <summary>
        // Returns the canonical Model TypeUsage for a given PrimitiveTypeKind
        // </summary>
        // <param name="primitiveTypeKind"> PrimitiveTypeKind for which a canonical TypeUsage is expected </param>
        // <returns> a canonical model TypeUsage </returns>
        internal static TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
        {
            return EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveTypeKind);
        }

        // <summary>
        // Returns the Model PrimitiveType for a given primitiveTypeKind
        // </summary>
        // <param name="primitiveTypeKind"> a PrimitiveTypeKind for which a Model PrimitiveType is expected </param>
        // <returns> Model PrimitiveType </returns>
        internal static PrimitiveType GetModelPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            return EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
        }

        // GetRequiredOriginalValueMembers and GetRelevantMembersForUpdate return list of "interesting" members for the given EntitySet/EntityType
        // Interesting Members are a subset of the following:
        //    0. Key members
        //    1. Members with C-Side conditions (complex types can not have C-side condition at present)
        //    2. Members participating in association end
        //    3. Members with ConcurrencyMode 'Fixed'
        //      3.1 Complex Members with any child member having Concurrency mode Fixed
        //    4. Members included in Update ModificationFunction with Version='Original' (Original = Not Current)
        //      4.1 Complex Members in ModificationFunction if any sub-member is interesting
        //    5. Members included in Update ModificationFunction (mutually exclusive with 4 - required for partial update scenarios)
        //    6. Foreign keys
        //    7. All complex members - partial update scenarios only
        /// <summary>Gets original value members from an entity set and entity type.</summary>
        /// <returns>The original value members from an entity set and entity type.</returns>
        /// <param name="entitySet">The entity set from which to retrieve original values.</param>
        /// <param name="entityType">The entity type of which to retrieve original values.</param>
        [Obsolete("Use MetadataWorkspace.GetRelevantMembersForUpdate(EntitySetBase, EntityTypeBase, bool) instead")]
        public virtual IEnumerable<EdmMember> GetRequiredOriginalValueMembers(EntitySetBase entitySet, EntityTypeBase entityType)
        {
            return GetInterestingMembers(
                entitySet, entityType, StorageMappingItemCollection.InterestingMembersKind.RequiredOriginalValueMembers);
        }

        /// <summary>
        /// Returns members of a given <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />/
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" />
        /// for which original values are needed when modifying an entity.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmMember" />s for which original value is required.
        /// </returns>
        /// <param name="entitySet">
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> belonging to the C-Space.
        /// </param>
        /// <param name="entityType">
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" /> that participates in the given
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" />
        /// .
        /// </param>
        /// <param name="partialUpdateSupported">true if entities may be updated partially; otherwise, false.</param>
        public virtual ReadOnlyCollection<EdmMember> GetRelevantMembersForUpdate(
            EntitySetBase entitySet, EntityTypeBase entityType, bool partialUpdateSupported)
        {
            return GetInterestingMembers(
                entitySet,
                entityType,
                partialUpdateSupported
                    ? StorageMappingItemCollection.InterestingMembersKind.PartialUpdate
                    : StorageMappingItemCollection.InterestingMembersKind.FullUpdate);
        }

        // <summary>
        // Return members for <see cref="GetRequiredOriginalValueMembers" /> and <see cref="GetRelevantMembersForUpdate" /> methods.
        // </summary>
        // <param name="entitySet"> An EntitySet belonging to the C-Space </param>
        // <param name="entityType"> An EntityType that participates in the given EntitySet </param>
        // <param name="interestingMembersKind"> Scenario the members should be returned for. </param>
        // <returns>
        // ReadOnlyCollection of interesting members for the requested scenario (
        // <paramref
        //     name="interestingMembersKind" />
        // ).
        // </returns>
        private ReadOnlyCollection<EdmMember> GetInterestingMembers(
            EntitySetBase entitySet, EntityTypeBase entityType, StorageMappingItemCollection.InterestingMembersKind interestingMembersKind)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            Debug.Assert(entitySet.EntityContainer != null);

            var associationSet = entitySet as AssociationSet;

            //Check that EntitySet is from CSpace
            if (entitySet.EntityContainer.DataSpace != DataSpace.CSpace)
            {
                if (associationSet != null)
                {
                    throw new ArgumentException(Strings.EntitySetNotInCSPace(entitySet.Name));
                }
                else
                {
                    throw new ArgumentException(Strings.EntitySetNotInCSPace(entitySet.Name));
                }
            }

            //Check that entityType belongs to entitySet
            if (!entitySet.ElementType.IsAssignableFrom(entityType))
            {
                if (associationSet != null)
                {
                    throw new ArgumentException(
                        Strings.TypeNotInAssociationSet(entityType.FullName, entitySet.ElementType.FullName, entitySet.Name));
                }
                else
                {
                    throw new ArgumentException(
                        Strings.TypeNotInEntitySet(entityType.FullName, entitySet.ElementType.FullName, entitySet.Name));
                }
            }

            var mappingCollection = (StorageMappingItemCollection)GetItemCollection(DataSpace.CSSpace, required: true);
            return mappingCollection.GetInterestingMembers(entitySet, entityType, interestingMembersKind);
        }

        // <summary>
        // Returns the QueryCacheManager hosted by this metadata workspace instance
        // </summary>
        internal virtual QueryCacheManager GetQueryCacheManager()
        {
            Debug.Assert(_itemsSSpace != null && _itemsSSpace.Value != null);
            return _itemsSSpace.Value.QueryCacheManager;
        }

        internal bool TryDetermineCSpaceModelType<T>(out EdmType modelEdmType)
        {
            return TryDetermineCSpaceModelType(typeof(T), out modelEdmType);
        }

        internal virtual bool TryDetermineCSpaceModelType(Type type, out EdmType modelEdmType)
        {
            var nonNullableType = TypeSystem.GetNonNullableType(type);

            // make sure the workspace knows about T
            ImplicitLoadAssemblyForType(nonNullableType, Assembly.GetCallingAssembly());
            var objectItemCollection = (ObjectItemCollection)GetItemCollection(DataSpace.OSpace);
            EdmType objectEdmType;
            if (objectItemCollection.TryGetItem(nonNullableType.FullNameWithNesting(), out objectEdmType))
            {
                MappingBase map;
                if (TryGetMap(objectEdmType, DataSpace.OCSpace, out map))
                {
                    var objectMapping = (ObjectTypeMapping)map;
                    modelEdmType = objectMapping.EdmType;
                    return true;
                }
            }

            modelEdmType = null;

            return false;
        }
    }
}
