// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class ModificationCommandTreeGenerator
    {
        private readonly DbCompiledModel _compiledModel;
        private readonly DbConnection _connection;
        private readonly MetadataWorkspace _metadataWorkspace;

        private class TempDbContext : DbContext
        {
            [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public TempDbContext(DbCompiledModel model)
                : base(model)
            {
                InternalContext.InitializerDisabled = true;
            }

            [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public TempDbContext(DbConnection connection, DbCompiledModel model)
                : base(connection, model, false)
            {
                InternalContext.InitializerDisabled = true;
            }
        }

        public ModificationCommandTreeGenerator(DbModel model, DbConnection connection = null)
        {
            DebugCheck.NotNull(model);

            _compiledModel = model.Compile();
            _connection = connection;

            using (var context = CreateContext())
            {
                _metadataWorkspace
                    = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            }
        }

        private DbContext CreateContext()
        {
            return _connection == null
                ? new TempDbContext(_compiledModel)
                : new TempDbContext(_connection, _compiledModel);
        }

        public IEnumerable<DbInsertCommandTree> GenerateAssociationInsert(string associationIdentity)
        {
            DebugCheck.NotEmpty(associationIdentity);

            return GenerateAssociation<DbInsertCommandTree>(associationIdentity, EntityState.Added);
        }

        public IEnumerable<DbDeleteCommandTree> GenerateAssociationDelete(string associationIdentity)
        {
            DebugCheck.NotEmpty(associationIdentity);

            return GenerateAssociation<DbDeleteCommandTree>(associationIdentity, EntityState.Deleted);
        }

        private IEnumerable<TCommandTree> GenerateAssociation<TCommandTree>(string associationIdentity, EntityState state)
            where TCommandTree : DbCommandTree
        {
            DebugCheck.NotEmpty(associationIdentity);

            var associationType
                = _metadataWorkspace
                    .GetItem<AssociationType>(associationIdentity, DataSpace.CSpace);

            using (var context = CreateContext())
            {
                var sourceEntityType = associationType.SourceEnd.GetEntityType();
                var sourceEntity = InstantiateAndAttachEntity(sourceEntityType, context);

                var targetEntityType = associationType.TargetEnd.GetEntityType();
                var targetEntity
                    = sourceEntityType.GetRootType() == targetEntityType.GetRootType()
                        ? sourceEntity
                        : InstantiateAndAttachEntity(targetEntityType, context);

                var objectStateManager
                    = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .ObjectStateManager;

                objectStateManager
                    .ChangeRelationshipState(
                        sourceEntity,
                        targetEntity,
                        associationType.FullName,
                        associationType.TargetEnd.Name,
                        state == EntityState.Deleted ? state : EntityState.Added
                    );

                using (var commandTracer = new CommandTracer(context))
                {
                    context.SaveChanges();

                    foreach (var commandTree in commandTracer.CommandTrees)
                    {
                        yield return (TCommandTree)commandTree;
                    }
                }
            }
        }

        private object InstantiateAndAttachEntity(EntityType entityType, DbContext context)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(context);

            var clrType = entityType.GetClrType();
            var set = context.Set(clrType);

            var entity = InstantiateEntity(entityType, context, clrType, set);

            SetFakeReferenceKeyValues(entity, entityType);
            SetFakeKeyValues(entity, entityType);

            set.Attach(entity);
            
            return entity;
        }

        private object InstantiateEntity(EntityType entityType, DbContext context, Type clrType, DbSet set)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(context);
            DebugCheck.NotNull(clrType);
            DebugCheck.NotNull(set);

            object entity;

            if (!clrType.IsAbstract())
            {
                entity = set.Create();
            }
            else
            {
                var derivedEntityType
                    = _metadataWorkspace
                        .GetItems<EntityType>(DataSpace.CSpace)
                        .First(et => entityType.IsAncestorOf(et) && !et.Abstract);

                entity = context.Set(derivedEntityType.GetClrType()).Create();
            }

            InstantiateComplexProperties(entity, entityType.Properties);

            return entity;
        }

        public IEnumerable<DbModificationCommandTree> GenerateInsert(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate(entityIdentity, EntityState.Added);
        }

        public IEnumerable<DbModificationCommandTree> GenerateUpdate(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate(entityIdentity, EntityState.Modified);
        }

        public IEnumerable<DbModificationCommandTree> GenerateDelete(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate(entityIdentity, EntityState.Deleted);
        }

        private IEnumerable<DbModificationCommandTree> Generate(string entityIdentity, EntityState state)
        {
            DebugCheck.NotEmpty(entityIdentity);

            var entityType
                = _metadataWorkspace
                    .GetItem<EntityType>(entityIdentity, DataSpace.CSpace);

            using (var context = CreateContext())
            {
                var entity = InstantiateAndAttachEntity(entityType, context);

                if (state != EntityState.Deleted)
                {
                    // For deletes, we need to set the state
                    // _after_ dealing with IAs.
                    context.Entry(entity).State = state;
                }

                ChangeRelationshipStates(context, entityType, entity, state);
                
                if (state == EntityState.Deleted)
                {
                    context.Entry(entity).State = state;
                }

                HandleTableSplitting(context, entityType, entity, state);

                using (var commandTracer = new CommandTracer(context))
                {
                    ((IObjectContextAdapter)context).ObjectContext.SaveChanges(SaveOptions.None);

                    foreach (var commandTree in commandTracer.CommandTrees)
                    {
                        yield return (DbModificationCommandTree)commandTree;
                    }
                }
            }
        }

        private void ChangeRelationshipStates(DbContext context, EntityType entityType, object entity, EntityState state)
        {
            DebugCheck.NotNull(context);
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(entity);

            var objectStateManager
                = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .ObjectStateManager;

            var associationTypes
                = _metadataWorkspace
                    .GetItems<AssociationType>(DataSpace.CSpace)
                    .Where(
                        at => !at.IsForeignKey
                              && !at.IsManyToMany()
                              && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityType)
                                  || at.TargetEnd.GetEntityType().IsAssignableFrom(entityType)));

            foreach (var associationType in associationTypes)
            {
                AssociationEndMember principalEnd, dependentEnd;
                if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
                {
                    principalEnd = associationType.SourceEnd;
                    dependentEnd = associationType.TargetEnd;
                }

                if (dependentEnd.GetEntityType().IsAssignableFrom(entityType))
                {
                    var principalEntityType = principalEnd.GetEntityType();
                    var principalClrType = principalEntityType.GetClrType();
                    var set = context.Set(principalClrType);
                    var principalStub = set.Local.Cast<object>().SingleOrDefault();

                    if ((principalStub == null)
                        || (ReferenceEquals(entity, principalStub)
                            && state == EntityState.Added))
                    {
                        principalStub
                            = InstantiateEntity(principalEntityType, context, principalClrType, set);

                        SetFakeReferenceKeyValues(principalStub, principalEntityType);

                        set.Attach(principalStub);
                    }

                    if (principalEnd.IsRequired()
                        && state == EntityState.Modified)
                    {
                        // For updates with a required principal, 
                        // we need to fake delete the relationship first.

                        var principalStubForDelete
                            = InstantiateEntity(principalEntityType, context, principalClrType, set);

                        SetFakeKeyValues(principalStubForDelete, principalEntityType);

                        set.Attach(principalStubForDelete);

                        objectStateManager
                            .ChangeRelationshipState(
                                entity,
                                principalStubForDelete,
                                associationType.FullName,
                                principalEnd.Name,
                                EntityState.Deleted
                            );
                    }

                    objectStateManager
                        .ChangeRelationshipState(
                            entity,
                            principalStub,
                            associationType.FullName,
                            principalEnd.Name,
                            state == EntityState.Deleted ? state : EntityState.Added
                        );
                }
            }
        }

        private void HandleTableSplitting(DbContext context, EntityType entityType, object entity, EntityState state)
        {
            DebugCheck.NotNull(context);
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(entity);

            var associationTypes
                = _metadataWorkspace
                    .GetItems<AssociationType>(DataSpace.CSpace)
                    .Where(
                        at => at.IsForeignKey
                              && at.IsRequiredToRequired()
                              && !at.IsSelfReferencing()
                              && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityType)
                                  || at.TargetEnd.GetEntityType().IsAssignableFrom(entityType))
                              && _metadataWorkspace.GetItems<AssociationType>(DataSpace.SSpace)
                                  .All(fk => fk.Name != at.Name)); // no store FK == shared table

            foreach (var associationType in associationTypes)
            {
                AssociationEndMember principalEnd, dependentEnd;
                if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
                {
                    principalEnd = associationType.SourceEnd;
                    dependentEnd = associationType.TargetEnd;
                }

                EntityType otherEntityType;
                var entityTypeIsPrincipal = false;

                if (principalEnd.GetEntityType().GetRootType() == entityType.GetRootType())
                {
                    entityTypeIsPrincipal = true;
                    otherEntityType = dependentEnd.GetEntityType();
                }
                else
                {
                    otherEntityType = principalEnd.GetEntityType();
                }

                var otherEntity = InstantiateAndAttachEntity(otherEntityType, context);

                if (!entityTypeIsPrincipal)

                {
                    if (state == EntityState.Added)
                    {
                        // Rewrite dependent insert to update
                        context.Entry(entity).State = EntityState.Modified;
                    }
                    else if (state == EntityState.Deleted)
                    {
                        // Rewrite dependent delete to no-op
                        context.Entry(entity).State = EntityState.Unchanged;
                    }
                }
                else if (state != EntityState.Modified)
                {
                    context.Entry(otherEntity).State = state;
                }
            }
        }

        private static void SetFakeReferenceKeyValues(object entity, EntityType entityType)
        {
            DebugCheck.NotNull(entity);
            DebugCheck.NotNull(entityType);

            foreach (var property in entityType.KeyProperties)
            {
                var clrPropertyInfo = property.GetClrPropertyInfo();
                var value = GetFakeReferenceKeyValue(property.UnderlyingPrimitiveType.PrimitiveTypeKind);

                if (value != null)
                {
                    clrPropertyInfo.GetPropertyInfoForSet().SetValue(entity, value, null);
                }
            }
        }

        private static object GetFakeReferenceKeyValue(PrimitiveTypeKind primitiveTypeKind)
        {
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    return new byte[0];

                case PrimitiveTypeKind.String:
                    return "42";

                case PrimitiveTypeKind.Geometry:
                    return DefaultSpatialServices.Instance.GeometryFromText("POINT (4 2)");

                case PrimitiveTypeKind.Geography:
                    return DefaultSpatialServices.Instance.GeographyFromText("POINT (4 2)");
            }

            return null;
        }

        private static void SetFakeKeyValues(object entity, EntityType entityType)
        {
            DebugCheck.NotNull(entity);
            DebugCheck.NotNull(entityType);

            foreach (var property in entityType.KeyProperties)
            {
                var clrPropertyInfo = property.GetClrPropertyInfo();
                var value = GetFakeKeyValue(property.UnderlyingPrimitiveType.PrimitiveTypeKind);

                Debug.Assert(value != null);

                clrPropertyInfo.GetPropertyInfoForSet().SetValue(entity, value, null);
            }
        }

        private static object GetFakeKeyValue(PrimitiveTypeKind primitiveTypeKind)
        {
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    return new byte[] { 0x42 };

                case PrimitiveTypeKind.Boolean:
                    return true;

                case PrimitiveTypeKind.Byte:
                    return (byte)0x42;

                case PrimitiveTypeKind.DateTime:
                    return DateTime.Now;

                case PrimitiveTypeKind.Decimal:
                    return 42m;

                case PrimitiveTypeKind.Double:
                    return 42.0;

                case PrimitiveTypeKind.Guid:
                    return Guid.NewGuid();

                case PrimitiveTypeKind.Single:
                    return 42f;

                case PrimitiveTypeKind.SByte:
                    return (sbyte)42;

                case PrimitiveTypeKind.Int16:
                    return (short)42;

                case PrimitiveTypeKind.Int32:
                    return 42;

                case PrimitiveTypeKind.Int64:
                    return 42L;

                case PrimitiveTypeKind.String:
                    return "42'";

                case PrimitiveTypeKind.Time:
                    return TimeSpan.FromMilliseconds(42);

                case PrimitiveTypeKind.DateTimeOffset:
                    return DateTimeOffset.Now;

                case PrimitiveTypeKind.Geometry:
                    return DefaultSpatialServices.Instance.GeometryFromText("POINT (4 3)");

                case PrimitiveTypeKind.Geography:
                    return DefaultSpatialServices.Instance.GeographyFromText("POINT (4 3)");

                default:
                    Debug.Fail("Unexpected key PrimitiveTypeKind!");
                    break;
            }

            return null;
        }

        private static void InstantiateComplexProperties(object structuralObject, IEnumerable<EdmProperty> properties)
        {
            DebugCheck.NotNull(structuralObject);
            DebugCheck.NotNull(properties);

            foreach (var property in properties)
            {
                if (property.IsComplexType)
                {
                    var clrPropertyInfo = property.GetClrPropertyInfo();

                    var complexObject
                        = Activator.CreateInstance(clrPropertyInfo.PropertyType);

                    InstantiateComplexProperties(complexObject, property.ComplexType.Properties);

                    clrPropertyInfo.GetPropertyInfoForSet().SetValue(structuralObject, complexObject, null);
                }
            }
        }
    }
}
