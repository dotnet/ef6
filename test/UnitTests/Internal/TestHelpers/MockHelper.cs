// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq;

    public static class MockHelper
    {
        internal static InternalSqlSetQuery CreateInternalSqlSetQuery(string sql, bool isNoTracking = false,  params object[] parameters)
        {
            return new InternalSqlSetQuery(new Mock<InternalSetForMock<FakeEntity>>().Object, sql, isNoTracking, parameters);
        }

        internal static InternalSqlNonSetQuery CreateInternalSqlNonSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlNonSetQuery(new Mock<InternalContextForMock>().Object, typeof(object), sql, parameters);
        }

        internal static Mock<InternalSqlNonSetQuery> CreateMockInternalSqlNonSetQuery(string sql, params object[] parameters)
        {
            return new Mock<InternalSqlNonSetQuery>(new Mock<InternalContextForMock>().Object, typeof(object), sql, parameters);
        }

        internal static Mock<IEntityStateEntry> CreateMockStateEntry<TEntity>() where TEntity : class, new()
        {
            var mockStateEntry = new Mock<IEntityStateEntry>();
            var fakeEntity = new TEntity();
            mockStateEntry.Setup(e => e.Entity).Returns(fakeEntity);

            var entitySet = new EntitySet("foo set", "foo schema", "foo table", "foo query", new EntityType("E", "N", DataSpace.CSpace));
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("foo container", DataSpace.CSpace));
            mockStateEntry.Setup(e => e.EntitySet).Returns(entitySet);

            mockStateEntry.Setup(e => e.EntityKey).Returns(
                new EntityKey(
                    "foo.bar",
                    new[] { new KeyValuePair<string, object>("foo", "bar") }));

            var modifiedProps = new List<string>
                                    {
                                        "Foo",
                                        "ValueTypeProp",
                                        "Bar"
                                    };
            mockStateEntry.Setup(e => e.GetModifiedProperties()).Returns(modifiedProps);
            mockStateEntry.Setup(e => e.SetModifiedProperty(It.IsAny<string>())).Callback<string>(modifiedProps.Add);

            return mockStateEntry;
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity>()
            where TEntity : class, new()
        {
            return CreateMockInternalEntityEntry<TEntity, object>(new TEntity());
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity>(
            TEntity entity)
            where TEntity : class, new()
        {
            var mockInternalEntityEntry = CreateMockInternalEntityEntry<TEntity, object>(entity, isDetached: false);

            var mockChildProperties = GetMockPropertiesForEntityOrComplexType(mockInternalEntityEntry.Object, null, entity);
            mockInternalEntityEntry.Setup(e => e.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                                   .Returns((string propertyName, Type requestedType, bool requiresComplex) => mockChildProperties[propertyName]);
            mockInternalEntityEntry.Setup(e => e.Member(It.IsAny<string>(), It.IsAny<Type>()))
                                   .Returns((string propertyName, Type requestedType) => mockChildProperties[propertyName]);

            return mockInternalEntityEntry;
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity>(
            TEntity entity, bool isDetached)
            where TEntity : class, new()
        {
            return CreateMockInternalEntityEntry<TEntity, object>(entity, isDetached: isDetached);
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity, TRelated>(
            TEntity entity, EntityReference<TRelated> entityReference = null, bool isDetached = false)
            where TEntity : class, new()
            where TRelated : class
        {
            return CreateMockInternalEntityEntry(entity, entityReference, null, isDetached);
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity, TRelated>(
            TEntity entity, EntityCollection<TRelated> entityCollection, bool isDetached = false)
            where TEntity : class, new()
            where TRelated : class
        {
            return CreateMockInternalEntityEntry(entity, null, entityCollection, isDetached);
        }

        internal static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntityEntry<TEntity, TRelated>(
            TEntity entity, EntityReference<TRelated> entityReference, EntityCollection<TRelated> entityCollection, bool isDetached)
            where TEntity : class, new()
            where TRelated : class
        {
            var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<TEntity>>
                                              {
                                                  CallBase = true
                                              };
            mockInternalEntityEntry.SetupGet(e => e.Entity).Returns(entity);
            mockInternalEntityEntry.SetupGet(e => e.EntityType).Returns(typeof(TEntity));
            mockInternalEntityEntry.SetupGet(e => e.IsDetached).Returns(isDetached);
            mockInternalEntityEntry.Setup(e => e.GetRelatedEnd("Reference")).Returns(entityReference);
            mockInternalEntityEntry.Setup(e => e.GetRelatedEnd("Collection")).Returns(entityCollection);

            return mockInternalEntityEntry;
        }

        internal static Mock<InternalEntityPropertyEntry> CreateMockInternalEntityPropertyEntry(object entity)
        {
            var propertyEntry = new Mock<InternalEntityPropertyEntry>(
                CreateMockInternalEntityEntry(
                    entity, new EntityReference<FakeEntity>(), new EntityCollection<FakeEntity>(), isDetached: false).Object,
                new PropertyEntryMetadataForMock());
            propertyEntry.SetupGet(p => p.InternalEntityEntry).Returns(new InternalEntityEntryForMock<object>());

            return propertyEntry;
        }

        internal static Mock<InternalEntityEntryForMock<object>> CreateMockInternalEntityEntry(Dictionary<string, object> values)
        {
            var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<object>>();
            foreach (var propertyName in values.Keys)
            {
                var mockEntityProperty = new Mock<InternalEntityPropertyEntry>(
                    CreateMockInternalEntityEntry(new object()).Object,
                    new PropertyEntryMetadataForMock());
                mockEntityProperty.CallBase = true;
                mockEntityProperty.SetupGet(p => p.Name).Returns(propertyName);
                mockEntityProperty.SetupGet(p => p.CurrentValue).Returns(values[propertyName]);
                mockEntityProperty.SetupGet(p => p.InternalEntityEntry).Returns(mockInternalEntityEntry.Object);

                mockInternalEntityEntry.Setup(e => e.Property(propertyName, It.IsAny<Type>(), It.IsAny<bool>()))
                                       .Returns(mockEntityProperty.Object);
                mockInternalEntityEntry.Setup(e => e.Member(propertyName, It.IsAny<Type>()))
                                       .Returns(mockEntityProperty.Object);
            }

            mockInternalEntityEntry.Setup(e => e.Entity).Returns(new object());

            return mockInternalEntityEntry;
        }

        internal static Dictionary<string, InternalPropertyEntry> GetMockPropertiesForEntityOrComplexType(
            InternalEntityEntry owner, InternalPropertyEntry parentPropertyEntry, object parent)
        {
            var mockChildProperties = new Dictionary<string, InternalPropertyEntry>();

            // do not create mocks for nulls
            if (parent != null)
            {
                foreach (var childPropInfo in parent.GetType().GetInstanceProperties().Where(p => p.IsPublic()))
                {
                    if (childPropInfo.Getter() != null
                        && childPropInfo.Setter() != null)
                    {
                        var mockInternalPropertyEntry = CreateMockInternalPropertyEntry(owner, parentPropertyEntry, childPropInfo, parent);
                        mockChildProperties.Add(childPropInfo.Name, mockInternalPropertyEntry);
                    }
                }
            }

            return mockChildProperties;
        }

        internal static InternalPropertyEntry CreateMockInternalPropertyEntry(
            InternalEntityEntry owner, InternalPropertyEntry parentPropertyEntry, PropertyInfo propInfo, object parent)
        {
            var propertyValue = propInfo.Getter().Invoke(parent, new object[0]);

            InternalPropertyEntry childPropertyEntry;
            if (parentPropertyEntry == null)
            {
                var mockEntityProperty = new Mock<InternalEntityPropertyEntry>(
                    CreateMockInternalEntityEntry(propertyValue).Object,
                    new PropertyEntryMetadataForMock());
                mockEntityProperty.CallBase = true;
                mockEntityProperty.SetupGet(p => p.Name).Returns(propInfo.Name);
                mockEntityProperty.SetupGet(p => p.CurrentValue).Returns(propertyValue);
                mockEntityProperty.SetupGet(p => p.InternalEntityEntry).Returns(owner);

                var mockChildProperties = GetMockPropertiesForEntityOrComplexType(owner, mockEntityProperty.Object, propertyValue);

                mockEntityProperty.Setup(p => p.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                                  .Returns(
                                      (string propertyName, Type requestedType, bool requiresComplex) =>
                                      mockChildProperties.ContainsKey(propertyName)
                                          ? mockChildProperties[propertyName]
                                          : CreateInternalPropertyEntryForNullParent(propertyName));

                childPropertyEntry = mockEntityProperty.Object;
            }
            else
            {
                var mockComplexProperty = new Mock<InternalNestedPropertyEntry>(
                    CreateMockInternalEntityPropertyEntry(propertyValue).Object,
                    new PropertyEntryMetadataForMock());
                mockComplexProperty.CallBase = true;
                mockComplexProperty.SetupGet(p => p.Name).Returns(propInfo.Name);
                mockComplexProperty.SetupGet(p => p.CurrentValue).Returns(propertyValue);
                mockComplexProperty.SetupGet(p => p.InternalEntityEntry).Returns(owner);
                mockComplexProperty.SetupGet(p => p.ParentPropertyEntry).Returns(parentPropertyEntry);

                var mockChildProperties = GetMockPropertiesForEntityOrComplexType(owner, mockComplexProperty.Object, propertyValue);

                mockComplexProperty.Setup(p => p.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                                   .Returns(
                                       (string propertyName, Type requestedType, bool requiresComplex) =>
                                       mockChildProperties.ContainsKey(propertyName)
                                           ? mockChildProperties[propertyName]
                                           : CreateInternalPropertyEntryForNullParent(propertyName));

                childPropertyEntry = mockComplexProperty.Object;
            }

            return childPropertyEntry;
        }

        internal static InternalPropertyEntry CreateInternalPropertyEntryForNullParent(string propertyName)
        {
            var parentNullPropertyEntry = new Mock<InternalEntityPropertyEntry>(
                CreateMockInternalEntityEntry(new object()).Object,
                new PropertyEntryMetadataForMock());
            parentNullPropertyEntry.SetupGet(p => p.Name).Returns(propertyName);
            parentNullPropertyEntry.SetupGet(p => p.ParentPropertyEntry);
            parentNullPropertyEntry.SetupGet(p => p.CurrentValue).Throws(new NullReferenceException());

            return parentNullPropertyEntry.Object;
        }
    }
}
