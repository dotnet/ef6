// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Reflection;

    // <summary>
    // Shapes store reader values into EntityClient/ObjectQuery results. Also maintains
    // state used by materializer delegates.
    // </summary>
    internal abstract class Shaper
    {
        internal Shaper(
            DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption,
            int stateCount, bool streaming)
        {
            Debug.Assert(context == null || workspace == context.MetadataWorkspace, "workspace must match context's workspace");

            Reader = reader;
            MergeOption = mergeOption;
            State = new object[stateCount];
            Context = context;
            Workspace = workspace;
            _spatialReader = new Lazy<DbSpatialDataReader>(CreateSpatialDataReader);
            Streaming = streaming;
        }

        // <summary>
        // Keeps track of the entities that have been materialized so that we can fire an OnMaterialized
        // for them before returning control to the caller.
        // </summary>
        private IList<IEntityWrapper> _materializedEntities;

        #region Runtime callable/accessible code

        // Code in this section is called from the delegates produced by the Translator.  It   
        // may not show up if you search using Find All References...use Find in Files instead.
        // 
        // Many items on this class are public, simply to make the job of producing the
        // expressions that use them simpler.  If you have a hankering to make them private,
        // you will need to modify the code in the Translator that does the GetMethod/GetField
        // to use BindingFlags.NonPublic | BindingFlags.Instance as well.
        //
        // Debug.Asserts that fire from the code in this region will probably create a 
        // SecurityException in the Coordinator's Read method since those are restricted when 
        // running the Shaper.

        // <summary>
        // The store data reader we're pulling data from
        // </summary>
        public readonly DbDataReader Reader;

        // <summary>
        // The state slots we use in the coordinator expression.
        // </summary>
        public readonly object[] State;

        // <summary>
        // The context the shaper is performing for.
        // </summary>
        public readonly ObjectContext Context;

        // <summary>
        // The workspace we are performing for; yes we could get it from the context, but
        // it's much easier to just have it handy.
        // </summary>
        public readonly MetadataWorkspace Workspace;

        // <summary>
        // The merge option this shaper is performing under/for.
        // </summary>
        public readonly MergeOption MergeOption;

        protected readonly bool Streaming;
        
        // <summary>
        // Utility method used to evaluate a multi-discriminator column map. Takes
        // discriminator values and determines the appropriate entity type, then looks up
        // the appropriate handler and invokes it.
        // </summary>
        public TElement Discriminate<TElement>(
            object[] discriminatorValues, Func<object[], EntityType> discriminate,
            KeyValuePair<EntityType, Func<Shaper, TElement>>[] elementDelegates)
        {
            var entityType = discriminate(discriminatorValues);
            Func<Shaper, TElement> elementDelegate = null;
            foreach (var typeDelegatePair in elementDelegates)
            {
                if (typeDelegatePair.Key == entityType)
                {
                    elementDelegate = typeDelegatePair.Value;
                }
            }
            return elementDelegate(this);
        }

        public IEntityWrapper HandleEntityNoTracking<TEntity>(IEntityWrapper wrappedEntity)
        {
            DebugCheck.NotNull(wrappedEntity);
            RegisterMaterializedEntityForEvent(wrappedEntity);
            return wrappedEntity;
        }

        // <summary>
        // REQUIRES:: entity is not null and MergeOption is OverwriteChanges or PreserveChanges
        // Handles state management for an entity returned by a query. Where an existing entry
        // exists, updates that entry and returns the existing entity. Otherwise, the entity
        // passed in is returned.
        // </summary>
        public IEntityWrapper HandleEntity<TEntity>(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySet)
        {
            Debug.Assert(MergeOption.NoTracking != MergeOption, "no need to HandleEntity if there's no tracking");
            Debug.Assert(MergeOption.AppendOnly != MergeOption, "use HandleEntityAppendOnly instead...");
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);

            var result = wrappedEntity;

            // no entity set, so no tracking is required for this entity
            if (null != (object)entityKey)
            {
                Debug.Assert(null != entitySet, "if there is an entity key, there must also be an entity set");

                // check for an existing entity with the same key
                var existingEntry = Context.ObjectStateManager.FindEntityEntry(entityKey);
                if (null != existingEntry
                    && !existingEntry.IsKeyEntry)
                {
                    Debug.Assert(existingEntry.EntityKey.Equals(entityKey), "Found ObjectStateEntry with wrong EntityKey");
                    UpdateEntry<TEntity>(wrappedEntity, existingEntry);
                    result = existingEntry.WrappedEntity;
                }
                else
                {
                    RegisterMaterializedEntityForEvent(result);
                    if (null == existingEntry)
                    {
                        Context.ObjectStateManager.AddEntry(wrappedEntity, entityKey, entitySet, "HandleEntity", false);
                    }
                    else
                    {
                        Context.ObjectStateManager.PromoteKeyEntry(
                            existingEntry, wrappedEntity, false, /*setIsLoaded*/ true, /*keyEntryInitialized*/ false);
                    }
                }
            }
            return result;
        }

        // <summary>
        // REQUIRES:: entity exists; MergeOption is AppendOnly
        // Handles state management for an entity with the given key. When the entity already exists
        // in the state manager, it is returned directly. Otherwise, the entityDelegate is invoked and
        // the resulting entity is returned.
        // </summary>
        public IEntityWrapper HandleEntityAppendOnly<TEntity>(
            Func<Shaper, IEntityWrapper> constructEntityDelegate, EntityKey entityKey, EntitySet entitySet)
        {
            Debug.Assert(MergeOption == MergeOption.AppendOnly, "only use HandleEntityAppendOnly when MergeOption is AppendOnly");
            DebugCheck.NotNull(constructEntityDelegate);

            IEntityWrapper result;

            if (null == (object)entityKey)
            {
                // no entity set, so no tracking is required for this entity, just
                // call the delegate to "materialize" it.
                result = constructEntityDelegate(this);
                RegisterMaterializedEntityForEvent(result);
            }
            else
            {
                Debug.Assert(null != entitySet, "if there is an entity key, there must also be an entity set");

                // check for an existing entity with the same key
                var existingEntry = Context.ObjectStateManager.FindEntityEntry(entityKey);
                if (null != existingEntry
                    && !existingEntry.IsKeyEntry)
                {
                    Debug.Assert(existingEntry.EntityKey.Equals(entityKey), "Found ObjectStateEntry with wrong EntityKey");
                    if (typeof(TEntity)
                        != existingEntry.WrappedEntity.IdentityType)
                    {
                        var key = existingEntry.EntityKey;
                        throw new NotSupportedException(
                            Strings.Materializer_RecyclingEntity(
                                TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName), typeof(TEntity).FullName,
                                existingEntry.WrappedEntity.IdentityType.FullName));
                    }

                    if (EntityState.Added
                        == existingEntry.State)
                    {
                        throw new InvalidOperationException(
                            Strings.Materializer_AddedEntityAlreadyExists(typeof(TEntity).FullName));
                    }
                    result = existingEntry.WrappedEntity;
                }
                else
                {
                    // We don't already have the entity, so construct it
                    result = constructEntityDelegate(this);
                    RegisterMaterializedEntityForEvent(result);
                    if (null == existingEntry)
                    {
                        Context.ObjectStateManager.AddEntry(result, entityKey, entitySet, "HandleEntity", false);
                    }
                    else
                    {
                        Context.ObjectStateManager.PromoteKeyEntry(
                            existingEntry, result, false, /*setIsLoaded*/ true, /*keyEntryInitialized*/ false);
                    }
                }
            }
            return result;
        }

        // <summary>
        // Call to ensure a collection of full-spanned elements are added
        // into the state manager properly.  We registers an action to be called
        // when the collection is closed that pulls the collection of full spanned
        // objects into the state manager.
        // </summary>
        public IEntityWrapper HandleFullSpanCollection<TTargetEntity>(
            IEntityWrapper wrappedEntity, Coordinator<TTargetEntity> coordinator, AssociationEndMember targetMember)
        {
            DebugCheck.NotNull(wrappedEntity);
            if (null != wrappedEntity.Entity)
            {
                coordinator.RegisterCloseHandler((state, spannedEntities) => FullSpanAction(wrappedEntity, spannedEntities, targetMember));
            }
            return wrappedEntity;
        }

        // <summary>
        // Call to ensure a single full-spanned element is added into
        // the state manager properly.
        // </summary>
        public IEntityWrapper HandleFullSpanElement(
            IEntityWrapper wrappedSource, IEntityWrapper wrappedSpannedEntity, AssociationEndMember targetMember)
        {
            DebugCheck.NotNull(wrappedSource);
            if (wrappedSource.Entity == null)
            {
                return wrappedSource;
            }
            List<IEntityWrapper> spannedEntities = null;
            if (wrappedSpannedEntity.Entity != null)
            {
                // There was a single entity in the column
                // Create a list so we can perform the same logic as a collection of entities
                spannedEntities = new List<IEntityWrapper>(1);
                spannedEntities.Add(wrappedSpannedEntity);
            }
            else
            {
                var sourceKey = wrappedSource.EntityKey;
                CheckClearedEntryOnSpan(null, wrappedSource, sourceKey, targetMember);
            }
            FullSpanAction(wrappedSource, spannedEntities, targetMember);
            return wrappedSource;
        }

        // <summary>
        // Call to ensure a target entities key is added into the state manager
        // properly
        // </summary>
        public IEntityWrapper HandleRelationshipSpan(
            IEntityWrapper wrappedEntity, EntityKey targetKey, AssociationEndMember targetMember)
        {
            if (null == wrappedEntity.Entity)
            {
                return wrappedEntity;
            }
            DebugCheck.NotNull(targetMember);
            Debug.Assert(
                targetMember.RelationshipMultiplicity == RelationshipMultiplicity.One ||
                targetMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne);

            var sourceKey = wrappedEntity.EntityKey;
            var sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);
            CheckClearedEntryOnSpan(targetKey, wrappedEntity, sourceKey, targetMember);

            if (null != (object)targetKey)
            {
                EntitySet targetEntitySet;

                var associationSet = Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                    (AssociationType)targetMember.DeclaringType, targetMember.Name,
                    targetKey.EntitySetName, targetKey.EntityContainerName, out targetEntitySet);
                Debug.Assert(associationSet != null, "associationSet should not be null");

                var manager = Context.ObjectStateManager;
                EntityState newEntryState;
                // If there is an existing relationship entry, update it based on its current state and the MergeOption, otherwise add a new one            
                if (
                    !ObjectStateManager.TryUpdateExistingRelationships(
                        Context, MergeOption, associationSet, sourceMember, sourceKey, wrappedEntity, targetMember, targetKey,
                         /*setIsLoaded*/ true, out newEntryState))
                {
                    // Try to find a state entry for the target key
                    var targetEntry = manager.GetOrAddKeyEntry(targetKey, targetEntitySet);

                    // For 1-1 relationships we have to take care of the relationships of targetEntity
                    var needNewRelationship = true;
                    switch (sourceMember.RelationshipMultiplicity)
                    {
                        case RelationshipMultiplicity.ZeroOrOne:
                        case RelationshipMultiplicity.One:
                            // devnote: targetEntry can be a key entry (targetEntry.Entity == null), 
                            // but it that case this parameter won't be used in TryUpdateExistingRelationships
                            needNewRelationship = !ObjectStateManager.TryUpdateExistingRelationships(
                                Context,
                                MergeOption,
                                associationSet,
                                targetMember,
                                targetKey,
                                targetEntry.WrappedEntity,
                                sourceMember,
                                sourceKey,
                                setIsLoaded: true,
                                newEntryState: out newEntryState);

                            // It is possible that as part of removing existing relationships, the key entry was deleted
                            // If that is the case, recreate the key entry
                            if (targetEntry.State
                                == EntityState.Detached)
                            {
                                targetEntry = manager.AddKeyEntry(targetKey, targetEntitySet);
                            }
                            break;
                        case RelationshipMultiplicity.Many:
                            // we always need a new relationship with Many-To-Many, if there was no exact match between these two entities, so do nothing                                
                            break;
                        default:
                            Debug.Assert(false, "Unexpected sourceMember.RelationshipMultiplicity");
                            break;
                    }

                    if (needNewRelationship)
                    {
                        // If the target entry is a key entry, then we need to add a relation 
                        //   between the source and target entries
                        // If we are in a state where we just need to add a new Deleted relation, we
                        //   only need to do that and not touch the related ends
                        // If the target entry is a full entity entry, then we need to add 
                        //   the target entity to the source collection or reference
                        if (targetEntry.IsKeyEntry
                            || newEntryState == EntityState.Deleted)
                        {
                            // Add a relationship between the source entity and the target key entry
                            var wrapper = new RelationshipWrapper(
                                associationSet, sourceMember.Name, sourceKey, targetMember.Name, targetKey);
                            manager.AddNewRelation(wrapper, newEntryState);
                        }
                        else
                        {
                            Debug.Assert(!targetEntry.IsRelationship, "how IsRelationship?");
                            if (targetEntry.State
                                != EntityState.Deleted)
                            {
                                // The entry contains an entity, do collection or reference fixup
                                // This will also try to create a new relationship entry or will revert the delete on an existing deleted relationship
                                ObjectStateManager.AddEntityToCollectionOrReference(
                                    MergeOption, wrappedEntity, sourceMember,
                                    targetEntry.WrappedEntity,
                                    targetMember,
                                    setIsLoaded: true,
                                    relationshipAlreadyExists: false,
                                    inKeyEntryPromotion: false);
                            }
                            else
                            {
                                // if the target entry is deleted, then the materializer needs to create a deleted relationship
                                // between the entity and the target entry so that if the entity is deleted, the update
                                // pipeline can find the relationship (even though it is deleted)
                                var wrapper = new RelationshipWrapper(
                                    associationSet, sourceMember.Name, sourceKey, targetMember.Name, targetKey);
                                manager.AddNewRelation(wrapper, EntityState.Deleted);
                            }
                        }
                    }
                }
            }
            else
            {
                RelatedEnd relatedEnd;
                if (TryGetRelatedEnd(
                    wrappedEntity, (AssociationType)targetMember.DeclaringType, sourceMember.Name, targetMember.Name, out relatedEnd))
                {
                    SetIsLoadedForSpan(relatedEnd, false);
                }
            }

            // else there is nothing else for us to do, the relationship has been handled already   
            return wrappedEntity;
        }

        private bool TryGetRelatedEnd(
            IEntityWrapper wrappedEntity, AssociationType associationType, string sourceEndName, string targetEndName,
            out RelatedEnd relatedEnd)
        {
            Debug.Assert(associationType.DataSpace == DataSpace.CSpace);

            // Get the OSpace AssociationType
            var oSpaceAssociation = Workspace.MetadataOptimization.GetOSpaceAssociationType(associationType,
                () => Workspace.GetItemCollection(DataSpace.OSpace).GetItem<AssociationType>(associationType.FullName));

            AssociationEndMember sourceEnd = null;
            AssociationEndMember targetEnd = null;
            foreach (var end in oSpaceAssociation.AssociationEndMembers)
            {
                if (end.Name == sourceEndName)
                {
                    sourceEnd = end;
                }
                else if (end.Name == targetEndName)
                {
                    targetEnd = end;
                }
            }

            if (sourceEnd != null
                && targetEnd != null)
            {
                var createRelatedEnd = false;
                if (wrappedEntity.EntityKey == null)
                {
                    // Free-floating entity--key is null, so don't have EntitySet for validation, so always create RelatedEnd
                    createRelatedEnd = true;
                }
                else
                {
                    // It is possible, because of MEST, that we're trying to load a relationship that is valid for this EntityType
                    // in metadata, but is not valid in this case because the specific entity is part of an EntitySet that is not
                    // mapped in any AssociationSet for this association type.                  
                    // The metadata structure makes checking for this somewhat time consuming because of the loop required.
                    // Because the whole reason for this method is perf, we try to reduce the
                    // impact of this check by caching positive hits in a HashSet so we don't have to do this for
                    // every entity in a query.  (We could also cache misses, but since these only happen in MEST, which
                    // is not common, we decided not to slow down the normal non-MEST case anymore by doing this.)
                    EntitySet entitySet;
                    var associationSet = Workspace.MetadataOptimization.FindCSpaceAssociationSet(associationType, sourceEndName,
                        wrappedEntity.EntityKey.EntitySetName, wrappedEntity.EntityKey.EntityContainerName, out entitySet);
                    if (associationSet != null)
                    {
                        createRelatedEnd = true;
                    }
                }
                if (createRelatedEnd)
                {
                    relatedEnd = DelegateFactory.GetRelatedEnd(wrappedEntity.RelationshipManager, sourceEnd, targetEnd, null);
                    return true;
                }
            }

            relatedEnd = null;
            return false;
        }

        // <summary>
        // Sets the IsLoaded flag to "true"
        // There are also rules for when this can be set based on MergeOption and the current value(s) in the related end.
        // </summary>
        private void SetIsLoadedForSpan(RelatedEnd relatedEnd, bool forceToTrue)
        {
            DebugCheck.NotNull(relatedEnd);

            // We can now say this related end is "Loaded" 
            // The cases where we should set this to true are:
            // AppendOnly: the related end is empty and does not point to a stub
            // PreserveChanges: the related end is empty and does not point to a stub (otherwise, an Added item exists and IsLoaded should not change)
            // OverwriteChanges: always
            // NoTracking: always
            if (!forceToTrue)
            {
                // Detect the empty value state of the relatedEnd
                forceToTrue = relatedEnd.IsEmpty();
                var reference = relatedEnd as EntityReference;
                if (reference != null)
                {
                    forceToTrue &= reference.EntityKey == null;
                }
            }
            if (forceToTrue || MergeOption == MergeOption.OverwriteChanges)
            {
                relatedEnd.IsLoaded = true;
            }
        }

        // <summary>
        // REQUIRES:: entity is not null and MergeOption is OverwriteChanges or PreserveChanges
        // Calls through to HandleEntity after retrieving the EntityKey from the given entity.
        // Still need this so that the correct key will be used for iPOCOs that implement IEntityWithKey
        // in a non-default manner.
        // </summary>
        public IEntityWrapper HandleIEntityWithKey<TEntity>(IEntityWrapper wrappedEntity, EntitySet entitySet)
        {
            DebugCheck.NotNull(wrappedEntity);
            return HandleEntity<TEntity>(wrappedEntity, wrappedEntity.EntityKey, entitySet);
        }

        // <summary>
        // Calls through to the specified RecordState to set the value for the specified column ordinal.
        // </summary>
        public bool SetColumnValue(int recordStateSlotNumber, int ordinal, object value)
        {
            var recordState = (RecordState)State[recordStateSlotNumber];
            recordState.SetColumnValue(ordinal, value);
            return true; // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        // <summary>
        // Calls through to the specified RecordState to set the value for the EntityRecordInfo.
        // </summary>
        public bool SetEntityRecordInfo(int recordStateSlotNumber, EntityKey entityKey, EntitySet entitySet)
        {
            var recordState = (RecordState)State[recordStateSlotNumber];
            recordState.SetEntityRecordInfo(entityKey, entitySet);
            return true; // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        // <summary>
        // REQUIRES:: should be called only by delegate allocating this state.
        // Utility method assigning a value to a state slot. Returns an arbitrary value
        // allowing the method call to be composed in a ShapeEmitter Expression delegate.
        // </summary>
        public bool SetState<T>(int ordinal, T value)
        {
            State[ordinal] = value;
            return true; // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        // <summary>
        // REQUIRES:: should be called only by delegate allocating this state.
        // Utility method assigning a value to a state slot and return the value, allowing
        // the value to be accessed/set in a ShapeEmitter Expression delegate and later
        // retrieved.
        // </summary>
        public T SetStatePassthrough<T>(int ordinal, T value)
        {
            State[ordinal] = value;
            return value;
        }

        // <summary>
        // Used to retrieve a property value with exception handling. Normally compiled
        // delegates directly call typed methods on the DbDataReader (e.g. GetInt32)
        // but when an exception occurs we retry using this method to potentially get
        // a more useful error message to the user.
        // </summary>
        public TProperty GetPropertyValueWithErrorHandling<TProperty>(int ordinal, string propertyName, string typeName)
        {
            var result = new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName).GetValue(Reader, ordinal);
            return result;
        }

        // <summary>
        // Used to retrieve a column value with exception handling. Normally compiled
        // delegates directly call typed methods on the DbDataReader (e.g. GetInt32)
        // but when an exception occurs we retry using this method to potentially get
        // a more useful error message to the user.
        // </summary>
        public TColumn GetColumnValueWithErrorHandling<TColumn>(int ordinal)
        {
            var result = new ColumnErrorHandlingValueReader<TColumn>().GetValue(Reader, ordinal);
            return result;
        }

        protected virtual DbSpatialDataReader CreateSpatialDataReader()
        {
            return SpatialHelpers.CreateSpatialDataReader(Workspace, Reader);
        }

        private readonly Lazy<DbSpatialDataReader> _spatialReader;

        public DbGeography GetGeographyColumnValue(int ordinal)
        {
            if (Streaming)
            {
                return _spatialReader.Value.GetGeography(ordinal);
            }
            else
            {
                return (DbGeography)Reader.GetValue(ordinal);
            }
        }

        public DbGeometry GetGeometryColumnValue(int ordinal)
        {
            if (Streaming)
            {
                return _spatialReader.Value.GetGeometry(ordinal);
            }
            else
            {
                return (DbGeometry)Reader.GetValue(ordinal);
            }
        }

        public TColumn GetSpatialColumnValueWithErrorHandling<TColumn>(int ordinal, PrimitiveTypeKind spatialTypeKind)
        {
            Debug.Assert(
                spatialTypeKind == PrimitiveTypeKind.Geography || spatialTypeKind == PrimitiveTypeKind.Geometry,
                "Spatial primitive type kind is not geography or geometry?");

            TColumn result;
            if (spatialTypeKind == PrimitiveTypeKind.Geography)
            {
                if (Streaming)
                {
                    result = new ColumnErrorHandlingValueReader<TColumn>(
                        (reader, column) => (TColumn)(object)_spatialReader.Value.GetGeography(column),
                        (reader, column) => _spatialReader.Value.GetGeography(column)
                        ).GetValue(Reader, ordinal);
                }
                else
                {
                    result = new ColumnErrorHandlingValueReader<TColumn>(
                        (reader, column) => (TColumn)Reader.GetValue(column),
                        (reader, column) => Reader.GetValue(column)
                        ).GetValue(Reader, ordinal);
                }
            }
            else
            {
                if (Streaming)
                {
                    result = new ColumnErrorHandlingValueReader<TColumn>(
                        (reader, column) => (TColumn)(object)_spatialReader.Value.GetGeometry(column),
                        (reader, column) => _spatialReader.Value.GetGeometry(column)
                        ).GetValue(Reader, ordinal);
                }
                else
                {
                    result = new ColumnErrorHandlingValueReader<TColumn>(
                        (reader, column) => (TColumn)Reader.GetValue(column),
                        (reader, column) => Reader.GetValue(column)
                        ).GetValue(Reader, ordinal);
                }
            }
            return result;
        }

        public TProperty GetSpatialPropertyValueWithErrorHandling<TProperty>(
            int ordinal, string propertyName, string typeName, PrimitiveTypeKind spatialTypeKind)
        {
            TProperty result;
            if (Helper.IsGeographicTypeKind(spatialTypeKind))
            {
                if (Streaming)
                {
                    result = new PropertyErrorHandlingValueReader<TProperty>(
                        propertyName, typeName,
                        (reader, column) => (TProperty)(object)_spatialReader.Value.GetGeography(column),
                        (reader, column) => _spatialReader.Value.GetGeography(column)
                        ).GetValue(Reader, ordinal);
                }
                else
                {
                    result = new PropertyErrorHandlingValueReader<TProperty>(
                        propertyName, typeName,
                        (reader, column) => (TProperty)Reader.GetValue(column),
                        (reader, column) => Reader.GetValue(column)
                        ).GetValue(Reader, ordinal);
                }
            }
            else
            {
                if (Streaming)
                {
                    Debug.Assert(Helper.IsGeometricTypeKind(spatialTypeKind));
                    result = new PropertyErrorHandlingValueReader<TProperty>(
                        propertyName, typeName,
                        (reader, column) => (TProperty)(object)_spatialReader.Value.GetGeometry(column),
                        (reader, column) => _spatialReader.Value.GetGeometry(column)
                        ).GetValue(Reader, ordinal);
                }
                else
                {
                    Debug.Assert(Helper.IsGeometricTypeKind(spatialTypeKind));
                    result = new PropertyErrorHandlingValueReader<TProperty>(
                        propertyName, typeName,
                        (reader, column) => (TProperty)Reader.GetValue(column),
                        (reader, column) => Reader.GetValue(column)
                        ).GetValue(Reader, ordinal);
                }
            }

            return result;
        }

        #endregion

        #region helper methods (used by runtime callable code)

        private void CheckClearedEntryOnSpan(
            object targetValue, IEntityWrapper wrappedSource, EntityKey sourceKey, AssociationEndMember targetMember)
        {
            // If a relationship does not exist on the server but does exist on the client,
            // we may need to remove it, depending on the current state and the MergeOption
            if ((null != (object)sourceKey)
                && (null == targetValue)
                && (MergeOption == MergeOption.PreserveChanges ||
                    MergeOption == MergeOption.OverwriteChanges))
            {
                // When the spanned value is null, it may be because the spanned association applies to a
                // subtype of the entity's type, and the entity is not actually an instance of that type.
                var sourceEnd = MetadataHelper.GetOtherAssociationEnd(targetMember);
                EdmType expectedSourceType = ((RefType)sourceEnd.TypeUsage.EdmType).ElementType;
                TypeUsage entityTypeUsage;
                if (!Context.Perspective.TryGetType(wrappedSource.IdentityType, out entityTypeUsage)
                    || entityTypeUsage.EdmType.EdmEquals(expectedSourceType)
                    || TypeSemantics.IsSubTypeOf(entityTypeUsage.EdmType, expectedSourceType))
                {
                    // Otherwise, the source entity is the correct type (exactly or a subtype) for the source
                    // end of the spanned association, so validate that the relationhip that was spanned is
                    // part of the Container owning the EntitySet of the root entity.
                    // This can be done by comparing the EntitySet  of the row's entity to the relationships
                    // in the Container and their AssociationSetEnd's type
                    CheckClearedEntryOnSpan(sourceKey, targetMember);
                }
            }
        }

        private void CheckClearedEntryOnSpan(EntityKey sourceKey, AssociationEndMember targetMember)
        {
            DebugCheck.NotNull((object)sourceKey);
            DebugCheck.NotNull(targetMember);
            Debug.Assert(Context != null);

            var sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);

            EntitySet sourceEntitySet;
            var associationSet = Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                (AssociationType)sourceMember.DeclaringType, sourceMember.Name,
                sourceKey.EntitySetName, sourceKey.EntityContainerName, out sourceEntitySet);

            if (associationSet != null)
            {
                Debug.Assert(associationSet.AssociationSetEnds[sourceMember.Name].EntitySet == sourceEntitySet);
                Context.ObjectStateManager.RemoveRelationships(MergeOption, associationSet, sourceKey, sourceMember);
            }
        }

        // <summary>
        // Wire's one or more full-spanned entities into the state manager; used by
        // both full-spanned collections and full-spanned entities.
        // </summary>
        private void FullSpanAction<TTargetEntity>(
            IEntityWrapper wrappedSource, IList<TTargetEntity> spannedEntities, AssociationEndMember targetMember)
        {
            DebugCheck.NotNull(wrappedSource);

            if (wrappedSource.Entity != null)
            {
                var sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);

                RelatedEnd relatedEnd;
                if (TryGetRelatedEnd(
                    wrappedSource, (AssociationType)targetMember.DeclaringType, sourceMember.Name, targetMember.Name, out relatedEnd))
                {
                    // Add members of the list to the source entity (item in column 0)
                    var count = Context.ObjectStateManager.UpdateRelationships(
                        Context, MergeOption, (AssociationSet)relatedEnd.RelationshipSet, sourceMember, wrappedSource,
                        targetMember, (List<TTargetEntity>)spannedEntities, true);

                    SetIsLoadedForSpan(relatedEnd, count > 0);
                }
            }
        }

        #region update existing ObjectStateEntry

        private void UpdateEntry<TEntity>(IEntityWrapper wrappedEntity, EntityEntry existingEntry)
        {
            DebugCheck.NotNull(wrappedEntity);
            DebugCheck.NotNull(wrappedEntity.Entity);
            DebugCheck.NotNull(existingEntry);
            DebugCheck.NotNull(existingEntry.Entity);

            var clrType = typeof(TEntity);
            if (clrType != existingEntry.WrappedEntity.IdentityType)
            {
                var key = existingEntry.EntityKey;
                throw new NotSupportedException(
                    Strings.Materializer_RecyclingEntity(
                        TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName), clrType.FullName,
                        existingEntry.WrappedEntity.IdentityType.FullName));
            }

            if (EntityState.Added
                == existingEntry.State)
            {
                throw new InvalidOperationException(Strings.Materializer_AddedEntityAlreadyExists(clrType.FullName));
            }

            if (MergeOption.AppendOnly != MergeOption)
            {
                // existing entity, update CSpace values in place
                Debug.Assert(EntityState.Added != existingEntry.State, "entry in State=Added");
                Debug.Assert(EntityState.Detached != existingEntry.State, "entry in State=Detached");

                if (MergeOption.OverwriteChanges == MergeOption)
                {
                    if (EntityState.Deleted
                        == existingEntry.State)
                    {
                        existingEntry.RevertDelete();
                    }
                    existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
                    Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
                    existingEntry.AcceptChanges();
                    Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
                }
                else
                {
                    Debug.Assert(MergeOption.PreserveChanges == MergeOption, "not MergeOption.PreserveChanges");
                    if (EntityState.Unchanged
                        == existingEntry.State)
                    {
                        // same behavior as MergeOption.OverwriteChanges
                        existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
                        Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
                        existingEntry.AcceptChanges();
                        Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
                    }
                    else
                    {
                        if (Context.ContextOptions.UseLegacyPreserveChangesBehavior)
                        {
                            // Do not mark properties as modified if they differ from the entity.
                            existingEntry.UpdateRecordWithoutSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
                        }
                        else
                        {
                            // Mark properties as modified if they differ from the entity
                            existingEntry.UpdateRecordWithSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region nested types

        internal abstract class ErrorHandlingValueReader<T>
        {
            private readonly Func<DbDataReader, int, T> getTypedValue;
            private readonly Func<DbDataReader, int, object> getUntypedValue;

            protected ErrorHandlingValueReader(
                Func<DbDataReader, int, T> typedValueAccessor, Func<DbDataReader, int, object> untypedValueAccessor)
            {
                getTypedValue = typedValueAccessor;
                getUntypedValue = untypedValueAccessor;
            }

            protected ErrorHandlingValueReader()
                : this(GetTypedValueDefault, GetUntypedValueDefault)
            {
            }

            private static T GetTypedValueDefault(DbDataReader reader, int ordinal)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                // The value read from the reader is of a primitive type. Such a value cannot be cast to a nullable enum type directly
                // but first needs to be cast to the non-nullable enum type. Therefore we will call this method for non-nullable
                // underlying enum type and cast to the target type. 
                if (underlyingType != null
                    && underlyingType.IsEnum())
                {
                    var methodInfo = GetGenericTypedValueDefaultMethod(underlyingType);
                    return (T)methodInfo.Invoke(null, new object[] { reader, ordinal });
                }

                // use the specific reader.GetXXX method
                bool isNullable;
                var readerMethod = CodeGenEmitter.GetReaderMethod(typeof(T), out isNullable);
                var result = (T)readerMethod.Invoke(reader, new object[] { ordinal });
                return result;
            }

            public static MethodInfo GetGenericTypedValueDefaultMethod(Type underlyingType)
            {
                return typeof(ErrorHandlingValueReader<>).MakeGenericType(underlyingType).GetOnlyDeclaredMethod("GetTypedValueDefault");
            }

            private static object GetUntypedValueDefault(DbDataReader reader, int ordinal)
            {
                return reader.GetValue(ordinal);
            }

            // <summary>
            // Gets value from reader using the same pattern as the materializer delegate. Avoids
            // the need to compile multiple delegates for error handling. If there is a failure
            // reading a value
            // </summary>
            internal T GetValue(DbDataReader reader, int ordinal)
            {
                T result;
                if (reader.IsDBNull(ordinal))
                {
                    try
                    {
                        result = (T)(object)null;
                    }
                    catch (NullReferenceException)
                    {
                        // NullReferenceException is thrown when casting null to a value type.
                        // We don't use isNullable here because of an issue with GetReaderMethod
                        // CONSIDER:: is GetReaderMethod doing what it needs?
                        throw CreateNullValueException();
                    }
                }
                else
                {
                    try
                    {
                        result = getTypedValue(reader, ordinal);
                    }
                    catch (Exception e)
                    {
                        if (e.IsCatchableExceptionType())
                        {
                            // determine if the problem is with the result type
                            // (note that if we throw on this call, it's ok
                            // for it to percolate up -- we only intercept type
                            // and null mismatches)
                            var untypedResult = getUntypedValue(reader, ordinal);
                            var resultType = null == untypedResult ? null : untypedResult.GetType();
                            if (!typeof(T).IsAssignableFrom(resultType))
                            {
                                throw CreateWrongTypeException(resultType);
                            }
                        }
                        throw;
                    }
                }
                return result;
            }

            // <summary>
            // Creates the exception thrown when the reader returns a null value
            // for a non nullable property/column.
            // </summary>
            protected abstract Exception CreateNullValueException();

            // <summary>
            // Creates the exception thrown when the reader returns a value with
            // an incompatible type.
            // </summary>
            protected abstract Exception CreateWrongTypeException(Type resultType);
        }

        private class ColumnErrorHandlingValueReader<TColumn> : ErrorHandlingValueReader<TColumn>
        {
            internal ColumnErrorHandlingValueReader()
            {
            }

            internal ColumnErrorHandlingValueReader(
                Func<DbDataReader, int, TColumn> typedAccessor, Func<DbDataReader, int, object> untypedAccessor)
                : base(typedAccessor, untypedAccessor)
            {
            }

            protected override Exception CreateNullValueException()
            {
                return new InvalidOperationException(Strings.Materializer_NullReferenceCast(typeof(TColumn)));
            }

            protected override Exception CreateWrongTypeException(Type resultType)
            {
                return EntityUtil.ValueInvalidCast(resultType, typeof(TColumn));
            }
        }

        private class PropertyErrorHandlingValueReader<TProperty> : ErrorHandlingValueReader<TProperty>
        {
            private readonly string _propertyName;
            private readonly string _typeName;

            internal PropertyErrorHandlingValueReader(string propertyName, string typeName)
            {
                _propertyName = propertyName;
                _typeName = typeName;
            }

            internal PropertyErrorHandlingValueReader(
                string propertyName, string typeName, Func<DbDataReader, int, TProperty> typedAccessor,
                Func<DbDataReader, int, object> untypedAccessor)
                : base(typedAccessor, untypedAccessor)
            {
                _propertyName = propertyName;
                _typeName = typeName;
            }

            protected override Exception CreateNullValueException()
            {
                return new ConstraintException(
                    Strings.Materializer_SetInvalidValue(
                        Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty),
                        _typeName, _propertyName, "null"));
            }

            protected override Exception CreateWrongTypeException(Type resultType)
            {
                return new InvalidOperationException(
                    Strings.Materializer_SetInvalidValue(
                        Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty),
                        _typeName, _propertyName, resultType));
            }
        }

        #endregion

        #region OnMaterialized helpers

        public void RaiseMaterializedEvents()
        {
            if (_materializedEntities != null)
            {
                foreach (var wrappedEntity in _materializedEntities)
                {
                    Context.OnObjectMaterialized(wrappedEntity.Entity);
                }
                _materializedEntities.Clear();
            }
        }

        public void InitializeForOnMaterialize()
        {
            if (Context.OnMaterializedHasHandlers)
            {
                if (_materializedEntities == null)
                {
                    _materializedEntities = new List<IEntityWrapper>();
                }
            }
            else if (_materializedEntities != null)
            {
                _materializedEntities = null;
            }
        }

        protected void RegisterMaterializedEntityForEvent(IEntityWrapper wrappedEntity)
        {
            if (_materializedEntities != null)
            {
                _materializedEntities.Add(wrappedEntity);
            }
        }

        #endregion
    }
}
