// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ModificationCommandTreeGenerator
    {
        private readonly DbCompiledModel _compiledModel;
        private readonly DbConnection _connection;
        private readonly MetadataWorkspace _metadataWorkspace;

        private class TempDbContext : DbContext
        {
            static TempDbContext()
            {
                Database.SetInitializer<TempDbContext>(null);
            }

            public TempDbContext(DbCompiledModel model)
                : base(model)
            {
            }

            public TempDbContext(DbConnection connection, DbCompiledModel model)
                : base(connection, model, false)
            {
            }
        }

        public ModificationCommandTreeGenerator(DbModel model, DbConnection connection = null)
        {
            DebugCheck.NotNull(model);

            _compiledModel = new DbCompiledModel(model);
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
                var sourceSet = context.Set(sourceEntityType.GetClrType());
                var sourceEntity = sourceSet.Create();

                InstantiateNullableKeys(sourceEntity, sourceEntityType);
                InstantiateComplexProperties(sourceEntity, sourceEntityType.Properties);

                sourceSet.Attach(sourceEntity);

                var targetEntityType = associationType.TargetEnd.GetEntityType();
                var targetSet = context.Set(targetEntityType.GetClrType());
                var targetEntity = targetSet.Create();

                InstantiateNullableKeys(targetEntity, targetEntityType);
                InstantiateComplexProperties(targetEntity, targetEntityType.Properties);

                targetSet.Attach(targetEntity);

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

        public IEnumerable<DbInsertCommandTree> GenerateInsert(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate<DbInsertCommandTree>(entityIdentity, EntityState.Added);
        }

        public IEnumerable<DbUpdateCommandTree> GenerateUpdate(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate<DbUpdateCommandTree>(entityIdentity, EntityState.Modified);
        }

        public IEnumerable<DbDeleteCommandTree> GenerateDelete(string entityIdentity)
        {
            DebugCheck.NotEmpty(entityIdentity);

            return Generate<DbDeleteCommandTree>(entityIdentity, EntityState.Deleted);
        }

        private IEnumerable<TCommandTree> Generate<TCommandTree>(string entityIdentity, EntityState state)
            where TCommandTree : DbCommandTree
        {
            DebugCheck.NotEmpty(entityIdentity);

            var entityType
                = _metadataWorkspace
                    .GetItem<EntityType>(entityIdentity, DataSpace.CSpace);

            using (var context = CreateContext())
            {
                var set = context.Set(entityType.GetClrType());
                var entity = set.Create();

                InstantiateNullableKeys(entity, entityType);
                InstantiateComplexProperties(entity, entityType.Properties);

                set.Attach(entity);

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

                using (var commandTracer = new CommandTracer(context))
                {
                    ((IObjectContextAdapter)context).ObjectContext.SaveChanges(SaveOptions.None);
                    
                    foreach (var commandTree in commandTracer.CommandTrees)
                    {
                        yield return (TCommandTree)commandTree;
                    }
                }
            }
        }

        private void ChangeRelationshipStates(
            DbContext context,
            EntityType entityType,
            object entity,
            EntityState state)
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
                              && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityType)
                                  || at.TargetEnd.GetEntityType().IsAssignableFrom(entityType)));

            foreach (var associationType in associationTypes)
            {
                if (associationType.IsManyToMany())
                {
                    continue;
                }

                AssociationEndMember principalEnd, dependentEnd;
                if (!associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd))
                {
                    principalEnd = associationType.SourceEnd;
                    dependentEnd = associationType.TargetEnd;
                }

                if (dependentEnd.GetEntityType().IsAssignableFrom(entityType))
                {
                    var principalClrType
                        = principalEnd.GetEntityType().GetClrType();

                    var set = context.Set(principalClrType);

                    var principalStub = set.Local.Cast<object>().SingleOrDefault();

                    if ((principalStub == null)
                        || (ReferenceEquals(entity, principalStub)
                            && state == EntityState.Added))
                    {
                        principalStub = set.Create();

                        InstantiateNullableKeys(principalStub, principalEnd.GetEntityType());

                        set.Attach(principalStub);
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

        private static void InstantiateNullableKeys(object entity, EntityType entityType)
        {
            DebugCheck.NotNull(entity);
            DebugCheck.NotNull(entityType);

            foreach (var property in entityType.KeyProperties)
            {
                var clrPropertyInfo = property.GetClrPropertyInfo();

                switch (property.PrimitiveType.PrimitiveTypeKind)
                {
                    case PrimitiveTypeKind.String:
                        clrPropertyInfo.SetValue(entity, "tmp", null);
                        break;

                    case PrimitiveTypeKind.Binary:
                        clrPropertyInfo.SetValue(entity, new byte[0], null);
                        break;
                }
            }
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

                    clrPropertyInfo.SetValue(structuralObject, complexObject, null);
                }
            }
        }
    }
}
