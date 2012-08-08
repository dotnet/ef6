// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Internal.Linq;
    using Moq;

    public static class MockHelper
    {
        internal static InternalSqlSetQuery CreateInternalSqlSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlSetQuery(new Mock<InternalSetForMock<FakeEntity>>().Object, sql, false, parameters);
        }

        internal static InternalSqlNonSetQuery CreateInternalSqlNonSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlNonSetQuery(new Mock<InternalContextForMock>().Object, typeof(object), sql, parameters);
        }

        internal static Mock<IEntityStateEntry> CreateMockStateEntry<TEntity>() where TEntity : class, new()
        {
            var mockStateEntry = new Mock<IEntityStateEntry>();
            var fakeEntity = new TEntity();
            mockStateEntry.Setup(e => e.Entity).Returns(fakeEntity);

            var entitySet = new EntitySet("foo set", "foo schema", "foo table", "foo query", new EntityType());
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("foo container", DataSpace.CSpace));
            mockStateEntry.Setup(e => e.EntitySet).Returns(entitySet);

            mockStateEntry.Setup(e => e.EntityKey).Returns(new EntityKey("foo.bar",
                new[] { new KeyValuePair<string, object> ("foo", "bar")}));

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
            TEntity entity, bool isDetached = false)
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
            var mockInternalEntityEntry = new Mock<InternalEntityEntryForMock<TEntity>>() {CallBase = true};
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
    }
}
