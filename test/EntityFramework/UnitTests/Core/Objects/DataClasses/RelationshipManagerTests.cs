// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Moq;
    using Xunit;

    public class RelationshipManagerTests : TestBase
    {
        public class GetRelationshipType : TestBase
        {
            [Fact]
            public void GetRelationshipType_can_get_a_relationship_type_for_a_tracked_entity()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    Assert.Equal(
                        "FullyVirtualPrin_Other",
                        manager.GetRelationshipType("System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other").Name);
                }
            }

            [Fact]
            public void GetRelationshipType_can_get_a_relationship_type_for_an_untracked_proxy()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    Assert.Equal(
                        "FullyVirtualPrin_Dependents",
                        manager.GetRelationshipType("System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents").Name);
                }
            }

            [Fact]
            public void GetRelationshipType_can_get_a_relationship_type_for_an_untracked_entity_when_relationship_has_been_used()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);
                    manager.GetRelatedEnd(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Source");

                    manager.WrappedOwner.Context = null;

                    Assert.Equal(
                        "FullyVirtualPrin_Other",
                        manager.GetRelationshipType("System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other").Name);
                }
            }

            [Fact]
            public void GetRelationshipType_uses_expensive_lookup_as_a_last_resort()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    var associationType = new Mock<AssociationType>("A", "B", false, DataSpace.OCSpace).Object;
                    mockLoader.Setup(
                        m => m.GetRelationshipTypeExpensiveWay(
                            typeof(NavVirtual), "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other"))
                              .Returns(associationType);
                    manager.SetExpensiveLoader(mockLoader.Object);

                    manager.WrappedOwner.DetachContext();

                    Assert.Same(
                        associationType,
                        manager.GetRelationshipType("System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other"));
                }
            }

            [Fact]
            public void GetRelationshipType_throws_if_none_of_the_lookups_succeed()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    manager.WrappedOwner.DetachContext();

                    Assert.Contains(
                        Strings.RelationshipManager_UnableToFindRelationshipTypeInMetadata(
                            "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other"),
                        Assert.Throws<ArgumentException>(
                            () => manager.GetRelationshipType("System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other")).Message);
                }
            }
        }

        public class PrependNamespaceToRelationshipName : TestBase
        {
            [Fact]
            public void PrependNamespaceToRelationshipName_can_expand_a_name_for_a_tracked_entity()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    Assert.Equal(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other",
                        manager.PrependNamespaceToRelationshipName("FullyVirtualPrin_Other"));
                }
            }

            [Fact]
            public void PrependNamespaceToRelationshipName_can_expand_a_name_for_an_untracked_proxy()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    Assert.Equal(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents",
                        manager.PrependNamespaceToRelationshipName("FullyVirtualPrin_Dependents"));
                }
            }

            [Fact]
            public void PrependNamespaceToRelationshipName_can_expand_a_name_for_an_untracked_entity_when_relationship_has_been_used()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);
                    manager.GetRelatedEnd(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Source");

                    manager.WrappedOwner.Context = null;

                    Assert.Equal(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other",
                        manager.PrependNamespaceToRelationshipName("FullyVirtualPrin_Other"));
                }
            }

            [Fact]
            public void PrependNamespaceToRelationshipName_can_expand_a_name_using_expensive_lookup_as_a_last_resort()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                    var manager = objectContext.ObjectStateManager.GetRelationshipManager(anEntity);
                    var oSpaceType = objectContext.MetadataWorkspace
                                                  .GetItemCollection(DataSpace.OSpace)
                                                  .GetItem<EdmType>(typeof(NavVirtual).FullName);

                    var edmTypes = new Dictionary<string, EdmType>
                        {
                            { typeof(NavVirtual).FullName, oSpaceType }
                        };

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    mockLoader.Setup(m => m.LoadTypesExpensiveWay(typeof(NavVirtual).Assembly()))
                              .Returns(edmTypes);
                    manager.SetExpensiveLoader(mockLoader.Object);

                    manager.WrappedOwner.DetachContext();
                    typeof(RelationshipManager).GetField("_relationships", BindingFlags.Instance | BindingFlags.NonPublic)
                                               .SetValue(manager, null);

                    Assert.Equal(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other",
                        manager.PrependNamespaceToRelationshipName("FullyVirtualPrin_Other"));
                }
            }

            [Fact]
            public void PrependNamespaceToRelationshipName_does_not_expand_a_name_if_none_of_the_lookups_succeed()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());

                    var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                    var manager = objectContext.ObjectStateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    manager.WrappedOwner.DetachContext();
                    typeof(RelationshipManager).GetField("_relationships", BindingFlags.Instance | BindingFlags.NonPublic)
                                               .SetValue(manager, null);

                    Assert.Equal(
                        "FullyVirtualPrin_Other",
                        manager.PrependNamespaceToRelationshipName("FullyVirtualPrin_Other"));
                }
            }
        }

        public class GetRelatedEnd_variants : TestBase
        {
            [Fact]
            public void GetRelatedEnd_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedEnd("FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedCollection_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedCollection<FullyVirtualDep>("FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedReference_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedReference<NavVirtual>("FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Target"));
            }

            private void Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                Func<RelationshipManager, IRelatedEnd> getRelatedEnd)
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    Assert.NotNull(getRelatedEnd(manager));

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void GetRelatedEnd_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedEnd("FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedCollection_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedCollection<FullyVirtualDep>("FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedReference_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedReference<NavVirtual>("FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Target"));
            }

            private void Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup(
                Func<RelationshipManager, IRelatedEnd> getRelatedEnd)
            {
                using (var context = new DummyContext())
                {
                    var anEntity = SerializeAndDeserialize(context.Principals.Create());
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    Assert.NotNull(getRelatedEnd(manager));

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void GetRelatedEnd_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedEnd(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedCollection_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedCollection<FullyVirtualDep>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedReference_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedReference<NavVirtual>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Target"));
            }

            private void Getting_a_RelatedEnd_for_serialized_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                Func<RelationshipManager, IRelatedEnd> getRelatedEnd)
            {
                using (var context = new DummyContext())
                {
                    var anEntity = SerializeAndDeserialize(context.Principals.Create());
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    Assert.NotNull(getRelatedEnd(manager));

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void GetRelatedEnd_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedEnd(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedCollection_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedCollection<FullyVirtualDep>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));
            }

            [Fact]
            public void GetRelatedReference_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup()
            {
                Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                    m => m.GetRelatedReference<NavVirtual>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Target"));
            }

            private void Getting_a_RelatedEnd_for_detached_change_tracking_proxy_with_qualified_name_does_not_require_expensive_lookup(
                Func<RelationshipManager, IRelatedEnd> getRelatedEnd)
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    Assert.NotNull(getRelatedEnd(manager));

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void GetRelatedEnd_for_attached_change_tracking_proxy_with_unqualified_name_does_not_require_expensive_lookup()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Attach(context.Principals.Create());
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    Assert.NotNull(manager.GetRelatedEnd("FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target"));

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void Creating_graphs_of_never_attached_change_tracking_proxies_does_not_use_expensive_lookup()
            {
                Creating_graphs_of_detached_change_tracking_proxies_does_not_use_expensive_lookup(
                    c => c.Principals.Create(),
                    c => c.Dependents.Create());
            }

            [Fact]
            public void Creating_graphs_of_attached_and_then_detached_change_tracking_proxies_does_not_use_expensive_lookup()
            {
                Creating_graphs_of_detached_change_tracking_proxies_does_not_use_expensive_lookup(
                    c => c.Principals.Attach(c.Principals.Create()),
                    c => c.Dependents.Attach(c.Dependents.Create()));
            }

            [Fact]
            public void Creating_graphs_of_serialized_change_tracking_proxies_does_not_use_expensive_lookup()
            {
                Creating_graphs_of_detached_change_tracking_proxies_does_not_use_expensive_lookup(
                    c => SerializeAndDeserialize(c.Principals.Create()),
                    c => SerializeAndDeserialize(c.Dependents.Create()));
            }

            private void Creating_graphs_of_detached_change_tracking_proxies_does_not_use_expensive_lookup(
                Func<DummyContext, FullyVirtualPrin> createPrincipal,
                Func<DummyContext, FullyVirtualDep> createDependent)
            {
                using (var context = new DummyContext())
                {
                    var principal = createPrincipal(context);
                    var dependent = createDependent(context);
                    Assert.IsAssignableFrom<IEntityWithRelationships>(principal);
                    Assert.IsAssignableFrom<IEntityWithRelationships>(dependent);
                    context.Entry(principal).State = EntityState.Detached;
                    context.Entry(dependent).State = EntityState.Detached;

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var principalManager = stateManager.GetRelationshipManager(principal);
                    var dependentManager = stateManager.GetRelationshipManager(dependent);

                    var mockPrincipalLoader = new Mock<ExpensiveOSpaceLoader>();
                    principalManager.SetExpensiveLoader(mockPrincipalLoader.Object);

                    var mockDependentLoader = new Mock<ExpensiveOSpaceLoader>();
                    dependentManager.SetExpensiveLoader(mockDependentLoader.Object);

                    principal.Dependents.Add(dependent);
                    dependent.Principal = principal;

                    mockPrincipalLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockDependentLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockPrincipalLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockDependentLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockPrincipalLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockDependentLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }
        }

        public class GetRelationshipManger : TestBase
        {
            [Fact]
            public void GetRelationshipManger_throws_for_detatched_POCO_entity()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(new NavVirtual());
                    context.Entry(anEntity).State = EntityState.Detached;

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;

                    Assert.Equal(
                        Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity,
                        Assert.Throws<InvalidOperationException>(() => stateManager.GetRelationshipManager(anEntity)).Message);
                }
            }

            [Fact]
            public void GetRelationshipManger_throws_for_detatched_lazy_loading_proxy()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.NavVirtuals.Attach(context.NavVirtuals.Create());
                    context.Entry(anEntity).State = EntityState.Detached;

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;

                    Assert.Equal(
                        Strings.ObjectStateManager_CannotGetRelationshipManagerForDetachedPocoEntity,
                        Assert.Throws<InvalidOperationException>(() => stateManager.GetRelationshipManager(anEntity)).Message);
                }
            }

            [Fact]
            public void GetRelationshipManger_does_not_throw_for_attached_and_then_detatched_change_tracking_proxy()
            {
                GetRelationshipManger_does_not_throw_for_detatched_change_tracking_proxy(
                    c => c.Dependents.Attach(c.Dependents.Create()));
            }

            [Fact]
            public void GetRelationshipManger_does_not_throw_for_never_attached_change_tracking_proxy()
            {
                GetRelationshipManger_does_not_throw_for_detatched_change_tracking_proxy(
                    c => c.Dependents.Create());
            }

            private void GetRelationshipManger_does_not_throw_for_detatched_change_tracking_proxy(
                Func<DummyContext, FullyVirtualDep> createEntity)
            {
                using (var context = new DummyContext())
                {
                    var anEntity = createEntity(context);
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);
                    context.Entry(anEntity).State = EntityState.Detached;

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;

                    Assert.NotNull(stateManager.GetRelationshipManager(anEntity));
                }
            }
        }

        public class InitializeRelatedReference : TestBase
        {
            [Fact]
            public void InitializeRelatedReference_throws_for_change_tracking_proxy_related_end_that_has_been_serialized()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = SerializeAndDeserialize(context.Principals.Create());
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    var relatedEnd = manager.GetRelatedReference<NavVirtual>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other", "FullyVirtualPrin_Other_Target");

                    Assert.Equal(
                        Strings.RelationshipManager_ReferenceAlreadyInitialized(Strings.RelationshipManager_InitializeIsForDeserialization),
                        Assert.Throws<InvalidOperationException>(
                            () => manager.InitializeRelatedReference(
                                "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Other",
                                "FullyVirtualPrin_Other_Target",
                                relatedEnd)).Message);
                }
            }
        }

        public class InitializeRelatedCollection : TestBase
        {
            [Fact]
            public void InitializeRelatedCollection_throws_for_change_tracking_proxy_related_end_that_has_been_serialized()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = SerializeAndDeserialize(context.Principals.Create());
                    Assert.IsAssignableFrom<IEntityWithRelationships>(anEntity);

                    var stateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
                    var manager = stateManager.GetRelationshipManager(anEntity);

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    var relatedEnd = manager.GetRelatedCollection<FullyVirtualDep>(
                        "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents", "FullyVirtualPrin_Dependents_Target");

                    Assert.Equal(
                        Strings.RelationshipManager_CollectionAlreadyInitialized(
                            Strings.RelationshipManager_CollectionInitializeIsForDeserialization),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            manager.InitializeRelatedCollection(
                                "System.Data.Entity.Core.Objects.DataClasses.FullyVirtualPrin_Dependents",
                                "FullyVirtualPrin_Dependents_Target",
                                relatedEnd)).Message);
                }
            }
        }

        public class GetAllRelatedEnds : TestBase
        {
            [Fact]
            public void Serializing_and_deserializing_change_tracking_proxies_does_not_require_expensive_lookup()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();
                    var manager = ((IEntityWithRelationships)anEntity).RelationshipManager;

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    SerializeAndDeserialize(anEntity);

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }

            [Fact]
            public void GetAllRelatedEnds_for_a_detached_change_tracking_proxy_does_not_require_expensive_lookup()
            {
                using (var context = new DummyContext())
                {
                    var anEntity = context.Principals.Create();
                    var manager = ((IEntityWithRelationships)anEntity).RelationshipManager;

                    var mockLoader = new Mock<ExpensiveOSpaceLoader>();
                    manager.SetExpensiveLoader(mockLoader.Object);

                    var ends = manager.GetAllRelatedEnds().ToList<IRelatedEnd>();
                    Assert.Equal(2, ends.Count);

                    mockLoader.Verify(m => m.LoadTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                    mockLoader.Verify(m => m.GetRelationshipTypeExpensiveWay(It.IsAny<Type>(), It.IsAny<string>()), Times.Never());
                    mockLoader.Verify(m => m.GetAllRelationshipTypesExpensiveWay(It.IsAny<Assembly>()), Times.Never());
                }
            }
        }

        private static T SerializeAndDeserialize<T>(T instance)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, instance);
            stream.Seek(0, SeekOrigin.Begin);

            return (T)formatter.Deserialize(stream);
        }

        public class DummyContext : DbContext
        {
            static DummyContext()
            {
                Database.SetInitializer<DummyContext>(null);
            }

            public DummyContext()
            {
                Configuration.LazyLoadingEnabled = false;
            }

            public DbSet<FullyVirtualDep> Dependents { get; set; }
            public DbSet<FullyVirtualPrin> Principals { get; set; }
            public DbSet<NavVirtual> NavVirtuals { get; set; }
        }
    }

    [Serializable]
    public class FullyVirtualDep
    {
        public virtual int Id { get; set; }
        public virtual int OtherId { get; set; }
        public virtual NavVirtual Other { get; set; }
        public virtual int PrincipalId { get; set; }
        public virtual FullyVirtualPrin Principal { get; set; }
    }

    [Serializable]
    public class NavVirtual
    {
        public int Id { get; set; }
        public virtual ICollection<FullyVirtualDep> OtherDeps { get; set; }
        public virtual ICollection<FullyVirtualPrin> OtherPrins { get; set; }
    }

    [Serializable]
    public class FullyVirtualPrin
    {
        public virtual int Id { get; set; }
        public virtual ICollection<FullyVirtualDep> Dependents { get; set; }
        public virtual int OtherId { get; set; }
        public virtual NavVirtual Other { get; set; }
    }
}
