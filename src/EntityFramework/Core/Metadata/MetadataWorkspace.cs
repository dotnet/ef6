namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Versioning;
    using eSQL = System.Data.Entity.Core.Common.EntitySql;

    /// <summary>
    /// Runtime Metadata Workspace
    /// </summary>
    public sealed class MetadataWorkspace
    {
        private InternalMetadataWorkspace _internalMetadataWorkspace;

        /// <summary>
        /// Constructs the new instance of runtime metadata workspace
        /// </summary>
        public MetadataWorkspace()
            : this(new InternalMetadataWorkspace())
        {
        }

        /// <summary>
        /// Create MetadataWorkspace that is populated with ItemCollections for all the spaces that the metadata artifacts provided.
        /// All res:// paths will be resolved only from the assemblies returned from the enumerable assembliesToConsider.
        /// </summary>
        /// <param name="paths">The paths where the metadata artifacts located</param>
        /// <param name="assembliesToConsider">User specified assemblies to consider</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Throw when assembliesToConsider is empty or contains null, or cannot find the corresponding assembly in it</exception>
        /// <exception cref="Core.MetadataException"></exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        public MetadataWorkspace(IEnumerable<string> paths, IEnumerable<Assembly> assembliesToConsider)
            : this(new InternalMetadataWorkspace(paths, assembliesToConsider))
        {
        }

        internal MetadataWorkspace(InternalMetadataWorkspace internalMetadataWorkspace)
        {
            _internalMetadataWorkspace = internalMetadataWorkspace;
            _internalMetadataWorkspace.MetadataWorkspaceWrapper = this;
        }

        internal Guid MetadataWorkspaceId
        {
            get { return _internalMetadataWorkspace.MetadataWorkspaceId; }
        }

        #region Methods

        /// <summary>
        /// Create an <see cref="eSQL.EntitySqlParser"/> configured to use the <see cref="DataSpace.CSpace"/> data space.
        /// </summary>
        public eSQL.EntitySqlParser CreateEntitySqlParser()
        {
            return _internalMetadataWorkspace.CreateEntitySqlParser();
        }

        /// <summary>
        /// Creates a new <see cref="DbQueryCommandTree"/> bound to this metadata workspace based on the specified query expression.
        /// </summary>
        /// <param name="query">A <see cref="DbExpression"/> that defines the query</param>
        /// <returns>A new <see cref="DbQueryCommandTree"/> with the specified expression as it's <see cref="DbQueryCommandTree.Query"/> property</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="query"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="query"/> contains metadata that cannot be resolved in this metadata workspace</exception>
        /// <exception cref="ArgumentException">If <paramref name="query"/> is not structurally valid because it contains unresolvable variable references</exception>
        public DbQueryCommandTree CreateQueryCommandTree(DbExpression query)
        {
            return _internalMetadataWorkspace.CreateQueryCommandTree(query);
        }

        /// <summary>
        /// Get item collection for the space. The ItemCollection is in read only mode as it is
        /// part of the workspace.
        /// </summary>
        /// <param name="dataSpace">The dataspace for the item colelction that should be returned</param>
        /// <returns>The item collection for the given space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        [CLSCompliant(false)]
        public ItemCollection GetItemCollection(DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetItemCollection(dataSpace);
        }

        /// <summary>
        /// Register the item collection for the space associated with it.
        /// This should be done only once for a space.
        /// If a space already has a registered ItemCollection InvalidOperation exception is thrown
        /// </summary>
        /// <param name="collection">The out parameter collection that needs to be filled up</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if collection argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If there is an ItemCollection that has already been registered for collection's space passed in</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [CLSCompliant(false)]
        public void RegisterItemCollection(ItemCollection collection)
        {
            _internalMetadataWorkspace.RegisterItemCollection(collection);
        }

        /// <summary>
        /// Add a token for this MetadataWorkspace just so this metadata workspace holds a reference to it, this
        /// is for metadata caching to make the workspace marking a particular cache entry is still in used
        /// </summary>
        /// <param name="token"></param>
        internal void AddMetadataEntryToken(object token)
        {
            _internalMetadataWorkspace.AddMetadataEntryToken(token);
        }

        /// <summary>
        /// Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly">The assembly from which to load metadata</param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly)
        {
            _internalMetadataWorkspace.LoadFromAssembly(assembly);
        }

        /// <summary>
        /// Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly">The assembly from which to load metadata</param>
        /// <param name="logLoadMessage">The delegate for logging the load messages</param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly, Action<string> logLoadMessage)
        {
            _internalMetadataWorkspace.LoadFromAssembly(assembly, logLoadMessage);
        }

        /// <summary>
        /// Implicit loading means that we are trying to help the user find the right 
        /// assembly, but they didn't explicitly ask for it. Our Implicit rules require that
        /// we filter out assemblies with the Ecma or MicrosoftPublic PublicKeyToken on them
        /// 
        /// Load metadata from the type's assembly into the OSpace ItemCollection.
        /// If type comes from known source, has Ecma or Microsoft PublicKeyToken then the type's assembly is not
        /// loaded, but the callingAssembly and its referenced assemblies are loaded.
        /// </summary>
        /// <param name="type">The type's assembly is loaded into the OSpace ItemCollection</param>
        /// <param name="callingAssembly">The assembly and its referenced assemblies to load when type is insuffiecent</param>
        internal void ImplicitLoadAssemblyForType(Type type, Assembly callingAssembly)
        {
            _internalMetadataWorkspace.ImplicitLoadAssemblyForType(type, callingAssembly);
        }

        /// <summary>
        /// If OSpace is not loaded for the specified EntityType
        /// the load metadata from the callingAssembly and its referenced assemblies.
        /// </summary>
        /// <param name="type">The CSPace type to verify its OSpace counterpart is loaded</param>
        /// <param name="callingAssembly">The assembly and its referenced assemblies to load when type is insuffiecent</param>
        internal void ImplicitLoadFromEntityType(EntityType type, Assembly callingAssembly)
        {
            _internalMetadataWorkspace.ImplicitLoadFromEntityType(type, callingAssembly);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// For example, The identity for EdmType is Namespace.Name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have an item with the given identity</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public T GetItem<T>(string identity, DataSpace dataSpace) where T : GlobalItem
        {
            return _internalMetadataWorkspace.GetItem<T>(identity, dataSpace);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="space"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if identity or space argument is null</exception>
        public bool TryGetItem<T>(string identity, DataSpace space, out T item) where T : GlobalItem
        {
            return _internalMetadataWorkspace.TryGetItem(identity, space, out item);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// For example, The identity for EdmType is Namespace.Name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have an item with the given identity</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public T GetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace) where T : GlobalItem
        {
            return _internalMetadataWorkspace.GetItem<T>(identity, ignoreCase, dataSpace);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ignoreCase"></param>
        /// <param name="identity"></param>
        /// <param name="dataSpace"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if identity or space argument is null</exception>
        public bool TryGetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace, out T item) where T : GlobalItem
        {
            return _internalMetadataWorkspace.TryGetItem(identity, ignoreCase, dataSpace, out item);
        }

        /// <summary>
        /// Returns ReadOnlyCollection of the Items of the given type
        /// in the workspace.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<T> GetItems<T>(DataSpace dataSpace) where T : GlobalItem
        {
            return _internalMetadataWorkspace.GetItems<T>(dataSpace);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName arguments passed in are null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a type with the given name and namespaceName</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EdmType GetType(string name, string namespaceName, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetType(name, namespaceName, dataSpace);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns false if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName or space argument is null</exception>
        public bool TryGetType(string name, string namespaceName, DataSpace dataSpace, out EdmType type)
        {
            return _internalMetadataWorkspace.TryGetType(name, namespaceName, dataSpace, out type);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="ignoreCase"></param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName arguments passed in are null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a type with the given name and namespaceName</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EdmType GetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetType(name, namespaceName, ignoreCase, dataSpace);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <param name="ignoreCase"></param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName or space argument is null</exception>
        public bool TryGetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace, out EdmType type)
        {
            return _internalMetadataWorkspace.TryGetType(name, namespaceName, ignoreCase, dataSpace, out type);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="dataSpace"></param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EntityContainer with the given name</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EntityContainer GetEntityContainer(string name, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetEntityContainer(name, dataSpace);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="dataSpace"></param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if either space or name arguments is null</exception>
        public bool TryGetEntityContainer(string name, DataSpace dataSpace, out EntityContainer entityContainer)
        {
            return _internalMetadataWorkspace.TryGetEntityContainer(name, dataSpace, out entityContainer);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EntityContainer with the given name</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EntityContainer GetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetEntityContainer(name, ignoreCase, dataSpace);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if name or space argument is null</exception>
        public bool TryGetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace, out EntityContainer entityContainer)
        {
            return _internalMetadataWorkspace.TryGetEntityContainer(name, ignoreCase, dataSpace, out entityContainer);
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="dataSpace">The dataspace for which we need to get the function for</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if functionName argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EdmFunction with the given functionName</exception>
        /// <exception cref="System.ArgumentException">If the name or namespaceName is empty</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetFunctions(name, namespaceName, dataSpace);
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="dataSpace">The dataspace for which we need to get the function for</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if functionName argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EdmFunction with the given functionName</exception>
        /// <exception cref="System.ArgumentException">If the name or namespaceName is empty</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace, bool ignoreCase)
        {
            return _internalMetadataWorkspace.GetFunctions(name, namespaceName, dataSpace, ignoreCase);
        }

        /// <summary>
        /// Gets the function as specified by the function key.
        /// All parameters are assumed to be <see cref="ParameterMode.In"/>.
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="parameterTypes">types of the parameters</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <param name="function">The function that needs to be returned</param>
        /// <returns> The function as specified in the function key or null</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName, parameterTypes or space argument is null</exception>
        internal bool TryGetFunction(
            string name,
            string namespaceName,
            TypeUsage[] parameterTypes,
            bool ignoreCase,
            DataSpace dataSpace,
            out EdmFunction function)
        {
            return _internalMetadataWorkspace.TryGetFunction(name, namespaceName, parameterTypes, ignoreCase, dataSpace, out function);
        }

        /// <summary>
        /// Get the list of primitive types for the given space
        /// </summary>
        /// <param name="dataSpace">dataspace for which you need the list of primitive types</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetPrimitiveTypes(dataSpace);
        }

        /// <summary>
        /// Get all the items in the data space
        /// </summary>
        /// <param name="dataSpace">dataspace for which you need the list of items</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<GlobalItem> GetItems(DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetItems(dataSpace);
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind">primitive type kind</param>
        /// <param name="dataSpace">dataspace in which one needs to the mapping primitive types</param>
        /// <returns>The mapped scalar type</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        internal PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetMappedPrimitiveType(primitiveTypeKind, dataSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="typeIdentity">type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="mappingSpace">space for which you want to get the mapped type</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal bool TryGetMap(string typeIdentity, DataSpace typeSpace, bool ignoreCase, DataSpace mappingSpace, out Map map)
        {
            return _internalMetadataWorkspace.TryGetMap(typeIdentity, typeSpace, ignoreCase, mappingSpace, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">typeIdentity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal Map GetMap(string identity, DataSpace typeSpace, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetMap(identity, typeSpace, dataSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal Map GetMap(GlobalItem item, DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.GetMap(item, dataSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal bool TryGetMap(GlobalItem item, DataSpace dataSpace, out Map map)
        {
            return _internalMetadataWorkspace.TryGetMap(item, dataSpace, out map);
        }

        /// <summary>
        /// Get item collection for the space, if registered. If returned, the ItemCollection is in read only mode as it is
        /// part of the workspace.
        /// </summary>
        /// <param name="dataSpace">The dataspace for the item collection that should be returned</param>
        /// <param name="collection">The collection registered for the specified dataspace, if any</param>
        /// <returns><c>true</c> if an item collection is currently registered for the specified space; otherwise <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        [CLSCompliant(false)]
        public bool TryGetItemCollection(DataSpace dataSpace, out ItemCollection collection)
        {
            return _internalMetadataWorkspace.TryGetItemCollection(dataSpace, out collection);
        }

        /// <summary>
        /// Checks if the space is valid and whether the collection is registered for the given space, and if both are valid,
        /// then returns the itemcollection for the given space
        /// </summary>
        /// <param name="dataSpace"></param>
        /// <param name="required">if true, will throw</param>
        /// <exception cref="ArgumentException">Thrown if required and mapping space is not valid or registered</exception>
        internal ItemCollection GetItemCollection(DataSpace dataSpace, bool required)
        {
            return _internalMetadataWorkspace.GetItemCollection(dataSpace, required);
        }

        /// <summary>
        /// The method returns the OSpace structural type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or the mapped OSpace type 
        /// cannot be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <returns>The OSpace type mapped to the supplied argument</returns>
        public StructuralType GetObjectSpaceType(StructuralType edmSpaceType)
        {
            return _internalMetadataWorkspace.GetObjectSpaceType(edmSpaceType);
        }

        /// <summary>
        /// This method returns the OSpace structural type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or if the mapped OSpace type 
        /// cannot be determined, the method returns false and sets the out parameter
        /// to null.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <param name="objectSpaceType">The OSpace type mapped to the supplied argument</param>
        /// <returns>true on success, false on failure</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public bool TryGetObjectSpaceType(StructuralType edmSpaceType, out StructuralType objectSpaceType)
        {
            return _internalMetadataWorkspace.TryGetObjectSpaceType(edmSpaceType, out objectSpaceType);
        }

        /// <summary>
        /// The method returns the OSpace enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or the mapped OSpace type 
        /// cannot be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <returns>The OSpace type mapped to the supplied argument</returns>
        public EnumType GetObjectSpaceType(EnumType edmSpaceType)
        {
            return _internalMetadataWorkspace.GetObjectSpaceType(edmSpaceType);
        }

        /// <summary>
        /// This method returns the OSpace enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or if the mapped OSpace type 
        /// cannot be determined, the method returns false and sets the out parameter
        /// to null.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <param name="objectSpaceType">The OSpace type mapped to the supplied argument</param>
        /// <returns>true on success, false on failure</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public bool TryGetObjectSpaceType(EnumType edmSpaceType, out EnumType objectSpaceType)
        {
            return _internalMetadataWorkspace.TryGetObjectSpaceType(edmSpaceType, out objectSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space structural type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <returns>The CSpace type mapped to the OSpace parameter</returns>
        public StructuralType GetEdmSpaceType(StructuralType objectSpaceType)
        {
            return _internalMetadataWorkspace.GetEdmSpaceType(objectSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space structural type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, the method returns false and sets the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <param name="edmSpaceType">The mapped CSpace type</param>
        /// <returns>true on success, false on failure</returns>
        public bool TryGetEdmSpaceType(StructuralType objectSpaceType, out StructuralType edmSpaceType)
        {
            return _internalMetadataWorkspace.TryGetEdmSpaceType(objectSpaceType, out edmSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <returns>The CSpace type mapped to the OSpace parameter</returns>
        public EnumType GetEdmSpaceType(EnumType objectSpaceType)
        {
            return _internalMetadataWorkspace.GetEdmSpaceType(objectSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, the method returns false and sets the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <param name="edmSpaceType">The mapped CSpace type</param>
        /// <returns>true on success, false on failure</returns>
        public bool TryGetEdmSpaceType(EnumType objectSpaceType, out EnumType edmSpaceType)
        {
            return _internalMetadataWorkspace.TryGetEdmSpaceType(objectSpaceType, out edmSpaceType);
        }

        ///// <summary>
        ///// Returns the update or query view for an Extent as a
        ///// command tree. For a given Extent, MetadataWorkspace will
        ///// have either a Query view or an Update view but not both.
        ///// </summary>
        ///// <param name="extent"></param>
        ///// <returns></returns>
        internal DbQueryCommandTree GetCqtView(EntitySetBase extent)
        {
            return _internalMetadataWorkspace.GetCqtView(extent);
        }

        /// <summary>
        /// Returns generated update or query view for the given extent.
        /// </summary>
        internal GeneratedView GetGeneratedView(EntitySetBase extent)
        {
            return _internalMetadataWorkspace.GetGeneratedView(extent);
        }

        /// <summary>
        /// Returns a TypeOf/TypeOfOnly Query for a given Extent and Type as a command tree.
        /// </summary>
        /// <param name="extent"></param>
        /// <returns></returns>
        internal bool TryGetGeneratedViewOfType(
            EntitySetBase extent, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
        {
            return _internalMetadataWorkspace.TryGetGeneratedViewOfType(extent, type, includeSubtypes, out generatedView);
        }

        /// <summary>
        /// Returns generated function definition for the given function.
        /// Guarantees type match of declaration and generated parameters.
        /// Guarantees return type match.
        /// Throws internal error for functions without definition.
        /// Passes thru exception occured during definition generation.
        /// </summary>
        internal DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
        {
            return _internalMetadataWorkspace.GetGeneratedFunctionDefinition(function);
        }

        /// <summary>
        /// Determines if a target function exists for the given function import.
        /// </summary>
        /// <param name="functionImport">Function import (function declared in a model entity container)</param>
        /// <param name="targetFunctionMapping">Function target mapping (function to which the import is mapped in the target store)</param>
        /// <returns>true if a mapped target function exists; false otherwise</returns>
        internal bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping targetFunctionMapping)
        {
            return _internalMetadataWorkspace.TryGetFunctionImportMapping(functionImport, out targetFunctionMapping);
        }

        /// <summary>
        /// Returns the view loader associated with this workspace,
        /// creating a loader if non exists. The loader includes
        /// context information used by the update pipeline when
        /// processing changes to C-space extents.
        /// </summary>
        /// <returns></returns>
        internal ViewLoader GetUpdateViewLoader()
        {
            return _internalMetadataWorkspace.GetUpdateViewLoader();
        }

        /// <summary>
        /// Takes in a Edm space type usage and converts into an
        /// equivalent O space type usage
        /// </summary>
        /// <param name="edmSpaceTypeUsage"></param>
        /// <returns></returns>
        internal TypeUsage GetOSpaceTypeUsage(TypeUsage edmSpaceTypeUsage)
        {
            return _internalMetadataWorkspace.GetOSpaceTypeUsage(edmSpaceTypeUsage);
        }

        /// <summary>
        /// Returns true if the item collection for the given space has already been registered else returns false
        /// </summary>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        internal bool IsItemCollectionAlreadyRegistered(DataSpace dataSpace)
        {
            return _internalMetadataWorkspace.IsItemCollectionAlreadyRegistered(dataSpace);
        }

        /// <summary>
        /// Requires: C, S and CS are registered in this and other
        /// Determines whether C, S and CS are equivalent. Useful in determining whether a DbCommandTree
        /// is usable within a particular entity connection.
        /// </summary>
        /// <param name="other">Other workspace.</param>
        /// <returns>true is C, S and CS collections are equivalent</returns>
        internal bool IsMetadataWorkspaceCSCompatible(MetadataWorkspace other)
        {
            return _internalMetadataWorkspace.IsInternalMetadataWorkspaceCSCompatible(other._internalMetadataWorkspace);
        }

        /// <summary>
        /// Clear all the metadata cache entries
        /// </summary>
        public static void ClearCache()
        {
            MetadataCache.Clear();
            ObjectItemCollection.ViewGenerationAssemblies.Clear();
            using (var cache = AssemblyCache.AquireLockedAssemblyCache())
            {
                cache.Clear();
            }
        }

        /// <summary>
        /// Creates a new Metadata workspace sharing the (currently defined) item collections
        /// and tokens for caching purposes.
        /// </summary>
        /// <returns></returns>
        internal MetadataWorkspace ShallowCopy()
        {
            var copy = (MetadataWorkspace)MemberwiseClone();
            copy._internalMetadataWorkspace = _internalMetadataWorkspace.ShallowCopy();
            copy._internalMetadataWorkspace.MetadataWorkspaceWrapper = copy;

            return copy;
        }

        /// <summary>
        /// Returns the canonical Model TypeUsage for a given PrimitiveTypeKind
        /// </summary>
        /// <param name="primitiveTypeKind">PrimitiveTypeKind for which a canonical TypeUsage is expected</param>
        /// <returns>a canonical model TypeUsage</returns>
        internal static TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
        {
            return EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveTypeKind);
        }

        /// <summary>
        /// Returns the Model PrimitiveType for a given primitiveTypeKind
        /// </summary>
        /// <param name="primitiveTypeKind">a PrimitiveTypeKind for which a Model PrimitiveType is expected</param>
        /// <returns>Model PrimitiveType</returns>
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
        /// <summary>
        /// Returns members of a given EntitySet/EntityType for which original values are necessary for determining which tables to modify.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet</param>
        /// <returns>Edm Members for which original Value is required</returns>
        /// <remarks>
        /// This method returns the following groups of members: 0, 1, 2, 3, 3.1, 4, 4.1. (see group descriptions above). 
        /// This method is marked as obsolete since it does not support partial update scenarios as it does not return 
        /// members from group 5 and changing it to return these members would be a breaking change.
        /// </remarks>
        [Obsolete("Use MetadataWorkspace.GetRelevantMembersForUpdate(EntitySetBase, EntityTypeBase, bool) instead")]
        public IEnumerable<EdmMember> GetRequiredOriginalValueMembers(EntitySetBase entitySet, EntityTypeBase entityType)
        {
            return _internalMetadataWorkspace.GetRequiredOriginalValueMembers(entitySet, entityType);
        }

        /// <summary>
        /// Returns members of a given EntitySet/EntityType for which original values are needed when modifying an entity.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet</param>
        /// <param name="partialUpdateSupported">Whether entities may be updated partially.</param>
        /// <returns>Edm Members for which original Value is required</returns>
        /// <remarks>
        /// This method returns the following groups of members:
        /// - if <paramref name="partialUpdateSupported"/> is <c>false</c>: 1, 2, 3, 3.1, 4, 4.1, 6 (see group descriptions above)
        /// - if <paramref name="partialUpdateSupported"/> is <c>true</c>: 1, 2, 3, 3.1, 5, 6, 7 (see group descriptions above)
        /// See DevDiv bugs #124460 and #272992 for more details.
        /// </remarks>
        public ReadOnlyCollection<EdmMember> GetRelevantMembersForUpdate(
            EntitySetBase entitySet, EntityTypeBase entityType, bool partialUpdateSupported)
        {
            return _internalMetadataWorkspace.GetRelevantMembersForUpdate(entitySet, entityType, partialUpdateSupported);
        }

        /// <summary>
        /// Returns the QueryCacheManager hosted by this metadata workspace instance
        /// </summary>
        internal QueryCacheManager GetQueryCacheManager()
        {
            return _internalMetadataWorkspace.GetQueryCacheManager();
        }

        internal bool TryDetermineCSpaceModelType<T>(out EdmType modelEdmType)
        {
            return _internalMetadataWorkspace.TryDetermineCSpaceModelType(typeof(T), out modelEdmType);
        }

        internal bool TryDetermineCSpaceModelType(Type type, out EdmType modelEdmType)
        {
            return _internalMetadataWorkspace.TryDetermineCSpaceModelType(type, out modelEdmType);
        }

        #endregion
    }
}
