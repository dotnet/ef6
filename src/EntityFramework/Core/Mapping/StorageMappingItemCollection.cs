using som = System.Data.Entity.Core.EntityModel.SchemaObjectModel;

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Xml;
    using OfTypeQVCacheKey =
        System.Data.Entity.Core.Common.Utils.Pair<Metadata.Edm.EntitySetBase, Common.Utils.Pair<Metadata.Edm.EntityTypeBase, bool>>;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [CLSCompliant(false)]
    public class StorageMappingItemCollection : MappingItemCollection
    {
        internal delegate bool TryGetUserDefinedQueryView(EntitySetBase extent, out GeneratedView generatedView);

        internal delegate bool TryGetUserDefinedQueryViewOfType(OfTypeQVCacheKey extent, out GeneratedView generatedView);

        internal class ViewDictionary
        {
            private readonly TryGetUserDefinedQueryView TryGetUserDefinedQueryView;
            private readonly TryGetUserDefinedQueryViewOfType TryGetUserDefinedQueryViewOfType;

            private readonly StorageMappingItemCollection m_storageMappingItemCollection;

            private static readonly ConfigViewGenerator MConfig = new ConfigViewGenerator();

            // Indicates whether the views are being fetched from a generated class or they are being generated at the runtime
            private bool m_generatedViewsMode = true;

            /// <summary>
            /// Caches computation of view generation per <see cref="StorageEntityContainerMapping"/>. Cached value contains both query and update views.
            /// </summary>
            private readonly Memoizer<EntityContainer, Dictionary<EntitySetBase, GeneratedView>> m_generatedViewsMemoizer;

            /// <summary>
            /// Caches computation of getting Type-specific Query Views - either by view gen or user-defined input.
            /// </summary>
            private readonly Memoizer<OfTypeQVCacheKey, GeneratedView> m_generatedViewOfTypeMemoizer;

            internal ViewDictionary(
                StorageMappingItemCollection storageMappingItemCollection,
                out Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
                out Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict)
            {
                m_storageMappingItemCollection = storageMappingItemCollection;
                m_generatedViewsMemoizer =
                    new Memoizer<EntityContainer, Dictionary<EntitySetBase, GeneratedView>>(SerializedGetGeneratedViews, null);
                m_generatedViewOfTypeMemoizer = new Memoizer<OfTypeQVCacheKey, GeneratedView>(
                    SerializedGeneratedViewOfType, OfTypeQVCacheKey.PairComparer.Instance);

                userDefinedQueryViewsDict = new Dictionary<EntitySetBase, GeneratedView>(EqualityComparer<EntitySetBase>.Default);
                userDefinedQueryViewsOfTypeDict = new Dictionary<OfTypeQVCacheKey, GeneratedView>(OfTypeQVCacheKey.PairComparer.Instance);

                TryGetUserDefinedQueryView = userDefinedQueryViewsDict.TryGetValue;
                TryGetUserDefinedQueryViewOfType = userDefinedQueryViewsOfTypeDict.TryGetValue;
            }

            private Dictionary<EntitySetBase, GeneratedView> SerializedGetGeneratedViews(EntityContainer container)
            {
                Debug.Assert(container != null);

                // Note that extentMappingViews will contain both query and update views.
                Dictionary<EntitySetBase, GeneratedView> extentMappingViews;

                // Get the mapping that has the entity container mapped.
                var entityContainerMap = MappingMetadataHelper.GetEntityContainerMap(m_storageMappingItemCollection, container);

                // We get here because memoizer didn't find an entry for the container.
                // It might happen that the entry with generated views already exists for the counterpart container, so check it first.
                var counterpartContainer = container.DataSpace == DataSpace.CSpace
                                               ? entityContainerMap.StorageEntityContainer
                                               : entityContainerMap.EdmEntityContainer;
                if (m_generatedViewsMemoizer.TryGetValue(counterpartContainer, out extentMappingViews))
                {
                    return extentMappingViews;
                }

                extentMappingViews = new Dictionary<EntitySetBase, GeneratedView>();

                if (!entityContainerMap.HasViews)
                {
                    return extentMappingViews;
                }

                // If we are in generated views mode.
                if (m_generatedViewsMode)
                {
                    if (ObjectItemCollection.ViewGenerationAssemblies != null
                        && ObjectItemCollection.ViewGenerationAssemblies.Count > 0)
                    {
                        SerializedCollectViewsFromObjectCollection(m_storageMappingItemCollection.Workspace, extentMappingViews);
                    }
                    else
                    {
                        SerializedCollectViewsFromReferencedAssemblies(m_storageMappingItemCollection.Workspace, extentMappingViews);
                    }
                }

                if (extentMappingViews.Count == 0)
                {
                    // We should change the mode to runtime generation of views.
                    m_generatedViewsMode = false;
                    SerializedGenerateViews(entityContainerMap, extentMappingViews);
                }

                Debug.Assert(extentMappingViews.Count > 0, "view should be generated at this point");

                return extentMappingViews;
            }

            /// <summary>
            /// Call the View Generator's Generate view method
            /// and collect the Views and store it in a local dictionary.
            /// </summary>
            /// <param name="entityContainerMap"></param>
            /// <param name="resultDictionary"></param>
            private static void SerializedGenerateViews(
                StorageEntityContainerMapping entityContainerMap, Dictionary<EntitySetBase, GeneratedView> resultDictionary)
            {
                //If there are no entity set maps, don't call the view generation process
                Debug.Assert(entityContainerMap.HasViews);

                var viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(entityContainerMap, MConfig);
                var extentMappingViews = viewGenResults.Views;
                if (viewGenResults.HasErrors)
                {
                    // Can get the list of errors using viewGenResults.Errors
                    throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
                }

                foreach (var keyValuePair in extentMappingViews.KeyValuePairs)
                {
                    //Multiple Views are returned for an extent but the first view
                    //is the only one that we will use for now. In the future,
                    //we might start using the other views which are per type within an extent.
                    GeneratedView generatedView;
                    //Add the view to the local dictionary

                    if (!resultDictionary.TryGetValue(keyValuePair.Key, out generatedView))
                    {
                        generatedView = keyValuePair.Value[0];
                        resultDictionary.Add(keyValuePair.Key, generatedView);
                    }
                }
            }

            /// <summary>
            /// Generates a single query view for a given Extent and type. It is used to generate OfType and OfTypeOnly views.
            /// </summary>
            /// <param name="includeSubtypes">Whether the view should include extents that are subtypes of the given entity</param>
            private bool TryGenerateQueryViewOfType(
                EntityContainer entityContainer, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes,
                out GeneratedView generatedView)
            {
                Debug.Assert(entityContainer != null);
                Debug.Assert(entity != null);
                Debug.Assert(type != null);

                if (type.Abstract)
                {
                    generatedView = null;
                    return false;
                }

                //Get the mapping that has the entity container mapped.
                var entityContainerMap = MappingMetadataHelper.GetEntityContainerMap(m_storageMappingItemCollection, entityContainer);
                Debug.Assert(!entityContainerMap.IsEmpty, "There are no entity set maps");

                bool success;
                var viewGenResults = ViewgenGatekeeper.GenerateTypeSpecificQueryView(
                    entityContainerMap, MConfig, entity, type, includeSubtypes, out success);
                if (!success)
                {
                    generatedView = null;
                    return false; //could not generate view
                }

                var extentMappingViews = viewGenResults.Views;

                if (viewGenResults.HasErrors)
                {
                    throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
                }

                Debug.Assert(extentMappingViews.AllValues.Count() == 1, "Viewgen should have produced only one view");
                generatedView = extentMappingViews.AllValues.First();

                return true;
            }

            /// <summary>
            /// Tries to generate the Oftype or OfTypeOnly query view for a given entity set and type. 
            /// Returns false if the view could not be generated.
            /// Possible reasons for failing are 
            ///   1) Passing in OfTypeOnly on an abstract type
            ///   2) In user-specified query views mode a query for the given type is absent
            /// </summary>
            internal bool TryGetGeneratedViewOfType(
                EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
            {
                var key = new OfTypeQVCacheKey(entity, new Pair<EntityTypeBase, bool>(type, includeSubtypes));
                generatedView = m_generatedViewOfTypeMemoizer.Evaluate(key);
                return (generatedView != null);
            }

            /// <summary>
            /// Note: Null return value implies QV was not generated.
            /// </summary>
            /// <returns></returns>
            private GeneratedView SerializedGeneratedViewOfType(OfTypeQVCacheKey arg)
            {
                GeneratedView generatedView;
                //See if we have collected user-defined QueryView
                if (TryGetUserDefinedQueryViewOfType(arg, out generatedView))
                {
                    return generatedView;
                }

                //Now we have to generate the type-specific view
                var entity = arg.First;
                var type = arg.Second.First;
                var includeSubtypes = arg.Second.Second;

                if (!TryGenerateQueryViewOfType(entity.EntityContainer, entity, type, includeSubtypes, out generatedView))
                {
                    generatedView = null;
                }

                return generatedView;
            }

            /// <summary>
            /// Returns the update or query view for an Extent as a
            /// string.
            /// There are a series of steps that we go through for discovering a view for an extent.
            /// To start with we assume that we are working with Generated Views. To find out the 
            /// generated view we go to the ObjectItemCollection and see if it is not-null. If the ObjectItemCollection
            /// is non-null, we get the view generation assemblies that it might have cached during the
            /// Object metadata discovery.If there are no view generation assemblies we switch to the
            /// runtime view generation strategy. If there are view generation assemblies, we get the list and
            /// go through them and see if there are any assemblies that are there from which we have not already loaded
            /// the views. We collect the views from assemblies that we have not already collected from earlier.
            /// If the ObjectItemCollection is null and we are in the view generation mode, that means that
            /// the query or update is issued from the Value layer and this is the first time view has been asked for.
            /// The compile time view gen for value layer queries will work for very simple scenarios.
            /// If the users wants to get the performance benefit, they should call MetadataWorkspace.LoadFromAssembly.
            /// At this point we go through the referenced assemblies of the entry assembly( this wont work for Asp.net 
            /// or if the viewgen assembly was not referenced by the executing application).
            /// and try to see if there were any view gen assemblies. If there are, we collect the views for all extents.
            /// Once we have all the generated views gathered, we try to get the view for the extent passed in.
            /// If we find one we will return it. If we can't find one an exception will be thrown.
            /// If there were no view gen assemblies either in the ObjectItemCollection or in the list of referenced
            /// assemblies of calling assembly, we change the mode to runtime view generation and will continue to
            /// be in that mode for the rest of the lifetime of the mapping item collection.
            /// </summary>
            internal GeneratedView GetGeneratedView(
                EntitySetBase extent, MetadataWorkspace workspace, StorageMappingItemCollection storageMappingItemCollection)
            {
                //First check if we have collected a view from user-defined query views
                //Dont need to worry whether to generate Query view or update viw, because that is relative to the extent.
                GeneratedView view;

                if (TryGetUserDefinedQueryView(extent, out view))
                {
                    return view;
                }

                //If this is a foreign key association, manufacture a view on the fly.
                if (extent.BuiltInTypeKind
                    == BuiltInTypeKind.AssociationSet)
                {
                    var aSet = (AssociationSet)extent;
                    if (aSet.ElementType.IsForeignKey)
                    {
                        if (MConfig.IsViewTracing)
                        {
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.FormatTraceLine("================= Generating FK Query View for: {0} =================", aSet.Name);
                            Helpers.StringTraceLine(String.Empty);
                            Helpers.StringTraceLine(String.Empty);
                        }

                        // Although we expose a collection of constraints in the API, there is only ever one constraint.
                        Debug.Assert(
                            aSet.ElementType.ReferentialConstraints.Count == 1, "aSet.ElementType.ReferentialConstraints.Count == 1");
                        var rc = aSet.ElementType.ReferentialConstraints.Single();

                        var dependentSet = aSet.AssociationSetEnds[rc.ToRole.Name].EntitySet;
                        var principalSet = aSet.AssociationSetEnds[rc.FromRole.Name].EntitySet;

                        DbExpression qView = dependentSet.Scan();

                        // Introduce an OfType view if the dependent end is a subtype of the entity set
                        var dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.ToRole);
                        var principalType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.FromRole);
                        if (dependentSet.ElementType.IsBaseTypeOf(dependentType))
                        {
                            qView = qView.OfType(TypeUsage.Create(dependentType));
                        }

                        if (rc.FromRole.RelationshipMultiplicity
                            == RelationshipMultiplicity.ZeroOrOne)
                        {
                            // Filter out instances with existing relationships.
                            qView = qView.Where(
                                e =>
                                    {
                                        DbExpression filter = null;
                                        foreach (var fkProp in rc.ToProperties)
                                        {
                                            DbExpression notIsNull = e.Property(fkProp).IsNull().Not();
                                            filter = null == filter ? notIsNull : filter.And(notIsNull);
                                        }
                                        return filter;
                                    });
                        }
                        qView = qView.Select(
                            e =>
                                {
                                    var ends = new List<DbExpression>();
                                    foreach (var end in aSet.ElementType.AssociationEndMembers)
                                    {
                                        if (end.Name
                                            == rc.ToRole.Name)
                                        {
                                            var keyValues = new List<KeyValuePair<string, DbExpression>>();
                                            foreach (var keyMember in dependentSet.ElementType.KeyMembers)
                                            {
                                                keyValues.Add(e.Property((EdmProperty)keyMember));
                                            }
                                            ends.Add(dependentSet.RefFromKey(DbExpressionBuilder.NewRow(keyValues), dependentType));
                                        }
                                        else
                                        {
                                            // Manufacture a key using key values.
                                            var keyValues = new List<KeyValuePair<string, DbExpression>>();
                                            foreach (var keyMember in principalSet.ElementType.KeyMembers)
                                            {
                                                var offset = rc.FromProperties.IndexOf((EdmProperty)keyMember);
                                                keyValues.Add(e.Property(rc.ToProperties[offset]));
                                            }
                                            ends.Add(principalSet.RefFromKey(DbExpressionBuilder.NewRow(keyValues), principalType));
                                        }
                                    }
                                    return TypeUsage.Create(aSet.ElementType).New(ends);
                                });
                        return GeneratedView.CreateGeneratedViewForFKAssociationSet(
                            aSet, aSet.ElementType, new DbQueryCommandTree(workspace, DataSpace.SSpace, qView), storageMappingItemCollection,
                            MConfig);
                    }
                }

                // If no User-defined QV is found, call memoized View Generation procedure.
                var generatedViews = m_generatedViewsMemoizer.Evaluate(extent.EntityContainer);

                if (!generatedViews.TryGetValue(extent, out view))
                {
                    throw new InvalidOperationException(System.Data.Entity.Resources.Strings.Mapping_Views_For_Extent_Not_Generated(
                        (extent.EntityContainer.DataSpace == DataSpace.SSpace) ? "Table" : "EntitySet", extent.Name));
                }

                return view;
            }

            /// <summary>
            /// Collect the views from object collection's view gen assembly
            /// </summary>
            /// <param name="workspace"></param>
            /// <param name="objectCollection"></param>
            private void SerializedCollectViewsFromObjectCollection(
                MetadataWorkspace workspace, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                var allViewGenAssemblies = ObjectItemCollection.ViewGenerationAssemblies;
                if (allViewGenAssemblies != null)
                {
                    foreach (var assembly in allViewGenAssemblies)
                    {
                        var viewGenAttributes =
                            assembly.GetCustomAttributes(
                                typeof(System.Data.Entity.Core.Mapping.EntityViewGenerationAttribute), false /*inherit*/);
                        if ((viewGenAttributes != null)
                            && (viewGenAttributes.Length != 0))
                        {
                            foreach (EntityViewGenerationAttribute viewGenAttribute in viewGenAttributes)
                            {
                                var viewContainerType = viewGenAttribute.ViewGenerationType;
                                if (!viewContainerType.IsSubclassOf(typeof(EntityViewContainer)))
                                {
                                    throw new InvalidOperationException(System.Data.Entity.Resources.Strings.Generated_View_Type_Super_Class(
                                        StorageMslConstructs.EntityViewGenerationTypeName));
                                }
                                var viewContainer = (Activator.CreateInstance(viewContainerType) as EntityViewContainer);
                                Debug.Assert(viewContainer != null, "Should be able to create the type");

                                SerializedAddGeneratedViewsInEntityViewContainer(workspace, viewContainer, extentMappingViews);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// this method do the following check on the generated views in the EntityViewContainer, 
            /// then add those views all at once to the dictionary
            /// 1. there should be one storeageEntityContainerMapping that has the same h
            ///     C side and S side names as the EnittyViewcontainer
            /// 2. Generate the hash for the storageEntityContainerMapping in the MM closure, 
            ///     and this hash should be the same in EntityViewContainer
            /// 3. Generate the hash for all of the view text in the EntityViewContainer and 
            ///     this hash should be the same as the stored on in the EntityViewContainer
            /// </summary>
            /// <param name="entityViewContainer"></param>
            private void SerializedAddGeneratedViewsInEntityViewContainer(
                MetadataWorkspace workspace, EntityViewContainer entityViewContainer,
                Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                StorageEntityContainerMapping storageEntityContainerMapping;
                // first check
                if (!TryGetCorrespondingStorageEntityContainerMapping(
                    entityViewContainer,
                    workspace.GetItemCollection(DataSpace.CSSpace).GetItems<StorageEntityContainerMapping>(),
                    out storageEntityContainerMapping))
                {
                    return;
                }

                // second check
                if (!SerializedVerifyHashOverMmClosure(storageEntityContainerMapping, entityViewContainer))
                {
                    throw new MappingException(
                        Strings.ViewGen_HashOnMappingClosure_Not_Matching(entityViewContainer.EdmEntityContainerName));
                }

                // third check, prior to the check, we collect all the views in the entity view container to the dictionary
                // if the views are changed then we will throw exception out
                if (VerifyViewsHaveNotChanged(workspace, entityViewContainer))
                {
                    SerializedAddGeneratedViews(workspace, entityViewContainer, extentMappingViews);
                }
                else
                {
                    throw new InvalidOperationException(System.Data.Entity.Resources.Strings.Generated_Views_Changed);
                }
            }

            private static bool TryGetCorrespondingStorageEntityContainerMapping(
                EntityViewContainer viewContainer,
                IEnumerable<StorageEntityContainerMapping> storageEntityContainerMappingList,
                out StorageEntityContainerMapping storageEntityContainerMapping)
            {
                storageEntityContainerMapping = null;

                foreach (var entityContainerMapping in storageEntityContainerMappingList)
                {
                    // first check
                    if (entityContainerMapping.EdmEntityContainer.Name == viewContainer.EdmEntityContainerName
                        &&
                        entityContainerMapping.StorageEntityContainer.Name == viewContainer.StoreEntityContainerName)
                    {
                        storageEntityContainerMapping = entityContainerMapping;
                        return true;
                    }
                }
                return false;
            }

            private bool SerializedVerifyHashOverMmClosure(
                StorageEntityContainerMapping entityContainerMapping, EntityViewContainer entityViewContainer)
            {
                if (MetadataMappingHasherVisitor.GetMappingClosureHash(
                    m_storageMappingItemCollection.MappingVersion, entityContainerMapping)
                    ==
                    entityViewContainer.HashOverMappingClosure)
                {
                    return true;
                }
                return false;
            }

            private static bool VerifyViewsHaveNotChanged(MetadataWorkspace workspace, EntityViewContainer viewContainer)
            {
                //Now check whether the hash of the generated views match the one
                //we stored in the code file during design
                //Produce the hash and add it to the code
                var mappingCollection = (workspace.GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection);
                Debug.Assert(mappingCollection != null, "Must have Mapping Collection in the Metadataworkspace");

                var viewHash = MetadataHelper.GenerateHashForAllExtentViewsContent(
                    mappingCollection.MappingVersion, viewContainer.ExtentViews);
                var storedViewHash = viewContainer.HashOverAllExtentViews;
                if (viewHash != storedViewHash)
                {
                    return false;
                }
                return true;
            }

            //Collect the names of the entitysetbases and the generated views from
            //the generated type into a string so that we can produce a hash over it.
            private void SerializedAddGeneratedViews(
                MetadataWorkspace workspace, EntityViewContainer viewContainer, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                foreach (var extentView in viewContainer.ExtentViews)
                {
                    EntityContainer entityContainer = null;
                    EntitySetBase extent = null;

                    var extentFullName = extentView.Key;
                    var extentNameIndex = extentFullName.LastIndexOf('.');

                    if (extentNameIndex != -1)
                    {
                        var entityContainerName = extentFullName.Substring(0, extentNameIndex);
                        var extentName = extentFullName.Substring(extentFullName.LastIndexOf('.') + 1);

                        if (!workspace.TryGetItem(entityContainerName, DataSpace.CSpace, out entityContainer))
                        {
                            workspace.TryGetItem(entityContainerName, DataSpace.SSpace, out entityContainer);
                        }

                        if (entityContainer != null)
                        {
                            entityContainer.BaseEntitySets.TryGetValue(extentName, false, out extent);
                        }
                    }

                    if (extent == null)
                    {
                        throw new MappingException(System.Data.Entity.Resources.Strings.Generated_Views_Invalid_Extent(extentFullName));
                    }

                    //Create a Generated view and cache it
                    GeneratedView generatedView;
                    //Add the view to the local dictionary
                    if (!extentMappingViews.TryGetValue(extent, out generatedView))
                    {
                        generatedView = GeneratedView.CreateGeneratedView(
                            extent,
                            null, // edmType
                            null, // commandTree
                            extentView.Value, // eSQL
                            m_storageMappingItemCollection,
                            new ConfigViewGenerator());
                        extentMappingViews.Add(extent, generatedView);
                    }
                }
            }

            /// <summary>
            /// Tries to collect the views from the referenced assemblies of Entry assembly.
            /// </summary>
            /// <param name="workspace"></param>
            private void SerializedCollectViewsFromReferencedAssemblies(
                MetadataWorkspace workspace, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
            {
                ObjectItemCollection objectCollection;
                ItemCollection itemCollection;
                if (!workspace.TryGetItemCollection(DataSpace.OSpace, out itemCollection))
                {
                    //Possible enhancement : Think about achieving the same thing without creating Object Item Collection.
                    objectCollection = new ObjectItemCollection();
                    itemCollection = objectCollection;
                    // The GetEntryAssembly method can return a null reference
                    //when a managed assembly has been loaded from an unmanaged application.
                    var entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        objectCollection.ImplicitLoadViewsFromAllReferencedAssemblies(entryAssembly);
                    }
                }
                SerializedCollectViewsFromObjectCollection(workspace, extentMappingViews);
            }
        }

        #region Fields

        //EdmItemCollection that is associated with the MSL Loader.
        private EdmItemCollection m_edmCollection;

        //StoreItemCollection that is associated with the MSL Loader.
        private StoreItemCollection m_storeItemCollection;
        private ViewDictionary m_viewDictionary;
        private double m_mappingVersion = XmlConstants.UndefinedVersion;

        private MetadataWorkspace m_workspace;

        // In this version, we won't allow same types in CSpace to map to different types in store. If the same type
        // need to be reused, the store type must be the same. To keep track of this, we need to keep track of the member 
        // mapping across maps to make sure they are mapped to the same store side.
        // The first TypeUsage in the KeyValuePair stores the store equivalent type for the cspace member type and the second
        // one store the actual store type to which the member is mapped to.
        // For e.g. If the CSpace member of type Edm.Int32 maps to a sspace member of type SqlServer.bigint, then the KeyValuePair
        // for the cspace member will contain SqlServer.int (store equivalent for Edm.Int32) and SqlServer.bigint (Actual store type
        // to which the member was mapped to)
        private readonly Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> m_memberMappings =
            new Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>>();

        private ViewLoader _viewLoader;

        internal enum InterestingMembersKind
        {
            RequiredOriginalValueMembers, // legacy - used by the obsolete GetRequiredOriginalValueMembers
            FullUpdate, // Interesting members in case of full update scenario
            PartialUpdate // Interesting members in case of partial update scenario
        };

        private readonly ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>>
            _cachedInterestingMembers =
                new ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>>();

        #endregion

        #region Constructors

        /// <summary>
        /// constructor that takes in a list of folder or files or a mix of both and
        /// creates metadata for mapping in all the files.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="storeCollection"></param>
        /// <param name="filePaths"></param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)]
        //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but we do not create the file paths in this method
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StorageMappingItemCollection(
            EdmItemCollection edmCollection, StoreItemCollection storeCollection,
            params string[] filePaths)
            : base(DataSpace.CSSpace)
        {
            Contract.Requires(edmCollection != null);
            Contract.Requires(storeCollection != null);
            Contract.Requires(filePaths != null);

            m_edmCollection = edmCollection;
            m_storeItemCollection = storeCollection;

            // Wrap the file paths in instances of the MetadataArtifactLoader class, which provides
            // an abstraction and a uniform interface over a diverse set of metadata artifacts.
            //
            MetadataArtifactLoader composite = null;
            List<XmlReader> readers = null;
            try
            {
                composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(filePaths, XmlConstants.CSSpaceSchemaExtension);
                readers = composite.CreateReaders(DataSpace.CSSpace);

                Init(
                    edmCollection, storeCollection, readers,
                    composite.GetPaths(DataSpace.CSSpace), true /*throwOnError*/);
            }
            finally
            {
                if (readers != null)
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StorageMappingItemCollection(
            EdmItemCollection edmCollection,
            StoreItemCollection storeCollection,
            IEnumerable<XmlReader> xmlReaders)
            : base(DataSpace.CSSpace)
        {
            Contract.Requires(xmlReaders != null);

            var composite = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);

            Init(
                edmCollection,
                storeCollection,
                composite.GetReaders(), // filter out duplicates
                composite.GetPaths(),
                true /* throwOnError*/);
        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        /// <param name="errors">a list of errors for each file loaded</param>
        // referenced by System.Data.Entity.Design.dll
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal StorageMappingItemCollection(
            EdmItemCollection edmCollection,
            StoreItemCollection storeCollection,
            IEnumerable<XmlReader> xmlReaders,
            List<string> filePaths,
            out IList<EdmSchemaError> errors)
            : base(DataSpace.CSSpace)
        {
            // we will check the parameters for this internal ctor becuase
            // it is pretty much publicly exposed through the MetadataItemCollectionFactory
            // in System.Data.Entity.Design
            Contract.Requires(xmlReaders != null);
            EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
            // filePaths is allowed to be null

            errors = Init(edmCollection, storeCollection, xmlReaders, filePaths, false /*throwOnError*/);
        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal StorageMappingItemCollection(
            EdmItemCollection edmCollection,
            StoreItemCollection storeCollection,
            IEnumerable<XmlReader> xmlReaders,
            List<string> filePaths)
            : base(DataSpace.CSSpace)
        {
            Init(edmCollection, storeCollection, xmlReaders, filePaths, true /*throwOnError*/);
        }

        /// <summary>
        /// Initializer that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        /// <param name="errors">a list of errors for each file loaded</param>
        private IList<EdmSchemaError> Init(
            EdmItemCollection edmCollection,
            StoreItemCollection storeCollection,
            IEnumerable<XmlReader> xmlReaders,
            List<string> filePaths,
            bool throwOnError)
        {
            Contract.Requires(xmlReaders != null);
            Contract.Requires(edmCollection != null);
            Contract.Requires(storeCollection != null);

            m_edmCollection = edmCollection;
            m_storeItemCollection = storeCollection;

            Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict;
            Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict;

            m_viewDictionary = new ViewDictionary(this, out userDefinedQueryViewsDict, out userDefinedQueryViewsOfTypeDict);

            var errors = new List<EdmSchemaError>();

            if (m_edmCollection.EdmVersion != XmlConstants.UndefinedVersion &&
                m_storeItemCollection.StoreSchemaVersion != XmlConstants.UndefinedVersion
                &&
                m_edmCollection.EdmVersion != m_storeItemCollection.StoreSchemaVersion)
            {
                errors.Add(
                    new EdmSchemaError(
                        Strings.Mapping_DifferentEdmStoreVersion,
                        (int)StorageMappingErrorCode.MappingDifferentEdmStoreVersion, EdmSchemaErrorSeverity.Error));
            }
            else
            {
                var expectedVersion = m_edmCollection.EdmVersion != XmlConstants.UndefinedVersion
                                          ? m_edmCollection.EdmVersion
                                          : m_storeItemCollection.StoreSchemaVersion;
                errors.AddRange(
                    LoadItems(xmlReaders, filePaths, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, expectedVersion));
            }

            Debug.Assert(errors != null);

            if (errors.Count > 0 && throwOnError)
            {
                if (!MetadataHelper.CheckIfAllErrorsAreWarnings(errors))
                {
                    // NOTE: not using Strings.InvalidSchemaEncountered because it will truncate the errors list.
                    throw new MappingException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            EntityRes.GetString(EntityRes.InvalidSchemaEncountered),
                            Helper.CombineErrorMessage(errors)));
                }
            }

            return errors;
        }

        #endregion Constructors

        internal MetadataWorkspace Workspace
        {
            get
            {
                if (m_workspace == null)
                {
                    m_workspace = new MetadataWorkspace();
                    m_workspace.RegisterItemCollection(m_edmCollection);
                    m_workspace.RegisterItemCollection(m_storeItemCollection);
                    m_workspace.RegisterItemCollection(this);
                }
                return m_workspace;
            }
        }

        /// <summary>
        /// Return the EdmItemCollection associated with the Mapping Collection
        /// </summary>
        internal EdmItemCollection EdmItemCollection
        {
            get { return m_edmCollection; }
        }

        /// <summary>
        /// Version of this StorageMappingItemCollection represents.
        /// </summary>
        public double MappingVersion
        {
            get { return m_mappingVersion; }
        }

        /// <summary>
        /// Return the StoreItemCollection associated with the Mapping Collection
        /// </summary>
        internal StoreItemCollection StoreItemCollection
        {
            get { return m_storeItemCollection; }
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
        {
            Contract.Requires(identity != null);
            if (typeSpace != DataSpace.CSpace)
            {
                throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return GetItem<Map>(identity, ignoreCase);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out Map map)
        {
            if (typeSpace != DataSpace.CSpace)
            {
                throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return TryGetItem(identity, ignoreCase, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace)
        {
            return GetMap(identity, typeSpace, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, out Map map)
        {
            return TryGetMap(identity, typeSpace, false /*ignoreCase*/, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        internal override Map GetMap(GlobalItem item)
        {
            Contract.Requires(item != null);
            var typeSpace = item.DataSpace;
            if (typeSpace != DataSpace.CSpace)
            {
                throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return GetMap(item.Identity, typeSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(GlobalItem item, out Map map)
        {
            if (item == null)
            {
                map = null;
                return false;
            }
            var typeSpace = item.DataSpace;
            if (typeSpace != DataSpace.CSpace)
            {
                map = null;
                return false;
            }
            return TryGetMap(item.Identity, typeSpace, out map);
        }

        /// <summary>
        /// This method
        ///     - generates views from the mapping elements in the collection;
        ///     - does not process user defined views - these are processed during mapping collection loading;
        ///     - does not cache generated views in the mapping collection.
        /// The main purpose is design-time view validation and generation.
        /// </summary>
        internal Dictionary<EntitySetBase, string> GenerateEntitySetViews(out IList<EdmSchemaError> errors)
        {
            var esqlViews = new Dictionary<EntitySetBase, string>();
            errors = new List<EdmSchemaError>();
            foreach (var mapping in GetItems<Map>())
            {
                var entityContainerMapping = mapping as StorageEntityContainerMapping;
                if (entityContainerMapping != null)
                {
                    // If there are no entity set maps, don't call the view generation process.
                    if (!entityContainerMapping.HasViews)
                    {
                        return esqlViews;
                    }

                    // If entityContainerMapping contains only query views, then add a warning to the errors and continue to next mapping.
                    if (!entityContainerMapping.HasMappingFragments())
                    {
                        Debug.Assert(
                            2088 == (int)StorageMappingErrorCode.MappingAllQueryViewAtCompileTime,
                            "Please change the ERRORCODE_MAPPINGALLQUERYVIEWATCOMPILETIME value as well");
                        errors.Add(
                            new EdmSchemaError(
                                Strings.Mapping_AllQueryViewAtCompileTime(entityContainerMapping.Identity),
                                (int)StorageMappingErrorCode.MappingAllQueryViewAtCompileTime,
                                EdmSchemaErrorSeverity.Warning));
                    }
                    else
                    {
                        var viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(
                            entityContainerMapping, new ConfigViewGenerator
                                                        {
                                                            GenerateEsql = true
                                                        });
                        if (viewGenResults.HasErrors)
                        {
                            ((List<EdmSchemaError>)errors).AddRange(viewGenResults.Errors);
                        }
                        var extentMappingViews = viewGenResults.Views;
                        foreach (var extentViewPair in extentMappingViews.KeyValuePairs)
                        {
                            var generatedViews = extentViewPair.Value;
                            // Multiple Views are returned for an extent but the first view
                            // is the only one that we will use for now. In the future,
                            // we might start using the other views which are per type within an extent.
                            esqlViews.Add(extentViewPair.Key, generatedViews[0].eSQL);
                        }
                    }
                }
            }
            return esqlViews;
        }

        #region Get interesting members

        /// <summary>
        /// Return members for MetdataWorkspace.GetRequiredOriginalValueMembers() and MetdataWorkspace.GetRelevantMembersForUpdate() methods.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space. Must not be null.</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <returns>ReadOnlyCollection of interesting members for the requested scenario (<paramref name="interestingMembersKind"/>).</returns>        
        internal ReadOnlyCollection<EdmMember> GetInterestingMembers(
            EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            Debug.Assert(entityType != null, "entityType != null");

            var key = new Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>(entitySet, entityType, interestingMembersKind);
            return _cachedInterestingMembers.GetOrAdd(key, FindInterestingMembers(entitySet, entityType, interestingMembersKind));
        }

        /// <summary>
        /// Finds interesting members for MetdataWorkspace.GetRequiredOriginalValueMembers() and MetdataWorkspace.GetRelevantMembersForUpdate() methods
        /// for the given <paramref name="entitySet"/> and <paramref name="entityType"/>.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space. Must not be null.</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <returns>ReadOnlyCollection of interesting members for the requested scenario (<paramref name="interestingMembersKind"/>).</returns>        
        private ReadOnlyCollection<EdmMember> FindInterestingMembers(
            EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            Debug.Assert(entityType != null, "entityType != null");

            var interestingMembers = new List<EdmMember>();

            foreach (
                var storageTypeMapping in
                    MappingMetadataHelper.GetMappingsForEntitySetAndSuperTypes(this, entitySet.EntityContainer, entitySet, entityType))
            {
                var associationTypeMapping = storageTypeMapping as StorageAssociationTypeMapping;
                if (associationTypeMapping != null)
                {
                    FindInterestingAssociationMappingMembers(associationTypeMapping, interestingMembers);
                }
                else
                {
                    Debug.Assert(storageTypeMapping is StorageEntityTypeMapping, "StorageEntityTypeMapping expected.");

                    FindInterestingEntityMappingMembers(
                        (StorageEntityTypeMapping)storageTypeMapping, interestingMembersKind, interestingMembers);
                }
            }

            // For backwards compatibility we don't return foreign keys from the obsolete MetadataWorkspace.GetRequiredOriginalValueMembers() method
            if (interestingMembersKind != InterestingMembersKind.RequiredOriginalValueMembers)
            {
                FindForeignKeyProperties(entitySet, entityType, interestingMembers);
            }

            foreach (var functionMappings in MappingMetadataHelper
                .GetModificationFunctionMappingsForEntitySetAndType(this, entitySet.EntityContainer, entitySet, entityType)
                .Where(functionMappings => functionMappings.UpdateFunctionMapping != null))
            {
                FindInterestingFunctionMappingMembers(functionMappings, interestingMembersKind, ref interestingMembers);
            }

            Debug.Assert(interestingMembers != null, "interestingMembers must never be null.");

            return new ReadOnlyCollection<EdmMember>(interestingMembers.Distinct().ToList());
        }

        /// <summary>
        /// Finds members participating in the assocciation and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="associationTypeMapping">Association type mapping. Must not be null.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private static void FindInterestingAssociationMappingMembers(
            StorageAssociationTypeMapping associationTypeMapping, List<EdmMember> interestingMembers)
        {
            Debug.Assert(associationTypeMapping != null, "entityTypeMapping != null");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            //(2) Ends participating in association are "interesting"
            interestingMembers.AddRange(
                associationTypeMapping
                    .MappingFragments
                    .SelectMany(m => m.AllProperties)
                    .OfType<StorageEndPropertyMapping>()
                    .Select(epm => epm.EndMember));
        }

        /// <summary>
        /// Finds interesting entity properties - primary keys (if requested), properties (including complex properties and nested properties)
        /// with concurrency mode set to fixed and C-Side condition members and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="entityTypeMapping">Entity type mapping. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private static void FindInterestingEntityMappingMembers(
            StorageEntityTypeMapping entityTypeMapping, InterestingMembersKind interestingMembersKind, List<EdmMember> interestingMembers)
        {
            Debug.Assert(entityTypeMapping != null, "entityTypeMapping != null");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            foreach (var propertyMapping in entityTypeMapping.MappingFragments.SelectMany(mf => mf.AllProperties))
            {
                var scalarPropMapping = propertyMapping as StorageScalarPropertyMapping;
                var complexPropMapping = propertyMapping as StorageComplexPropertyMapping;
                var conditionMapping = propertyMapping as StorageConditionPropertyMapping;

                Debug.Assert(!(propertyMapping is StorageEndPropertyMapping), "association mapping properties should be handled elsewhere.");

                Debug.Assert(
                    scalarPropMapping != null ||
                    complexPropMapping != null ||
                    conditionMapping != null, "Unimplemented property mapping");

                //scalar property
                if (scalarPropMapping != null
                    && scalarPropMapping.EdmProperty != null)
                {
                    // (0) if a member is part of the key it is interesting
                    if (MetadataHelper.IsPartOfEntityTypeKey(scalarPropMapping.EdmProperty))
                    {
                        // For backwards compatibility we do return primary keys from the obsolete MetadataWorkspace.GetRequiredOriginalValueMembers() method
                        if (interestingMembersKind == InterestingMembersKind.RequiredOriginalValueMembers)
                        {
                            interestingMembers.Add(scalarPropMapping.EdmProperty);
                        }
                    }
                        //(3) if a scalar property has Fixed concurrency mode then it is "interesting"
                    else if (MetadataHelper.GetConcurrencyMode(scalarPropMapping.EdmProperty)
                             == ConcurrencyMode.Fixed)
                    {
                        interestingMembers.Add(scalarPropMapping.EdmProperty);
                    }
                }
                else if (complexPropMapping != null)
                {
                    // (7) All complex members - partial update scenarios only
                    // (3.1) The complex property or its one of its children has fixed concurrency mode
                    if (interestingMembersKind == InterestingMembersKind.PartialUpdate ||
                        MetadataHelper.GetConcurrencyMode(complexPropMapping.EdmProperty) == ConcurrencyMode.Fixed
                        || HasFixedConcurrencyModeInAnyChildProperty(complexPropMapping))
                    {
                        interestingMembers.Add(complexPropMapping.EdmProperty);
                    }
                }
                else if (conditionMapping != null)
                {
                    //(1) C-Side condition members are 'interesting'
                    if (conditionMapping.EdmProperty != null)
                    {
                        interestingMembers.Add(conditionMapping.EdmProperty);
                    }
                }
            }
        }

        /// <summary>
        /// Recurses down the complex property to find whether any of the nseted properties has concurrency mode set to "Fixed"
        /// </summary>
        /// <param name="complexMapping">Complex property mapping. Must not be null.</param>
        /// <returns><c>true</c> if any of the descendant properties has concurrency mode set to "Fixed". Otherwise <c>false</c>.</returns>
        private static bool HasFixedConcurrencyModeInAnyChildProperty(StorageComplexPropertyMapping complexMapping)
        {
            Debug.Assert(complexMapping != null, "complexMapping != null");

            foreach (var propertyMapping in complexMapping.TypeMappings.SelectMany(m => m.AllProperties))
            {
                var childScalarPropertyMapping = propertyMapping as StorageScalarPropertyMapping;
                var childComplexPropertyMapping = propertyMapping as StorageComplexPropertyMapping;

                Debug.Assert(
                    childScalarPropertyMapping != null ||
                    childComplexPropertyMapping != null, "Unimplemented property mapping for complex property");

                //scalar property and has Fixed CC mode
                if (childScalarPropertyMapping != null
                    && MetadataHelper.GetConcurrencyMode(childScalarPropertyMapping.EdmProperty) == ConcurrencyMode.Fixed)
                {
                    return true;
                }
                    // Complex Prop and sub-properties or itself has fixed CC mode
                else if (childComplexPropertyMapping != null
                         &&
                         (MetadataHelper.GetConcurrencyMode(childComplexPropertyMapping.EdmProperty) == ConcurrencyMode.Fixed
                          || HasFixedConcurrencyModeInAnyChildProperty(childComplexPropertyMapping)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds foreign key properties and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="entitySetBase">Entity set <paramref name="entityType"/> relates to. Must not be null.</param>
        /// <param name="entityType">Entity type for which to find foreign key properties. Must not be null.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private static void FindForeignKeyProperties(
            EntitySetBase entitySetBase, EntityTypeBase entityType, List<EdmMember> interestingMembers)
        {
            var entitySet = entitySetBase as EntitySet;
            if (entitySet != null
                && entitySet.HasForeignKeyRelationships)
            {
                // (6) Foreign keys
                // select all foreign key properties defined on the entityType and all its ancestors
                interestingMembers.AddRange(
                    MetadataHelper.GetTypeAndParentTypesOf(entityType, true)
                        .SelectMany(e => ((EntityType)e).Properties)
                        .Where(p => entitySet.ForeignKeyDependents.SelectMany(fk => fk.Item2.ToProperties).Contains(p)));
            }
        }

        /// <summary>
        /// Finds interesting members for modification functions mapped to stored procedures and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="functionMappings">Modification function mapping. Must not be null.</param>
        /// <param name="interestingMembersKind">Update scenario the members will be used in (in general - partial update vs. full update).</param>
        /// <param name="interestingMembers"></param>
        private static void FindInterestingFunctionMappingMembers(
            StorageEntityTypeModificationFunctionMapping functionMappings, InterestingMembersKind interestingMembersKind,
            ref List<EdmMember> interestingMembers)
        {
            Debug.Assert(
                functionMappings != null && functionMappings.UpdateFunctionMapping != null,
                "Expected function mapping fragment with non-null update function mapping");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            // for partial update scenarios (e.g. EntityDataSourceControl) all members are interesting otherwise the data may be corrupt. 
            // See bugs #272992 and #124460 in DevDiv database for more details. For full update scenarios and the obsolete 
            // MetadataWorkspace.GetRequiredOriginalValueMembers() metod we return only members with Version set to "Original".
            if (interestingMembersKind == InterestingMembersKind.PartialUpdate)
            {
                // (5) Members included in Update ModificationFunction
                interestingMembers.AddRange(
                    functionMappings.UpdateFunctionMapping.ParameterBindings.Select(p => p.MemberPath.Members.Last()));
            }
            else
            {
                //(4) Members in update ModificationFunction with Version="Original" are "interesting"
                // This also works when you have complex-types (4.1)

                Debug.Assert(
                    interestingMembersKind == InterestingMembersKind.FullUpdate
                    || interestingMembersKind == InterestingMembersKind.RequiredOriginalValueMembers,
                    "Unexpected kind of interesting members - if you changed the InterestingMembersKind enum type update this code accordingly");

                foreach (var parameterBinding in functionMappings.UpdateFunctionMapping.ParameterBindings.Where(p => !p.IsCurrent))
                {
                    //Last is the root element (with respect to the Entity)
                    //For example,  Entity1={
                    //                  S1, 
                    //                  C1{S2, 
                    //                     C2{ S3, S4 } 
                    //                     }, 
                    //                  S5}
                    // if S4 matches (i.e. C1.C2.S4), then it returns C1
                    //because internally the list is [S4][C2][C1]
                    interestingMembers.Add(parameterBinding.MemberPath.Members.Last());
                }
            }
        }

        #endregion

        /// <summary>
        /// Calls the view dictionary to load the view, see detailed comments in the view dictionary class.
        /// </summary>
        internal GeneratedView GetGeneratedView(EntitySetBase extent, MetadataWorkspace workspace)
        {
            return m_viewDictionary.GetGeneratedView(extent, workspace, this);
        }

        // Add to the cache. If it is already present, then throw an exception
        private void AddInternal(Map storageMap)
        {
            storageMap.DataSpace = DataSpace.CSSpace;
            try
            {
                base.AddInternal(storageMap);
            }
            catch (ArgumentException e)
            {
                throw new MappingException(Strings.Mapping_Duplicate_Type(storageMap.EdmItem.Identity), e);
            }
        }

        // Contains whether the given StorageEntityContainerName
        internal bool ContainsStorageEntityContainer(string storageEntityContainerName)
        {
            var entityContainerMaps =
                GetItems<StorageEntityContainerMapping>();
            return
                entityContainerMaps.Any(map => map.StorageEntityContainer.Name.Equals(storageEntityContainerName, StringComparison.Ordinal));
        }

        /// <summary>
        /// This helper method loads items based on contents of in-memory XmlReader instances.
        /// Assumption: This method is called only from the constructor because m_extentMappingViews is not thread safe.
        /// </summary>
        /// <param name="xmlReaders">A list of XmlReader instances</param>
        /// <param name="mappingSchemaUris">A list of URIs</param>
        /// <returns>A list of schema errors</returns>
        private List<EdmSchemaError> LoadItems(
            IEnumerable<XmlReader> xmlReaders,
            List<string> mappingSchemaUris,
            Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
            Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict,
            double expectedVersion)
        {
            Debug.Assert(
                m_memberMappings.Count == 0,
                "Assumption: This method is called only once, and from the constructor because m_extentMappingViews is not thread safe.");

            var errors = new List<EdmSchemaError>();

            var index = -1;
            foreach (var xmlReader in xmlReaders)
            {
                index++;
                string location = null;
                if (mappingSchemaUris == null)
                {
                    som.SchemaManager.TryGetBaseUri(xmlReader, out location);
                }
                else
                {
                    location = mappingSchemaUris[index];
                }

                var mapLoader = new StorageMappingItemLoader(
                    xmlReader,
                    this,
                    location, // ASSUMPTION: location is only used for generating error-messages
                    m_memberMappings);
                errors.AddRange(mapLoader.ParsingErrors);

                CheckIsSameVersion(expectedVersion, mapLoader.MappingVersion, errors);

                // Process container mapping.
                var containerMapping = mapLoader.ContainerMapping;
                if (mapLoader.HasQueryViews
                    && containerMapping != null)
                {
                    // Compile the query views so that we can report the errors in the user specified views.
                    CompileUserDefinedQueryViews(containerMapping, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, errors);
                }
                // Add container mapping if there are no errors and entity container mapping is not already present.
                if (MetadataHelper.CheckIfAllErrorsAreWarnings(errors)
                    && !Contains(containerMapping))
                {
                    AddInternal(containerMapping);
                }
            }

            CheckForDuplicateItems(EdmItemCollection, StoreItemCollection, errors);

            return errors;
        }

        /// <summary>
        /// This method compiles all the user defined query views in the <paramref name="entityContainerMapping"/>.
        /// </summary>
        private static void CompileUserDefinedQueryViews(
            StorageEntityContainerMapping entityContainerMapping,
            Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
            Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict,
            IList<EdmSchemaError> errors)
        {
            var config = new ConfigViewGenerator();
            foreach (var setMapping in entityContainerMapping.AllSetMaps)
            {
                if (setMapping.QueryView != null)
                {
                    GeneratedView generatedView;
                    if (!userDefinedQueryViewsDict.TryGetValue(setMapping.Set, out generatedView))
                    {
                        // Parse the view so that we will get back any errors in the view.
                        if (GeneratedView.TryParseUserSpecifiedView(
                            setMapping,
                            setMapping.Set.ElementType,
                            setMapping.QueryView,
                            true, // includeSubtypes
                            entityContainerMapping.StorageMappingItemCollection,
                            config,
                            /*out*/ errors,
                            out generatedView))
                        {
                            // Add first QueryView
                            userDefinedQueryViewsDict.Add(setMapping.Set, generatedView);
                        }

                        // Add all type-specific QueryViews
                        foreach (var key in setMapping.GetTypeSpecificQVKeys())
                        {
                            Debug.Assert(key.First.Equals(setMapping.Set));

                            if (GeneratedView.TryParseUserSpecifiedView(
                                setMapping,
                                key.Second.First, // type
                                setMapping.GetTypeSpecificQueryView(key),
                                key.Second.Second, // includeSubtypes
                                entityContainerMapping.StorageMappingItemCollection,
                                config,
                                /*out*/ errors,
                                out generatedView))
                            {
                                userDefinedQueryViewsOfTypeDict.Add(key, generatedView);
                            }
                        }
                    }
                }
            }
        }

        private void CheckIsSameVersion(double expectedVersion, double currentLoaderVersion, IList<EdmSchemaError> errors)
        {
            if (m_mappingVersion == XmlConstants.UndefinedVersion)
            {
                m_mappingVersion = currentLoaderVersion;
            }
            if (expectedVersion != XmlConstants.UndefinedVersion && currentLoaderVersion != XmlConstants.UndefinedVersion
                && currentLoaderVersion != expectedVersion)
            {
                // Check that the mapping version is the same as the storage and model version
                errors.Add(
                    new EdmSchemaError(
                        Strings.Mapping_DifferentMappingEdmStoreVersion,
                        (int)StorageMappingErrorCode.MappingDifferentMappingEdmStoreVersion, EdmSchemaErrorSeverity.Error));
            }
            if (currentLoaderVersion != m_mappingVersion
                && currentLoaderVersion != XmlConstants.UndefinedVersion)
            {
                // Check that the mapping versions are all consistent with each other
                errors.Add(
                    new EdmSchemaError(
                        Strings.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                        (int)StorageMappingErrorCode.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                        EdmSchemaErrorSeverity.Error));
            }
        }

        /// <summary>
        /// Return the update view loader
        /// </summary>
        /// <returns></returns>
        internal ViewLoader GetUpdateViewLoader()
        {
            if (_viewLoader == null)
            {
                _viewLoader = new ViewLoader(this);
            }

            return _viewLoader;
        }

        /// <summary>
        /// this method will be called in metadatworkspace, the signature is the same as the one in ViewDictionary
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="includeSubtypes"></param>
        /// <param name="generatedView"></param>
        /// <returns></returns>
        internal bool TryGetGeneratedViewOfType(
            EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
        {
            return m_viewDictionary.TryGetGeneratedViewOfType(entity, type, includeSubtypes, out generatedView);
        }

        // Check for duplicate items (items with same name) in edm item collection and store item collection. Mapping is the only logical place to do this. 
        // The only other place is workspace, but that is at the time of registering item collections (only when the second one gets registered) and we 
        // will have to throw exceptions at that time. If we do this check in mapping, we might throw error in a more consistent way (by adding it to error
        // collection). Also if someone is just creating item collection, and not registering it with workspace (tools), doing it in mapping makes more sense
        private static void CheckForDuplicateItems(
            EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, List<EdmSchemaError> errorCollection)
        {
            Debug.Assert(
                edmItemCollection != null && storeItemCollection != null && errorCollection != null,
                "The parameters must not be null in CheckForDuplicateItems");

            foreach (var item in edmItemCollection)
            {
                if (storeItemCollection.Contains(item.Identity))
                {
                    errorCollection.Add(
                        new EdmSchemaError(
                            Strings.Mapping_ItemWithSameNameExistsBothInCSpaceAndSSpace(item.Identity),
                            (int)StorageMappingErrorCode.ItemWithSameNameExistsBothInCSpaceAndSSpace, EdmSchemaErrorSeverity.Error));
                }
            }
        }
    }

//---- ItemCollection
}

//---- 
