// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class CodeGenEmitterTests
    {
        [Fact]
        public void Static_fields_are_initialized()
        {
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_BinaryEquals);
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_CheckedConvert);
            Assert.NotNull(CodeGenEmitter.CodeGenEmitter_Compile);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetValue);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetString);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt16);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt32);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetInt64);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetBoolean);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDecimal);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetFloat);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDouble);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetDateTime);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetGuid);
            Assert.NotNull(CodeGenEmitter.DbDataReader_GetByte);
            Assert.NotNull(CodeGenEmitter.DbDataReader_IsDBNull);
            Assert.NotNull(CodeGenEmitter.DBNull_Value);
            Assert.NotNull(CodeGenEmitter.EntityKey_ctor_SingleKey);
            Assert.NotNull(CodeGenEmitter.EntityKey_ctor_CompositeKey);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityProxyTypeInfo_SetEntityWrapper);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetPocoEntityKeyStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc);
            Assert.NotNull(CodeGenEmitter.EntityWrapperFactory_NullWrapper);
            Assert.NotNull(CodeGenEmitter.IEntityWrapper_Entity);
            Assert.NotNull(CodeGenEmitter.IEqualityComparerOfString_Equals);
            Assert.NotNull(CodeGenEmitter.MaterializedDataRecord_ctor);
            Assert.NotNull(CodeGenEmitter.RecordState_GatherData);
            Assert.NotNull(CodeGenEmitter.RecordState_SetNullRecord);
            Assert.NotNull(CodeGenEmitter.Shaper_Discriminate);
            Assert.NotNull(CodeGenEmitter.Shaper_GetPropertyValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetColumnValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetGeographyColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_GetGeometryColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_GetSpatialColumnValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_GetSpatialPropertyValueWithErrorHandling);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntity);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntityAppendOnly);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleEntityNoTracking);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleFullSpanCollection);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleFullSpanElement);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleIEntityWithKey);
            Assert.NotNull(CodeGenEmitter.Shaper_HandleRelationshipSpan);
            Assert.NotNull(CodeGenEmitter.Shaper_SetColumnValue);
            Assert.NotNull(CodeGenEmitter.Shaper_SetEntityRecordInfo);
            Assert.NotNull(CodeGenEmitter.Shaper_SetState);
            Assert.NotNull(CodeGenEmitter.Shaper_SetStatePassthrough);
            Assert.NotNull(CodeGenEmitter.Shaper_Parameter);
            Assert.NotNull(CodeGenEmitter.Shaper_Reader);
            Assert.NotNull(CodeGenEmitter.Shaper_Workspace);
            Assert.NotNull(CodeGenEmitter.Shaper_State);
            Assert.NotNull(CodeGenEmitter.Shaper_Context);
            Assert.NotNull(CodeGenEmitter.Shaper_Context_Options);
            Assert.NotNull(CodeGenEmitter.Shaper_ProxyCreationEnabled);
        }

        /// <summary>
        ///     Not really a unit test because of the complexity of setting up everything the materializer needs.
        /// </summary>
        [Fact]
        public void Materialized_entities_have_override_Equals_flag_set_appropriately()
        {
            using (var context = new EntityTypesContext())
            {
                var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;

                var wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.IPocos.First())).WrappedEntity;
                Assert.IsType<LightweightEntityWrapper<IPocoEntity>>(wrappedEntity);
                Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

                wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.IPocosWithEquals.First())).WrappedEntity;
                Assert.IsType<LightweightEntityWrapper<IPocoEntityWithEquals>>(wrappedEntity);
                Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);

                wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.WithRels.First())).WrappedEntity;
                Assert.IsType<EntityWrapperWithRelationships<EntityWithRelationships>>(wrappedEntity);
                Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

                wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.WithRelsAndEquals.First())).WrappedEntity;
                Assert.IsType<EntityWrapperWithRelationships<EntityWithRelationshipsAndEquals>>(wrappedEntity);
                Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);

                wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.Pocos.First())).WrappedEntity;
                Assert.IsType<EntityWrapperWithoutRelationships<PocoEntity>>(wrappedEntity);
                Assert.False(wrappedEntity.OverridesEqualsOrGetHashCode);

                wrappedEntity = ((EntityEntry)stateManager.GetObjectStateEntry(context.PocosWithEquals.First())).WrappedEntity;
                Assert.IsType<EntityWrapperWithoutRelationships<PocoEntityWithEquals>>(wrappedEntity);
                Assert.True(wrappedEntity.OverridesEqualsOrGetHashCode);
            }
        }

        public class EntityTypesInitializer : DropCreateDatabaseIfModelChanges<EntityTypesContext>
        {
            protected override void Seed(EntityTypesContext context)
            {
                context.IPocos.Add(new IPocoEntity());
                context.IPocosWithEquals.Add(new IPocoEntityWithEquals());
                context.WithRels.Add(new EntityWithRelationships());
                context.WithRelsAndEquals.Add(new EntityWithRelationshipsAndEquals());
                context.Pocos.Add(new PocoEntity());
                context.PocosWithEquals.Add(new PocoEntityWithEquals());
            }
        }

        public class EntityTypesContext : DbContext
        {
            static EntityTypesContext()
            {
                Database.SetInitializer(new EntityTypesInitializer());
            }

            public DbSet<IPocoEntity> IPocos { get; set; }
            public DbSet<IPocoEntityWithEquals> IPocosWithEquals { get; set; }
            public DbSet<EntityWithRelationships> WithRels { get; set; }
            public DbSet<EntityWithRelationshipsAndEquals> WithRelsAndEquals { get; set; }
            public DbSet<PocoEntity> Pocos { get; set; }
            public DbSet<PocoEntityWithEquals> PocosWithEquals { get; set; }
        }

        public class ChangeTracker : EntityObject
        {
        }

        public class IPocoEntity : IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
        {
            private readonly ChangeTracker _changeTracker = new ChangeTracker();
            private readonly RelationshipManager _relationshipManager;

            public IPocoEntity()
            {
                _relationshipManager = RelationshipManager.Create(this);
            }

            public int Id { get; set; }

            public RelationshipManager RelationshipManager
            {
                get { return _relationshipManager; }
            }

            public EntityKey EntityKey { get; set; }

            public void SetChangeTracker(IEntityChangeTracker changeTracker)
            {
                ((IEntityWithChangeTracker)_changeTracker).SetChangeTracker(changeTracker);
            }
        }

        public class IPocoEntityWithEquals : IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
        {
            private readonly ChangeTracker _changeTracker = new ChangeTracker();
            private readonly RelationshipManager _relationshipManager;

            public IPocoEntityWithEquals()
            {
                _relationshipManager = RelationshipManager.Create(this);
            }

            public int Id { get; set; }

            public RelationshipManager RelationshipManager
            {
                get { return _relationshipManager; }
            }

            public EntityKey EntityKey { get; set; }

            public void SetChangeTracker(IEntityChangeTracker changeTracker)
            {
                ((IEntityWithChangeTracker)_changeTracker).SetChangeTracker(changeTracker);
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public class EntityWithRelationships : IEntityWithRelationships
        {
            private readonly RelationshipManager _relationshipManager;

            public EntityWithRelationships()
            {
                _relationshipManager = RelationshipManager.Create(this);
            }

            public int Id { get; set; }

            public RelationshipManager RelationshipManager
            {
                get { return _relationshipManager; }
            }
        }

        public class EntityWithRelationshipsAndEquals : IEntityWithRelationships
        {
            private readonly RelationshipManager _relationshipManager;

            public EntityWithRelationshipsAndEquals()
            {
                _relationshipManager = RelationshipManager.Create(this);
            }

            public int Id { get; set; }

            public RelationshipManager RelationshipManager
            {
                get { return _relationshipManager; }
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public class PocoEntity
        {
            public int Id { get; set; }
        }

        public class PocoEntityWithEquals
        {
            public int Id { get; set; }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
