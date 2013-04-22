// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    /// <summary>
    ///     Unit tests for data binding and DbSet.Local.
    /// </summary>
    public class DatabindingTests : TestBase
    {
        #region Test subjects

        private class ListElement
        {
            public ListElement()
            {
            }

            public ListElement(int i)
            {
                Int = i;
                NullableInt = i;
                String = i.ToString();
                XNode = new XText(i.ToString());
                Random = new Random();
                ByteArray = new[] { (byte)i, (byte)i, (byte)i, (byte)i };
            }

            public static implicit operator ListElement(int i)
            {
                return new ListElement(i);
            }

            public int Int { get; set; }
            public int? NullableInt { get; set; }
            public string String { get; set; }
            public XNode XNode { get; set; }
            public Random Random { get; set; }
            public byte[] ByteArray { get; set; }

            public static PropertyDescriptor Property(string name)
            {
                return TypeDescriptor.GetProperties(typeof(ListElement))[name];
            }
        }

        private class DerivedListElement : ListElement
        {
            public DerivedListElement()
            {
            }

            public DerivedListElement(int i)
                : base(i)
            {
            }
        }

        private class ListElementComparer : IEqualityComparer<ListElement>
        {
            public bool Equals(ListElement x, ListElement y)
            {
                return x.Int == y.Int;
            }

            public int GetHashCode(ListElement obj)
            {
                return obj.Int;
            }
        }

        #endregion

        #region SortableBindingList tests

        private void SortTest(string property, ListSortDirection direction)
        {
            var list = new List<ListElement>
                           {
                               3,
                               1,
                               4,
                               1,
                               5,
                               9
                           };
            var sortedList = direction == ListSortDirection.Ascending
                                 ? new List<ListElement>
                                       {
                                           1,
                                           1,
                                           3,
                                           4,
                                           5,
                                           9
                                       }
                                 : new List<ListElement>
                                       {
                                           9,
                                           5,
                                           4,
                                           3,
                                           1,
                                           1
                                       };

            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property(property), direction);

            Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_value_type()
        {
            SortTest("Int", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_nullable_value_type()
        {
            SortTest("NullableInt", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_reference_type()
        {
            SortTest("String", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_value_type()
        {
            SortTest("Int", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_nullable_value_type()
        {
            SortTest("NullableInt", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_reference_type()
        {
            SortTest("String", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_for_XNode_using_ToString()
        {
            SortTest("XNode", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_for_XNode_using_ToString()
        {
            SortTest("XNode", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_does_not_sort_for_non_XNode_that_does_not_implement_IComparable()
        {
            var list = new List<ListElement>
                           {
                               3,
                               1,
                               4,
                               1,
                               5,
                               9
                           };
            var unsortedList = new List<ListElement>
                                   {
                                       3,
                                       1,
                                       4,
                                       1,
                                       5,
                                       9
                                   };
            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("Random"), ListSortDirection.Ascending);

            Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_does_not_sort_for_byte_arrays()
        {
            var list = new List<ListElement>
                           {
                               3,
                               1,
                               4,
                               1,
                               5,
                               9
                           };
            var unsortedList = new List<ListElement>
                                   {
                                       3,
                                       1,
                                       4,
                                       1,
                                       5,
                                       9
                                   };
            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("ByteArray"), ListSortDirection.Descending);

            Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_can_sort_when_list_contains_derived_objects()
        {
            var list = new List<ListElement>
                           {
                               new DerivedListElement(3),
                               new DerivedListElement(1),
                               new DerivedListElement(4)
                           };
            var sortedList = new List<ListElement>
                                 {
                                     new DerivedListElement(1),
                                     new DerivedListElement(3),
                                     new DerivedListElement(4)
                                 };

            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("Int"), ListSortDirection.Ascending);

            Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_can_sort_when_list_is_of_derived_type()
        {
            var list = new List<DerivedListElement>
                           {
                               new DerivedListElement(3),
                               new DerivedListElement(1),
                               new DerivedListElement(4)
                           };
            var sortedList = new List<DerivedListElement>
                                 {
                                     new DerivedListElement(1),
                                     new DerivedListElement(3),
                                     new DerivedListElement(4)
                                 };

            var bindingList = new SortableBindingList<DerivedListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("Int"), ListSortDirection.Ascending);

            Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
        }

        #endregion

        #region ObservableBackedBindingList tests

        [Fact]
        public void Items_added_to_ObservableCollection_are_added_to_binding_list()
        {
            var oc = new ObservableCollection<ListElement>();
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var item = new ListElement(1);
            oc.Add(item);

            Assert.True(obbl.Contains(item));
        }

        [Fact]
        public void Items_removed_from_ObservableCollection_are_removed_from_binding_list()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            oc.Remove(item);

            Assert.False(obbl.Contains(item));
            Assert.Equal(5, obbl.Count);
        }

        [Fact]
        public void Items_replaced_in_the_ObservableCollection_are_replaced_in_the_binding_list()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var newItem = new ListElement(-4);
            oc[2] = newItem;

            Assert.False(obbl.Contains(item));
            Assert.True(obbl.Contains(newItem));
            Assert.Equal(6, obbl.Count);
        }

        [Fact]
        public void Items_cleared_in_the_ObservableCollection_are_cleared_in_the_binding_list()
        {
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             4,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            oc.Clear();

            Assert.Equal(0, obbl.Count);
        }

        [Fact]
        public void Adding_duplicate_item_to_the_ObservableCollection_adds_duplicate_to_the_binding_list()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            oc.Add(item);

            Assert.Equal(7, obbl.Count);
            Assert.Equal(2, obbl.Count(i => ReferenceEquals(i, item)));
        }

        [Fact]
        public void Items_added_to_the_binding_list_are_added_to_the_ObservableCollection()
        {
            var oc = new ObservableCollection<ListElement>();
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var item = new ListElement(7);
            obbl.Add(item);

            Assert.True(oc.Contains(item));
        }

        [Fact]
        public void Items_added_to_the_binding_list_with_AddNew_are_added_to_the_ObservableCollection()
        {
            var oc = new ObservableCollection<ListElement>();
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var item = obbl.AddNew();
            obbl.EndNew(0);

            Assert.True(oc.Contains(item));
        }

        [Fact]
        public void Items_canceled_during_AddNew_are_not_added_to_the_ObservableCollection()
        {
            var oc = new ObservableCollection<ListElement>();
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var item = obbl.AddNew();
            obbl.CancelNew(0);

            Assert.False(oc.Contains(item));
        }

        [Fact]
        public void Items_inserted_into_the_binding_list_are_added_to_the_ObservableCollection()
        {
            var oc = new ObservableCollection<ListElement>();
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var item = new ListElement(7);
            obbl.Insert(0, item);

            Assert.True(oc.Contains(item));
        }

        [Fact]
        public void Items_set_in_the_binding_list_are_replaced_in_the_ObservableCollection()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            var newItem = new ListElement(7);
            obbl[2] = newItem;

            Assert.True(oc.Contains(newItem));
            Assert.False(oc.Contains(item));
        }

        [Fact]
        public void Items_removed_from_the_binding_list_are_removed_from_the_ObservableCollection()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            obbl.Remove(item);

            Assert.False(oc.Contains(item));
        }

        [Fact]
        public void Items_removed_by_index_from_the_binding_list_are_removed_from_the_ObservableCollection()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            obbl.RemoveAt(2);

            Assert.False(oc.Contains(item));
        }

        [Fact]
        public void Items_cleared_from_the_binding_list_are_cleared_from_the_ObservableCollection()
        {
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             4,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            obbl.Clear();

            Assert.Equal(0, oc.Count);
        }

        [Fact]
        public void Adding_duplicate_item_to_the_binding_list_adds_duplicate_to_the_ObservableCollection()
        {
            var item = new ListElement(4);
            var oc = new ObservableCollection<ListElement>
                         {
                             3,
                             1,
                             item,
                             1,
                             5,
                             9
                         };
            var obbl = new ObservableBackedBindingList<ListElement>(oc);

            obbl.Add(item);

            Assert.Equal(7, oc.Count);
            Assert.Equal(2, oc.Count(i => ReferenceEquals(i, item)));
        }

        [Fact]
        public void Attempt_to_AddNew_for_abstract_type_works_if_AddingNew_event_is_used_to_create_new_object()
        {
            var obbl = new ObservableBackedBindingList<XNode>(new ObservableCollection<XNode>());
            var item = new XText("Some Value");

            obbl.AddingNew += (s, e) => e.NewObject = item;
            obbl.AddNew();
            obbl.EndNew(0);

            Assert.True(obbl.Contains(item));
        }

        [Fact]
        public void Attempt_to_AddNew_for_abstract_type_throws_if_AddingNew_event_is_not_used()
        {
            var obbl = new ObservableBackedBindingList<XNode>(new ObservableCollection<XNode>());

            const BindingFlags bindingAttr = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
            Assert.Equal(
                GenerateException(() => Activator.CreateInstance(typeof(XNode), bindingAttr, null, null, null)).Message,
                Assert.Throws<MissingMethodException>(() => obbl.AddNew()).Message);
        }

        [Fact]
        public void Attempt_to_AddNew_for_type_without_parameterless_constructor_works_if_AddingNew_event_is_used_to_create_new_object()
        {
            var obbl = new ObservableBackedBindingList<XText>(new ObservableCollection<XText>());
            var item = new XText("Some Value");

            obbl.AddingNew += (s, e) => e.NewObject = item;
            obbl.AddNew();
            obbl.EndNew(0);

            Assert.True(obbl.Contains(item));
        }

        [Fact]
        public void Attempt_to_AddNew_for_type_without_parameterless_constructor_throws_if_AddingNew_event_is_not_used()
        {
            var obbl = new ObservableBackedBindingList<XText>(new ObservableCollection<XText>());

            const BindingFlags bindingAttr = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
            Assert.Equal(
                GenerateException(() => Activator.CreateInstance(typeof(XText), bindingAttr, null, null, null)).Message,
                Assert.Throws<MissingMethodException>(() => obbl.AddNew()).Message);
        }

        #endregion

        #region DbLocalView tests

        private Mock<InternalContextForMock> CreateMockedInternalContext(
            Mock<IDbSet<FakeEntity>> mockDbSet, IList<FakeEntity> entities = null)
        {
            entities = entities ?? new List<FakeEntity>();
            var mockInternalContext = new Mock<InternalContextForMock>();
            mockInternalContext.Setup(i => i.GetLocalEntities<FakeEntity>()).Returns(entities);
            mockInternalContext.Setup(i => i.Set<FakeEntity>()).Returns(mockDbSet.Object);
            mockInternalContext.Setup(i => i.EntityInContextAndNotDeleted(It.Is<FakeEntity>(e => entities.Contains(e))));
            return mockInternalContext;
        }

        private DbLocalView<FakeEntity> CreateLocalView(Mock<IDbSet<FakeEntity>> mockDbSet, IList<FakeEntity> entities = null)
        {
            return new DbLocalView<FakeEntity>(CreateMockedInternalContext(mockDbSet, entities).Object);
        }

        [Fact]
        public void DbLocalView_is_initialized_with_entities_from_the_context()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var localView = CreateLocalView(new Mock<IDbSet<FakeEntity>>(), entities);

            Assert.Equal(2, localView.Count);
            Assert.True(localView.Contains(entities[0]));
            Assert.True(localView.Contains(entities[1]));
        }

        [Fact]
        public void Adding_entity_to_DbLocalView_adds_entity_to_DbSet()
        {
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(mockDbSet);

            var entity = new FakeEntity();
            localView.Add(entity);

            mockDbSet.Verify(s => s.Add(entity), Times.Once());
        }

        [Fact]
        public void Removing_entity_from_DbLocalView_removes_entity_from_DbSet()
        {
            var entity = new FakeEntity();
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(
                mockDbSet, new List<FakeEntity>
                               {
                                   entity
                               });

            localView.Remove(entity);

            mockDbSet.Verify(s => s.Remove(entity), Times.Once());
        }

        [Fact]
        public void Replacing_entity_in_DbLocalView_adds_an_entity_and_removes_an_entity_from_DbSet()
        {
            var entity = new FakeEntity();
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(
                mockDbSet, new List<FakeEntity>
                               {
                                   entity
                               });

            var newEntity = new FakeEntity();
            localView[0] = newEntity;

            mockDbSet.Verify(s => s.Remove(entity), Times.Once());
            mockDbSet.Verify(s => s.Add(newEntity), Times.Once());
        }

        [Fact]
        public void Moving_an_entity_in_DbLocalView_is_ignored()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(mockDbSet, entities);

            localView.Move(0, 1);

            mockDbSet.Verify(s => s.Remove(It.IsAny<FakeEntity>()), Times.Never());
            mockDbSet.Verify(s => s.Add(It.IsAny<FakeEntity>()), Times.Never());
        }

        [Fact]
        public void Adding_entity_to_DbLocalView_that_is_already_in_state_manager_is_ignored()
        {
            var entity = new FakeEntity();
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(
                mockDbSet, new List<FakeEntity>
                               {
                                   entity
                               });

            localView.Add(entity);

            mockDbSet.Verify(s => s.Add(It.IsAny<FakeEntity>()), Times.Never());
        }

        [Fact]
        public void BindingList_obtained_from_DbLocalView_is_cached()
        {
            var localView = CreateLocalView(new Mock<IDbSet<FakeEntity>>());

            var bindingList = localView.BindingList;
            Assert.NotNull(bindingList);

            var bindingListAgain = localView.BindingList;
            Assert.Same(bindingList, bindingListAgain);
        }

        [Fact]
        public void BindingList_obtaibed_from_DbLocalView_stays_in_sync_with_the_local_view()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var localView = CreateLocalView(new Mock<IDbSet<FakeEntity>>(), entities);

            var bindingList = localView.BindingList;
            Assert.Equal(2, bindingList.Count);

            localView.Add(new FakeEntity());
            Assert.Equal(3, bindingList.Count);

            localView.Remove(entities[0]);
            Assert.Equal(2, bindingList.Count);
        }

        [Fact]
        public void DbLocalView_stays_in_sync_with_BindingList_obtaibed_from_it()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var localView = CreateLocalView(new Mock<IDbSet<FakeEntity>>(), entities);

            var bindingList = localView.BindingList;
            Assert.Equal(2, bindingList.Count);

            bindingList.Add(new FakeEntity());
            Assert.Equal(3, localView.Count);

            bindingList.Remove(entities[0]);
            Assert.Equal(2, localView.Count);
        }

        [Fact]
        public void Clear_on_DbLocalView_removes_all_items_from_DbSet()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(mockDbSet, entities);

            localView.Clear();

            Assert.Equal(0, localView.Count);
            mockDbSet.Verify(s => s.Remove(It.IsAny<FakeEntity>()), Times.Exactly(2));
        }

        [Fact]
        public void Attempted_adds_of_duplicates_to_DbLocalView_are_ignored()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity()
                               };
            var mockDbSet = new Mock<IDbSet<FakeEntity>>();
            var localView = CreateLocalView(mockDbSet, entities);

            localView.Add(entities[0]);

            Assert.Equal(1, localView.Count);
            mockDbSet.Verify(s => s.Add(It.IsAny<FakeEntity>()), Times.Never());
        }

        [Fact]
        public void State_manager_Remove_event_causes_entity_to_be_removed_from_DbLocalView()
        {
            var entity = new FakeEntity();
            var mockInternalContext = CreateMockedInternalContext(
                new Mock<IDbSet<FakeEntity>>(), new List<FakeEntity>
                                                    {
                                                        entity
                                                    });

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            var localView = new DbLocalView<FakeEntity>(mockInternalContext.Object);

            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Remove, entity));

            Assert.False(localView.Contains(entity));
        }

        [Fact]
        public void State_manager_Remove_event_for_entity_not_in_DbLocalView_is_ignored()
        {
            var mockInternalContext = CreateMockedInternalContext(new Mock<IDbSet<FakeEntity>>());

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            new DbLocalView<FakeEntity>(mockInternalContext.Object);

            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Remove, new FakeEntity()));
        }

        [Fact]
        public void State_manager_Remove_event_for_entity_of_wrong_type_for_DbLocalView_is_ignored()
        {
            var mockInternalContext = CreateMockedInternalContext(new Mock<IDbSet<FakeEntity>>());

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            new DbLocalView<FakeEntity>(mockInternalContext.Object);

            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Remove, "Wrong Type"));
        }

        [Fact]
        public void State_manager_Add_event_causes_entity_to_be_added_to_DbLocalView()
        {
            var mockInternalContext = CreateMockedInternalContext(new Mock<IDbSet<FakeEntity>>());

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            var localView = new DbLocalView<FakeEntity>(mockInternalContext.Object);

            var entity = new FakeEntity();
            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Add, entity));

            Assert.True(localView.Contains(entity));
        }

        [Fact]
        public void State_manager_Add_event_for_entity_already_in_DbLocalView_is_ignored()
        {
            var entity = new FakeEntity();
            var mockInternalContext = CreateMockedInternalContext(
                new Mock<IDbSet<FakeEntity>>(), new List<FakeEntity>
                                                    {
                                                        entity
                                                    });

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            var localView = new DbLocalView<FakeEntity>(mockInternalContext.Object);

            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Add, entity));

            Assert.Equal(1, localView.Count);
        }

        [Fact]
        public void State_manager_Add_event_for_entity_of_wrong_type_for_DbLocalView_is_ignored()
        {
            var mockInternalContext = CreateMockedInternalContext(new Mock<IDbSet<FakeEntity>>());

            CollectionChangeEventHandler stateManagerChanged = null;
            mockInternalContext.Setup(i => i.RegisterObjectStateManagerChangedEvent(It.IsAny<CollectionChangeEventHandler>())).
                Callback<CollectionChangeEventHandler>(h => stateManagerChanged = h);

            var localView = new DbLocalView<FakeEntity>(mockInternalContext.Object);

            stateManagerChanged.Invoke(null, new CollectionChangeEventArgs(CollectionChangeAction.Add, "Wrong Type"));

            Assert.Equal(0, localView.Count);
        }

        #endregion

        #region ToBindingList tests

        [Fact]
        public void ToBindingList_throws_when_given_null_ObservableCollection()
        {
            Assert.Equal(
                "source",
                Assert.Throws<ArgumentNullException>(() => ObservableCollectionExtensions.ToBindingList<FakeEntity>(null)).ParamName);
        }

        [Fact]
        public void ToBindingList_returns_the_cached_BindingList_when_called_with_DbLocalView()
        {
            var localView = CreateLocalView(new Mock<IDbSet<FakeEntity>>());

            var bindingList = localView.ToBindingList();
            Assert.NotNull(bindingList);

            var bindingListAgain = localView.ToBindingList();
            Assert.Same(bindingList, bindingListAgain);
        }

        [Fact]
        public void ToBindingList_returns_a_new_binding_list_each_time_when_called_on_non_DbLocalView_ObervableCollections()
        {
            var oc = new ObservableCollection<FakeEntity>();

            var bindingList = oc.ToBindingList();
            Assert.NotNull(bindingList);

            var bindingListAgain = oc.ToBindingList();
            Assert.NotNull(bindingListAgain);
            Assert.NotSame(bindingList, bindingListAgain);
        }

        #endregion

        #region ObservableListSource tests

        [Fact]
        public void ObservableListSource_exposes_ObervableCollection_parameterless_constructor()
        {
            var ols = new ObservableListSource<FakeEntity>();
            Assert.Equal(0, ols.Count);
        }

        [Fact]
        public void ObservableListSource_exposes_ObervableCollection_IEnumerable_constructor()
        {
            IEnumerable<FakeEntity> entities = new[] { new FakeEntity(), new FakeEntity() };
            var ols = new ObservableListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void ObservableListSource_exposes_ObervableCollection_List_constructor()
        {
            var entities = new List<FakeEntity>
                               {
                                   new FakeEntity(),
                                   new FakeEntity()
                               };
            var ols = new ObservableListSource<FakeEntity>(entities);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void ObservableListSource_ContainsListCollection_returns_false()
        {
            Assert.False(((IListSource)new ObservableListSource<FakeEntity>()).ContainsListCollection);
        }

        [Fact]
        public void ObservableListSource_GetList_returns_BindingList_attached_to_the_ObservableCollection()
        {
            var ols = new ObservableListSource<FakeEntity>
                          {
                              new FakeEntity(),
                              new FakeEntity()
                          };
            var bindingList = ((IListSource)ols).GetList();

            Assert.Equal(2, bindingList.Count);

            ols.Add(new FakeEntity());
            Assert.Equal(3, bindingList.Count);

            ols.Remove(ols[0]);
            Assert.Equal(2, bindingList.Count);

            bindingList.Add(new FakeEntity());
            Assert.Equal(3, ols.Count);

            bindingList.RemoveAt(0);
            Assert.Equal(2, ols.Count);
        }

        [Fact]
        public void The_BindingList_returned_from_ObservableListSource_GetList_is_cached()
        {
            var ols = new ObservableListSource<FakeEntity>();
            var bindingList = ((IListSource)ols).GetList();

            Assert.Same(bindingList, ((IListSource)ols).GetList());
        }

        #endregion

        #region DbQuery as IListSource tests

        [Fact]
        public void DbQuery_ContainsListCollection_returns_false()
        {
            var fakeQuery = new DbQuery<FakeEntity>(new Mock<IInternalQuery<FakeEntity>>().Object);

            Assert.False(((IListSource)fakeQuery).ContainsListCollection);
        }

        [Fact]
        public void Non_generic_DbQuery_ContainsListCollection_returns_false()
        {
            var fakeQuery = new InternalDbQuery<FakeEntity>(new Mock<IInternalQuery<FakeEntity>>().Object);

            Assert.False(((IListSource)fakeQuery).ContainsListCollection);
        }

        [Fact]
        public void DbQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var fakeQuery = new DbQuery<FakeEntity>(new Mock<IInternalQuery<FakeEntity>>().Object);

            Assert.Equal(
                Strings.DbQuery_BindingToDbQueryNotSupported,
                Assert.Throws<NotSupportedException>(() => ((IListSource)fakeQuery).GetList()).Message);
        }

        [Fact]
        public void Non_generic_DbQuery_GetList_throws_indicating_that_binding_to_queries_is_not_allowed()
        {
            var fakeQuery = new InternalDbQuery<FakeEntity>(new Mock<IInternalQuery<FakeEntity>>().Object);

            Assert.Equal(
                Strings.DbQuery_BindingToDbQueryNotSupported,
                Assert.Throws<NotSupportedException>(() => ((IListSource)fakeQuery).GetList()).Message);
        }

        #endregion

        #region Load tests

        [Fact]
        public void Load_throws_when_given_null_query()
        {
            Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => QueryableExtensions.Load(null)).ParamName);
        }

        [Fact]
        public void Load_enumerates_the_query()
        {
            var count = 0;
            var mockEnumerator = new Mock<IEnumerator<FakeEntity>>();
            var mockQuery = new Mock<IList<FakeEntity>>();
            mockQuery.Setup(q => q.GetEnumerator()).Returns(mockEnumerator.Object);
            mockEnumerator.Setup(e => e.MoveNext()).Returns(() => count++ < 5 ? true : false);

            mockQuery.Object.AsQueryable().Load();

            mockEnumerator.Verify(e => e.MoveNext(), Times.Exactly(6));
        }

        #endregion
    }
}
