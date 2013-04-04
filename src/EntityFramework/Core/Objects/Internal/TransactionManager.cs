// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Diagnostics;

    internal class TransactionManager
    {
        #region Properties

        // Dictionary used to recovery after exception in ObjectContext.AttachTo()
        internal Dictionary<RelatedEnd, IList<IEntityWrapper>> PromotedRelationships { get; private set; }

        // Dictionary used to recovery after exception in ObjectContext.AttachTo()
        internal Dictionary<object, EntityEntry> PromotedKeyEntries { get; private set; }

        // HashSet used to recover after exception in ObjectContext.Add and related methods
        internal HashSet<EntityReference> PopulatedEntityReferences { get; private set; }

        // HashSet used to recover after exception in ObjectContext.Add and related methods
        internal HashSet<EntityReference> AlignedEntityReferences { get; private set; }

        // Used in recovery after exception in ObjectContext.AttachTo()
        private MergeOption? _originalMergeOption;

        internal MergeOption? OriginalMergeOption
        {
            get
            {
                Debug.Assert(_originalMergeOption != null, "OriginalMergeOption used before being initialized");
                return _originalMergeOption;
            }
            set { _originalMergeOption = value; }
        }

        // Dictionary used to recovery after exception in ObjectContext.AttachTo() and ObjectContext.AddObject()
        internal HashSet<IEntityWrapper> ProcessedEntities { get; private set; }

        // Used in Add/Attach/DetectChanges
        internal Dictionary<object, IEntityWrapper> WrappedEntities { get; private set; }

        // Used in Add/Attach/DetectChanges
        internal bool TrackProcessedEntities { get; private set; }

        internal bool IsAddTracking { get; private set; }

        internal bool IsAttachTracking { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> AddedRelationshipsByGraph { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> DeletedRelationshipsByGraph { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByForeignKey { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByPrincipalKey { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> DeletedRelationshipsByForeignKey { get; private set; }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, HashSet<RelatedEnd>> ChangedForeignKeys { get; private set; }

        internal bool IsDetectChanges { get; private set; }

        internal bool IsAlignChanges { get; private set; }

        internal bool IsLocalPublicAPI { get; private set; }

        internal bool IsOriginalValuesGetter { get; private set; }

        internal bool IsForeignKeyUpdate { get; private set; }

        internal bool IsRelatedEndAdd { get; private set; }

        private int _graphUpdateCount;

        internal bool IsGraphUpdate
        {
            get { return _graphUpdateCount != 0; }
        }

        internal object EntityBeingReparented { get; set; }

        internal bool IsDetaching { get; private set; }

        internal EntityReference RelationshipBeingUpdated { get; private set; }

        internal bool IsFixupByReference { get; private set; }

        #endregion Properties

        #region Methods

        // Methods and properties used by recovery code in ObjectContext.AddObject()
        internal void BeginAddTracking()
        {
            Debug.Assert(!IsAddTracking);
            Debug.Assert(PopulatedEntityReferences == null, "Expected promotion index to be null when begining tracking.");
            Debug.Assert(AlignedEntityReferences == null, "Expected promotion index to be null when begining tracking.");
            IsAddTracking = true;
            PopulatedEntityReferences = new HashSet<EntityReference>();
            AlignedEntityReferences = new HashSet<EntityReference>();
            PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();

            // BeginAddTracking can be called in the middle of DetectChanges.  In this case the following flags and dictionaries should not be changed here.
            if (!IsDetectChanges)
            {
                TrackProcessedEntities = true;
                ProcessedEntities = new HashSet<IEntityWrapper>();
                WrappedEntities = new Dictionary<object, IEntityWrapper>(new ObjectReferenceEqualityComparer());
            }
        }

        internal void EndAddTracking()
        {
            Debug.Assert(IsAddTracking);
            IsAddTracking = false;
            PopulatedEntityReferences = null;
            AlignedEntityReferences = null;
            PromotedRelationships = null;

            // Clear flags/dictionaries only if we are not in the iddle of DetectChanges.
            if (!IsDetectChanges)
            {
                TrackProcessedEntities = false;

                ProcessedEntities = null;
                WrappedEntities = null;
            }
        }

        // Methods and properties used by recovery code in ObjectContext.AttachTo()
        internal void BeginAttachTracking()
        {
            Debug.Assert(!IsAttachTracking);
            IsAttachTracking = true;

            PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();
            PromotedKeyEntries = new Dictionary<object, EntityEntry>(new ObjectReferenceEqualityComparer());
            PopulatedEntityReferences = new HashSet<EntityReference>();
            AlignedEntityReferences = new HashSet<EntityReference>();

            TrackProcessedEntities = true;
            ProcessedEntities = new HashSet<IEntityWrapper>();
            WrappedEntities = new Dictionary<object, IEntityWrapper>(new ObjectReferenceEqualityComparer());

            OriginalMergeOption = null; // this must be set explicitely to value!=null later when the merge option is known
        }

        internal void EndAttachTracking()
        {
            Debug.Assert(IsAttachTracking);
            IsAttachTracking = false;

            PromotedRelationships = null;
            PromotedKeyEntries = null;
            PopulatedEntityReferences = null;
            AlignedEntityReferences = null;

            TrackProcessedEntities = false;

            ProcessedEntities = null;
            WrappedEntities = null;

            OriginalMergeOption = null;
        }

        // This method should be called only when there is entity in OSM which doesn't implement IEntityWithRelationships
        internal bool BeginDetectChanges()
        {
            if (IsDetectChanges)
            {
                return false;
            }
            IsDetectChanges = true;

            TrackProcessedEntities = true;

            ProcessedEntities = new HashSet<IEntityWrapper>();
            WrappedEntities = new Dictionary<object, IEntityWrapper>(new ObjectReferenceEqualityComparer());

            DeletedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
            AddedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
            DeletedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            AddedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            AddedRelationshipsByPrincipalKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            ChangedForeignKeys = new Dictionary<IEntityWrapper, HashSet<RelatedEnd>>();
            return true;
        }

        internal void EndDetectChanges()
        {
            Debug.Assert(IsDetectChanges);
            IsDetectChanges = false;

            TrackProcessedEntities = false;

            ProcessedEntities = null;
            WrappedEntities = null;

            DeletedRelationshipsByGraph = null;
            AddedRelationshipsByGraph = null;
            DeletedRelationshipsByForeignKey = null;
            AddedRelationshipsByForeignKey = null;
            AddedRelationshipsByPrincipalKey = null;
            ChangedForeignKeys = null;
        }

        internal void BeginAlignChanges()
        {
            IsAlignChanges = true;
        }

        internal void EndAlignChanges()
        {
            IsAlignChanges = false;
        }

        internal void ResetProcessedEntities()
        {
            Debug.Assert(ProcessedEntities != null, "ProcessedEntities should not be null");
            ProcessedEntities.Clear();
        }

        internal void BeginLocalPublicAPI()
        {
            Debug.Assert(!IsLocalPublicAPI);

            IsLocalPublicAPI = true;
        }

        internal void EndLocalPublicAPI()
        {
            Debug.Assert(IsLocalPublicAPI);

            IsLocalPublicAPI = false;
        }

        internal void BeginOriginalValuesGetter()
        {
            Debug.Assert(!IsOriginalValuesGetter);

            IsOriginalValuesGetter = true;
        }

        internal void EndOriginalValuesGetter()
        {
            Debug.Assert(IsOriginalValuesGetter);

            IsOriginalValuesGetter = false;
        }

        internal void BeginForeignKeyUpdate(EntityReference relationship)
        {
            Debug.Assert(!IsForeignKeyUpdate);

            RelationshipBeingUpdated = relationship;
            IsForeignKeyUpdate = true;
        }

        internal void EndForeignKeyUpdate()
        {
            Debug.Assert(IsForeignKeyUpdate);

            RelationshipBeingUpdated = null;
            IsForeignKeyUpdate = false;
        }

        internal void BeginRelatedEndAdd()
        {
            Debug.Assert(!IsRelatedEndAdd);
            IsRelatedEndAdd = true;
        }

        internal void EndRelatedEndAdd()
        {
            Debug.Assert(IsRelatedEndAdd);
            IsRelatedEndAdd = false;
        }

        internal void BeginGraphUpdate()
        {
            _graphUpdateCount++;
        }

        internal void EndGraphUpdate()
        {
            Debug.Assert(_graphUpdateCount > 0);
            _graphUpdateCount--;
        }

        internal void BeginDetaching()
        {
            Debug.Assert(!IsDetaching);
            IsDetaching = true;
        }

        internal void EndDetaching()
        {
            Debug.Assert(IsDetaching);
            IsDetaching = false;
        }

        internal void BeginFixupKeysByReference()
        {
            Debug.Assert(!IsFixupByReference);
            IsFixupByReference = true;
        }

        internal void EndFixupKeysByReference()
        {
            Debug.Assert(IsFixupByReference);
            IsFixupByReference = false;
        }

        #endregion Methods
    }
}
