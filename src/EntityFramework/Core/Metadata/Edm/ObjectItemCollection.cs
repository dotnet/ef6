// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    ///     Class for representing a collection of items for the object layer.
    ///     Most of the implementation for actual maintenance of the collection is
    ///     done by ItemCollection
    /// </summary>
    [CLSCompliant(false)]
    public class ObjectItemCollection : ItemCollection
    {
        /// <summary>
        ///     The ObjectItemCollection that loads metadata from assemblies
        /// </summary>
        public ObjectItemCollection()
            : this(null)
        {
        }

        internal ObjectItemCollection(IViewAssemblyCache viewAssemblyCache, KnownAssembliesSet knownAssembliesSet = null)
            : base(DataSpace.OSpace)
        {
            _viewAssemblyCache = viewAssemblyCache ?? DbConfiguration.GetService<IViewAssemblyCache>();
            _knownAssemblies = knownAssembliesSet ?? new KnownAssembliesSet();

            foreach (var type in ClrProviderManifest.Instance.GetStoreTypes())
            {
                AddInternal(type);
                _primitiveTypeMaps.Add(type);
            }
        }

        // Cache for primitive type maps for Edm to provider
        private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();

        // Used for tracking the loading of an assembly and its referenced assemblies. Though the value of an entry is bool, the logic represented
        // by an entry is tri-state, the third state represented by a "missing" entry. To summarize:
        // 1. The <value> associated with an <entry> is "true"  : Specified and all referenced assemblies have been loaded 
        // 2. The <value> associated with an <entry> is "false" : Specified assembly loaded. Its referenced assemblies may not be loaded
        // 3. The <entry> is missing                            : Specified assembly has not been loaded
        private KnownAssembliesSet _knownAssemblies = new KnownAssembliesSet();

        // Dictionary which keeps tracks of oc mapping information - the key is the conceptual name of the type
        // and the value is the reference to the ospace type
        private readonly Dictionary<string, EdmType> _ocMapping = new Dictionary<string, EdmType>();

        private object _loaderCookie;
        private readonly object _loadAssemblyLock = new object();

        private readonly IViewAssemblyCache _viewAssemblyCache;

        internal bool OSpaceTypesLoaded { get; set; }

        internal object LoadAssemblyLock
        {
            get { return _loadAssemblyLock; }
        }

        /// <summary>
        ///     The method loads the O-space metadata for all the referenced assemblies starting from the given assembly
        ///     in a recursive way.
        ///     The assembly should be from Assembly.GetCallingAssembly via one of our public API's.
        /// </summary>
        /// <param name="assembly"> assembly whose dependency list we are going to traverse </param>
        internal void ImplicitLoadAllReferencedAssemblies(Assembly assembly, EdmItemCollection edmItemCollection)
        {
            if (!MetadataAssemblyHelper.ShouldFilterAssembly(assembly))
            {
                LoadAssemblyFromCache(assembly, true, edmItemCollection, null);
            }
        }

        /// <summary>
        ///     Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly"> The assembly from which to load metadata </param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly)
        {
            ExplicitLoadFromAssembly(assembly, null, null);
        }

        /// <summary>
        ///     Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly"> The assembly from which to load metadata </param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection, Action<String> logLoadMessage)
        {
            Check.NotNull(assembly, "assembly");
            Check.NotNull(edmItemCollection, "edmItemCollection");
            Check.NotNull(logLoadMessage, "logLoadMessage");

            ExplicitLoadFromAssembly(assembly, edmItemCollection, logLoadMessage);
        }

        public void LoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection)
        {
            Check.NotNull(assembly, "assembly");
            Check.NotNull(edmItemCollection, "edmItemCollection");

            ExplicitLoadFromAssembly(assembly, edmItemCollection, null);
        }

        /// <summary>
        ///     Explicit loading means that the user specifically asked us to load this assembly.
        ///     We won't do any filtering, they "know what they are doing"
        /// </summary>
        internal void ExplicitLoadFromAssembly(Assembly assembly, EdmItemCollection edmItemCollection, Action<String> logLoadMessage)
        {
            LoadAssemblyFromCache(assembly, false /*loadAllReferencedAssemblies*/, edmItemCollection, logLoadMessage);
        }

        /// <summary>
        ///     Implicit loading means that we are trying to help the user find the right
        ///     assembly, but they didn't explicitly ask for it. Our Implicit rules require that
        ///     we filter out assemblies with the Ecma or MicrosoftPublic PublicKeyToken on them
        ///     Load metadata from the type's assembly.
        /// </summary>
        /// <param name="type"> The type's assembly is loaded into the OSpace ItemCollection </param>
        /// <returns> true if the type and all its generic arguments are filtered out (did not attempt to load assembly) </returns>
        internal bool ImplicitLoadAssemblyForType(Type type, EdmItemCollection edmItemCollection)
        {
            var result = false;

            if (!MetadataAssemblyHelper.ShouldFilterAssembly(type.Assembly))
            {
                // InternalLoadFromAssembly will check _knownAssemblies
                result = LoadAssemblyFromCache(type.Assembly, false /*loadAllReferencedAssemblies*/, edmItemCollection, null);
            }

            if (type.IsGenericType)
            {
                // recursively load all generic types
                // interesting code paths are ObjectQuery<Nullable<Int32>>, ObjectQuery<IEnumerable<Product>>
                foreach (var t in type.GetGenericArguments())
                {
                    result |= ImplicitLoadAssemblyForType(t, edmItemCollection);
                }
            }
            return result;
        }

        /// <summary>
        ///     internal static method to get the relationship name
        /// </summary>
        /// <param name="clrType"> </param>
        /// <param name="relationshipName"> </param>
        /// <returns> </returns>
        internal AssociationType GetRelationshipType(string relationshipName)
        {
            AssociationType associationType;
            if (TryGetItem(relationshipName, out associationType))
            {
                return associationType;
            }
            return null;
        }

        private bool LoadAssemblyFromCache(
            Assembly assembly, bool loadReferencedAssemblies, EdmItemCollection edmItemCollection, Action<String> logLoadMessage)
        {
            _viewAssemblyCache.CheckAssembly(assembly, loadReferencedAssemblies);

            // Code First already did type loading
            if (OSpaceTypesLoaded)
            {
                return true;
            }

            // Check if its loaded in the cache - if the call is for loading referenced assemblies, make sure that all referenced
            // assemblies are also loaded
            KnownAssemblyEntry entry;
            if (_knownAssemblies.TryGetKnownAssembly(assembly, _loaderCookie, edmItemCollection, out entry))
            {
                // Proceed if only we need to load the referenced assemblies and they are not loaded
                if (loadReferencedAssemblies == false)
                {
                    // don't say we loaded anything, unless we actually did before
                    return entry.CacheEntry.TypesInAssembly.Count != 0;
                }
                else if (entry.ReferencedAssembliesAreLoaded)
                {
                    // this assembly was part of a all hands reference search
                    return true;
                }
            }

            lock (LoadAssemblyLock)
            {
                // Check after acquiring the lock, since the known assemblies might have got modified
                // Check if the assembly is already loaded. The reason we need to check if the assembly is already loaded, is that 
                if (_knownAssemblies.TryGetKnownAssembly(assembly, _loaderCookie, edmItemCollection, out entry))
                {
                    // Proceed if only we need to load the referenced assemblies and they are not loaded
                    if (loadReferencedAssemblies == false
                        || entry.ReferencedAssembliesAreLoaded)
                    {
                        return true;
                    }
                }

                Dictionary<string, EdmType> typesInLoading;
                List<EdmItemError> errors;
                var knownAssemblies = new KnownAssembliesSet(_knownAssemblies);

                // Load the assembly from the cache
                AssemblyCache.LoadAssembly(
                    assembly, loadReferencedAssemblies, knownAssemblies, edmItemCollection, logLoadMessage,
                    ref _loaderCookie, out typesInLoading, out errors);

                // Throw if we have encountered errors
                if (errors.Count != 0)
                {
                    throw EntityUtil.InvalidSchemaEncountered(Helper.CombineErrorMessage(errors));
                }

                // We can encounter new assemblies, but they may not have any time in them
                if (typesInLoading.Count != 0)
                {
                    // No errors, so go ahead and add the types and make them readonly
                    // The existence of the loading lock tells us whether we should be thread safe or not, if we need
                    // to be thread safe, then we need to use AtomicAddRange. We don't need to actually use the lock
                    // because the caller should have done it already
                    // Recheck the assemblies added, another list is created just to match up the collection type
                    // taken in by AtomicAddRange()
                    AddLoadedTypes(typesInLoading);
                }

                // Update the value of known assemblies
                _knownAssemblies = knownAssemblies;

                return typesInLoading.Count != 0;
            }
        }

        internal virtual void AddLoadedTypes(Dictionary<string, EdmType> typesInLoading)
        {
            DebugCheck.NotNull(typesInLoading);

            var globalItems = new List<GlobalItem>();
            foreach (var edmType in typesInLoading.Values)
            {
                globalItems.Add(edmType);

                var cspaceTypeName = "";
                try
                {
                    // Also populate the ocmapping information
                    if (Helper.IsEntityType(edmType))
                    {
                        cspaceTypeName = ((ClrEntityType)edmType).CSpaceTypeName;
                        _ocMapping.Add(cspaceTypeName, edmType);
                    }
                    else if (Helper.IsComplexType(edmType))
                    {
                        cspaceTypeName = ((ClrComplexType)edmType).CSpaceTypeName;
                        _ocMapping.Add(cspaceTypeName, edmType);
                    }
                    else if (Helper.IsEnumType(edmType))
                    {
                        cspaceTypeName = ((ClrEnumType)edmType).CSpaceTypeName;
                        _ocMapping.Add(cspaceTypeName, edmType);
                    }
                    // for the rest of the types like a relationship type, we do not have oc mapping, 
                    // so we don't keep that information
                }
                catch (ArgumentException e)
                {
                    throw new MappingException(Strings.Mapping_CannotMapCLRTypeMultipleTimes(cspaceTypeName), e);
                }
            }

            // Create a new ObjectItemCollection and add all the global items to it. 
            // Also copy all the existing items from the existing collection
            AtomicAddRange(globalItems);
        }

        /// <summary>
        ///     Get the list of primitive types for the given space
        /// </summary>
        /// <returns> </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<PrimitiveType> GetPrimitiveTypes()
        {
            return _primitiveTypeMaps.GetTypes();
        }

        /// <summary>
        ///     The method returns the underlying CLR type for the specified OSpace type argument.
        ///     If the DataSpace of the parameter is not OSpace, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace type to look up </param>
        /// <returns> The CLR type of the OSpace argument </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Type GetClrType(StructuralType objectSpaceType)
        {
            return GetClrType((EdmType)objectSpaceType);
        }

        /// <summary>
        ///     The method returns the underlying CLR type for the specified OSpace type argument.
        ///     If the DataSpace of the parameter is not OSpace, the method returns false and sets
        ///     the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace type to look up </param>
        /// <param name="clrType"> The CLR type of the OSpace argument </param>
        /// <returns> true on success, false on failure </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool TryGetClrType(StructuralType objectSpaceType, out Type clrType)
        {
            return TryGetClrType((EdmType)objectSpaceType, out clrType);
        }

        /// <summary>
        ///     The method returns the underlying CLR type for the specified OSpace type argument.
        ///     If the DataSpace of the parameter is not OSpace, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace type to look up </param>
        /// <returns> The CLR type of the OSpace argument </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Type GetClrType(EnumType objectSpaceType)
        {
            return GetClrType((EdmType)objectSpaceType);
        }

        /// <summary>
        ///     The method returns the underlying CLR type for the specified OSpace enum type argument.
        ///     If the DataSpace of the parameter is not OSpace, the method returns false and sets
        ///     the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace enum type to look up </param>
        /// <param name="clrType"> The CLR enum type of the OSpace argument </param>
        /// <returns> true on success, false on failure </returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool TryGetClrType(EnumType objectSpaceType, out Type clrType)
        {
            return TryGetClrType((EdmType)objectSpaceType, out clrType);
        }

        /// <summary>
        ///     A helper method returning the underlying CLR type for the specified OSpace Enum or Structural type argument.
        ///     If the DataSpace of the parameter is not OSpace, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace type to look up </param>
        /// <returns> The CLR type of the OSpace argument </returns>
        private static Type GetClrType(EdmType objectSpaceType)
        {
            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only enum or structural type expected");

            Type clrType;
            if (!TryGetClrType(objectSpaceType, out clrType))
            {
                throw new ArgumentException(Strings.FailedToFindClrTypeMapping(objectSpaceType.Identity));
            }

            return clrType;
        }

        /// <summary>
        ///     A helper method returning the underlying CLR type for the specified OSpace enum or structural type argument.
        ///     If the DataSpace of the parameter is not OSpace, the method returns false and sets
        ///     the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType"> The OSpace enum type to look up </param>
        /// <param name="clrType"> The CLR enum type of the OSpace argument </param>
        /// <returns> true on success, false on failure </returns>
        private static bool TryGetClrType(EdmType objectSpaceType, out Type clrType)
        {
            DebugCheck.NotNull(objectSpaceType);

            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only enum or structural type expected");

            if (objectSpaceType.DataSpace != DataSpace.OSpace)
            {
                throw new ArgumentException(Strings.ArgumentMustBeOSpaceType, "objectSpaceType");
            }

            clrType = null;

            if (Helper.IsEntityType(objectSpaceType)
                || Helper.IsComplexType(objectSpaceType)
                || Helper.IsEnumType(objectSpaceType))
            {
                Debug.Assert(
                    objectSpaceType is ClrEntityType || objectSpaceType is ClrComplexType || objectSpaceType is ClrEnumType,
                    "Unexpected OSpace object type.");

                clrType = objectSpaceType.ClrType;

                Debug.Assert(clrType != null, "ClrType property of ClrEntityType/ClrComplexType/ClrEnumType objects must not be null");
            }

            return clrType != null;
        }

        /// <summary>
        ///     Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="modelType"> canonical primitive type </param>
        /// <returns> The mapped scalar type </returns>
        internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind modelType)
        {
            if (Helper.IsGeometricTypeKind(modelType))
            {
                modelType = PrimitiveTypeKind.Geometry;
            }
            else if (Helper.IsGeographicTypeKind(modelType))
            {
                modelType = PrimitiveTypeKind.Geography;
            }

            PrimitiveType type = null;
            _primitiveTypeMaps.TryGetType(modelType, null, out type);
            return type;
        }

        /// <summary>
        ///     Get the OSpace type given the CSpace typename
        /// </summary>
        /// <param name="cspaceTypeName"> </param>
        /// <param name="ignoreCase"> </param>
        /// <param name="edmType"> </param>
        /// <returns> </returns>
        internal bool TryGetOSpaceType(EdmType cspaceType, out EdmType edmType)
        {
            Debug.Assert(DataSpace.CSpace == cspaceType.DataSpace, "DataSpace should be CSpace");

            // check if there is an entity, complex type or enum type mapping with this name
            if (Helper.IsEntityType(cspaceType)
                || Helper.IsComplexType(cspaceType)
                || Helper.IsEnumType(cspaceType))
            {
                return _ocMapping.TryGetValue(cspaceType.Identity, out edmType);
            }

            return TryGetItem(cspaceType.Identity, out edmType);
        }

        /// <summary>
        ///     Given the ospace type, returns the fullname of the mapped cspace type.
        ///     Today, since we allow non-default mapping between entity type and complex type,
        ///     this is only possible for entity and complex type.
        /// </summary>
        /// <param name="edmType"> </param>
        /// <returns> </returns>
        internal static string TryGetMappingCSpaceTypeIdentity(EdmType edmType)
        {
            Debug.Assert(DataSpace.OSpace == edmType.DataSpace, "DataSpace must be OSpace");

            if (Helper.IsEntityType(edmType))
            {
                return ((ClrEntityType)edmType).CSpaceTypeName;
            }
            else if (Helper.IsComplexType(edmType))
            {
                return ((ClrComplexType)edmType).CSpaceTypeName;
            }
            else if (Helper.IsEnumType(edmType))
            {
                return ((ClrEnumType)edmType).CSpaceTypeName;
            }

            return edmType.Identity;
        }

        public override ReadOnlyCollection<T> GetItems<T>()
        {
            return base.InternalGetItems(typeof(T)) as ReadOnlyCollection<T>;
        }
    }
}
