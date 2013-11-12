// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;
    using MockHelper = System.Data.Entity.Core.Objects.MockHelper;

    /// <summary>
    /// Unit tests for <see cref="DbSet" /> and <see cref="DbSet{T}" />.
    /// Note that some tests that would normally be unit tests are in the functional tests project because they
    /// were created before the functional/unit division.
    /// </summary>
    public class DbSetTests : TestBase
    {
        public class Add_Generic
        {
            [Fact]
            public void With_valid_entity_returns_the_added_entity()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Add(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class AddRange_Generic
        {
            [Fact]
            public void With_valid_entities_returns_the_added_entities()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();

                internalSet.Setup(i => i.InternalContext)
                           .Returns(internalContext.Object);

                var set = new DbSet<FakeEntity>(internalSet.Object);
                var entity1 = new FakeEntity();
                var entity2 = new FakeEntity();

                var retVal = set.AddRange(new[] { entity1, entity2 });

                Assert.True(retVal.Contains(entity1));
                Assert.True(retVal.Contains(entity2));
            }

            [Fact]
            public void Throw_if_collection_is_null()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();

                internalSet.Setup(i => i.InternalContext)
                           .Returns(internalContext.Object);

                var set = new DbSet<FakeEntity>(internalSet.Object);

                Assert.Equal(
                    new ArgumentNullException("entities").Message,
                    Assert.Throws<ArgumentNullException>(() => set.AddRange(null)).Message);
            }

            [Fact]
            public void Throw_if_some_item_in_collection_is_null()
            {
                var internalSet = new InternalSetForTests();
                var set = new DbSet<FakeEntity>(internalSet);

                Assert.Equal(
                    new ArgumentNullException("entity").Message,
                    Assert.Throws<ArgumentNullException>(() => set.AddRange(new[] { null, new FakeEntity() })).Message);
            }

            [Fact]
            public void Call_to_detect_changes_only_once()
            {
                var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<FakeEntity>());
                objectContextMock.Setup(s => s.AddObject(It.IsAny<string>(), It.IsAny<FakeEntity>()));

                var internalContext = new Mock<InternalContextForMock>();
                internalContext.Setup(c => c.ObjectContext)
                               .Returns(objectContextMock.Object);

                var internalSet = new InternalSetForTests(internalContext.Object);
                var set = new DbSet<FakeEntity>(internalSet);

                set.AddRange(new[] { new FakeEntity(), new FakeEntity() });

                internalContext.Verify(c => c.DetectChanges(It.Is<bool>(b => b == false)), Times.Once());
            }
        }

        public class Add_NonGeneric
        {
            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }
                    .Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbSet_BadTypeForAddAttachRemove("Add", "String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.Add("Bang!")).Message);
            }

            [Fact]
            public void With_valid_entity_returns_the_added_entity()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Add(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class AddRange_NonGeneric
        {
            [Fact]
            public void With_valid_entities_returns_the_added_entities()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();
                internalSet.Setup(i => i.InternalContext).Returns(internalContext.Object);

                var set = new InternalDbSet<FakeEntity>(internalSet.Object);

                var entity1 = new FakeEntity();
                var entity2 = new FakeEntity();

                var retVal = (IEnumerable<FakeEntity>)set
                                                          .AddRange(new[] { entity1, entity2 });

                Assert.True(retVal.Contains(entity1));
                Assert.True(retVal.Contains(entity2));
            }

            [Fact]
            public void Throw_if_collection_is_null()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();
                internalSet.Setup(i => i.InternalContext).Returns(internalContext.Object);

                var set = new InternalDbSet<FakeEntity>(internalSet.Object);

                Assert.Equal(
                    new ArgumentNullException("entities").Message,
                    Assert.Throws<ArgumentNullException>(() => set.AddRange(null)).Message);
            }

            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new InternalSetForTests();

                Assert.Equal(
                    Strings.DbSet_BadTypeForAddAttachRemove("AddRange", "String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.AddRange(new[] { "Bang!" })).Message);
            }

            [Fact]
            public void Throw_if_some_item_in_collection_is_null()
            {
                var set = new InternalSetForTests();

                Assert.Equal(
                    new ArgumentNullException("entity").Message,
                    Assert.Throws<ArgumentNullException>(() => set.AddRange(new[] { null, new FakeEntity() })).Message);
            }

            [Fact]
            public void Call_to_detect_changes_only_once()
            {
                var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<FakeEntity>());
                objectContextMock.Setup(s => s.AddObject(It.IsAny<string>(), It.IsAny<FakeEntity>()));

                var internalContext = new Mock<InternalContextForMock>();
                internalContext.Setup(c => c.ObjectContext)
                               .Returns(objectContextMock.Object);

                var set = new InternalSetForTests(internalContext.Object);

                set.AddRange(new[] { new FakeEntity(), new FakeEntity() });

                internalContext.Verify(c => c.DetectChanges(It.Is<bool>(b => b == false)), Times.Once());
            }
        }

        public class Attach_Generic
        {
            [Fact]
            public void With_valid_entity_returns_the_added_entity()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Attach(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class Attach_NonGeneric
        {
            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbSet_BadTypeForAddAttachRemove("Attach", "String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.Attach("Bang!")).Message);
            }

            [Fact]
            public void With_valid_entity_returns_the_added_entity()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Attach(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class Remove_Generic
        {
            [Fact]
            public void With_valid_entity_returns_the_removed_entity()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Remove(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class RemoveRange_Generic
        {
            [Fact]
            public void With_valid_entities_returns_the_removed_entities()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();

                internalSet.Setup(i => i.InternalContext)
                           .Returns(internalContext.Object);

                var set = new DbSet<FakeEntity>(internalSet.Object);
                var entity1 = new FakeEntity();
                var entity2 = new FakeEntity();

                var retVal = set.RemoveRange(new[] { entity1, entity2 });

                Assert.True(retVal.Contains(entity1));
                Assert.True(retVal.Contains(entity2));
            }

            [Fact]
            public void Throw_if_collection_is_null()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();

                internalSet.Setup(i => i.InternalContext)
                           .Returns(internalContext.Object);

                var set = new DbSet<FakeEntity>(internalSet.Object);

                Assert.Equal(
                    new ArgumentNullException("entities").Message,
                    Assert.Throws<ArgumentNullException>(() => set.RemoveRange(null)).Message);
            }

            [Fact]
            public void Throw_if_some_item_in_collection_is_null()
            {
                var internalSet = new InternalSetForTests();
                var set = new DbSet<FakeEntity>(internalSet);

                Assert.Equal(
                    new ArgumentNullException("entity").Message,
                    Assert.Throws<ArgumentNullException>(() => set.RemoveRange(new[] { null, new FakeEntity() })).Message);
            }

            [Fact]
            public void Call_to_detect_changes_only_once()
            {
                var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<FakeEntity>());
                objectContextMock.Setup(s => s.DeleteObject(It.IsAny<FakeEntity>()));

                var internalContext = new Mock<InternalContextForMock>();
                internalContext.Setup(c => c.ObjectContext)
                               .Returns(objectContextMock.Object);

                var internalSet = new InternalSetForTests(internalContext.Object);
                var set = new DbSet<FakeEntity>(internalSet);

                set.RemoveRange(new[] { new FakeEntity(), new FakeEntity() });

                internalContext.Verify(c => c.DetectChanges(It.Is<bool>(b => b == false)), Times.Once());
            }
        }

        public class Remove_NonGeneric
        {
            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbSet_BadTypeForAddAttachRemove("Remove", "String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.Remove("Bang!")).Message);
            }

            [Fact]
            public void With_valid_entity_returns_the_added_entity()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
                var entity = new FakeEntity();

                var retVal = set.Remove(entity);

                Assert.Same(entity, retVal);
            }
        }

        public class RemoveRange_NonGeneric
        {
            [Fact]
            public void With_valid_entities_returns_the_removed_entities()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();
                internalSet.Setup(i => i.InternalContext).Returns(internalContext.Object);

                var set = new InternalDbSet<FakeEntity>(internalSet.Object);

                var entity1 = new FakeEntity();
                var entity2 = new FakeEntity();

                var retVal = (IEnumerable<FakeEntity>)set
                    .RemoveRange(new[] { entity1, entity2 });

                Assert.True(retVal.Contains(entity1));
                Assert.True(retVal.Contains(entity2));
            }

            [Fact]
            public void Throw_if_collection_is_null()
            {
                var internalContext = new Mock<InternalContext>();
                var internalSet = new Mock<InternalSetForMock<FakeEntity>>();
                internalSet.Setup(i => i.InternalContext).Returns(internalContext.Object);

                var set = new InternalDbSet<FakeEntity>(internalSet.Object);

                Assert.Equal(
                    new ArgumentNullException("entities").Message,
                    Assert.Throws<ArgumentNullException>(() => set.RemoveRange(null)).Message);
            }

            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new InternalSetForTests();

                Assert.Equal(
                    Strings.DbSet_BadTypeForAddAttachRemove("RemoveRange", "String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.RemoveRange(new[] { "Bang!" })).Message);
            }

            [Fact]
            public void Throw_if_some_item_in_collection_is_null()
            {
                var set = new InternalSetForTests();

                Assert.Equal(
                    new ArgumentNullException("entity").Message,
                    Assert.Throws<ArgumentNullException>(() => set.RemoveRange(new[] { null, new FakeEntity() })).Message);
            }

            [Fact]
            public void Call_to_detect_changes_only_once()
            {
                var objectContextMock = Mock.Get(MockHelper.CreateMockObjectContext<FakeEntity>());
                objectContextMock.Setup(s => s.DeleteObject(It.IsAny<FakeEntity>()));

                var internalContext = new Mock<InternalContextForMock>();
                internalContext.Setup(c => c.ObjectContext)
                               .Returns(objectContextMock.Object);

                var set = new InternalSetForTests(internalContext.Object);

                set.RemoveRange(new[] { new FakeEntity(), new FakeEntity() });

                internalContext.Verify(c => c.DetectChanges(It.Is<bool>(b => b == false)), Times.Once());
            }
        }

        public class Create_Generic
        {
            [Fact]
            public void With_same_type_returns_non_null_object()
            {
                var internalContextMock = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    };

                internalContextMock.Setup(m => m.CreateObject<FakeEntity>()).Returns(new FakeEntity());

                var internalSetMock = new Mock<InternalSet<FakeEntity>>(internalContextMock.Object);
                internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);

                var set = new DbSet<FakeEntity>(internalSetMock.Object);
                Assert.NotNull(set.Create());
            }

            [Fact]
            public void With_derived_type_returns_non_null_object()
            {
                var internalContextMock = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    };

                internalContextMock.Setup(m => m.CreateObject<FakeDerivedEntity>()).Returns(new FakeDerivedEntity());

                var internalSetMock = new Mock<InternalSet<FakeEntity>>(internalContextMock.Object);
                internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);

                var set = new DbSet<FakeEntity>(internalSetMock.Object);
                Assert.IsType<FakeDerivedEntity>(set.Create<FakeDerivedEntity>());
            }
        }

        public class Create_NonGeneric
        {
            [Fact]
            public void With_null_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal("derivedEntityType", Assert.Throws<ArgumentNullException>(() => set.Create(null)).ParamName);
            }

            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbSet_BadTypeForCreate("String", "FakeEntity"),
                    Assert.Throws<ArgumentException>(() => set.Create(typeof(string))).Message);
            }

            [Fact]
            public void With_same_type_returns_non_null_object()
            {
                var internalContextMock = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    };

                internalContextMock.Setup(m => m.CreateObject<FakeEntity>()).Returns(new FakeEntity());

                var internalSetMock = new Mock<InternalSet<FakeEntity>>(internalContextMock.Object);
                internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);

                var set = new InternalDbSet<FakeEntity>(internalSetMock.Object);

                Assert.NotNull(set.Create());
            }

            [Fact]
            public void With_derived_type_returns_non_null_object()
            {
                var internalContextMock = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    };

                internalContextMock.Setup(m => m.CreateObject<FakeDerivedEntity>()).Returns(new FakeDerivedEntity());

                var internalSetMock = new Mock<InternalSet<FakeEntity>>(internalContextMock.Object);
                internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);

                var set = new InternalDbSet<FakeEntity>(internalSetMock.Object);

                Assert.IsType<FakeDerivedEntity>(set.Create(typeof(FakeDerivedEntity)));
            }
        }

        public class Cast
        {
            [Fact]
            public void With_wrong_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbEntity_BadTypeForCast("DbSet", "String", "FakeEntity"),
                    Assert.Throws<InvalidCastException>(() => set.Cast<string>()).Message);
            }

            [Fact]
            public void With_derived_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeEntity));
                Assert.Equal(
                    Strings.DbEntity_BadTypeForCast("DbSet", "FakeDerivedEntity", "FakeEntity"),
                    Assert.Throws<InvalidCastException>(() => set.Cast<FakeDerivedEntity>()).Message);
            }

            [Fact]
            public void With_base_type_throws()
            {
                var set = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    }.Object.Owner.Set(typeof(FakeDerivedEntity));
                Assert.Equal(
                    Strings.DbEntity_BadTypeForCast("DbSet", "FakeEntity", "FakeDerivedEntity"),
                    Assert.Throws<InvalidCastException>(() => set.Cast<FakeEntity>()).Message);
            }

            [Fact]
            public void With_same_type_returns_generic_DbSet()
            {
                var internalContextMock = new Mock<InternalContextForMock>
                    {
                        CallBase = true
                    };

                var internalSetMock = new Mock<InternalSet<FakeEntity>>(internalContextMock.Object);
                internalSetMock.Setup(m => m.InternalContext).Returns(internalContextMock.Object);

                var set = new InternalDbSet<FakeEntity>(internalSetMock.Object);

                Assert.IsType<DbSet<FakeEntity>>(set.Cast<FakeEntity>());
            }
        }

        public class SqlQuery_Generic
        {
            [Fact]
            public void With_null_SQL_string_throws()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(null)).Message);
            }

            [Fact]
            public void With_empty_SQL_string_throws()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_string_throws()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(" ")).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => set.SqlQuery("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                var query = set.SqlQuery("query");

                Assert.NotNull(query);
                Assert.Null(query.InternalQuery.Streaming);
            }
        }

        public class SqlQuery_NonGeneric
        {
            [Fact]
            public void With_empty_SQL_string_throws()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery("")).Message);
            }

            [Fact]
            public void With_whitespace_SQL_string_throws()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(" ")).Message);
            }

            [Fact]
            public void With_null_SQL_string_throws()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal(Strings.ArgumentIsNullOrWhitespace("sql"), Assert.Throws<ArgumentException>(() => set.SqlQuery(null)).Message);
            }

            [Fact]
            public void With_null_parameters_throws()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                Assert.Equal("parameters", Assert.Throws<ArgumentNullException>(() => set.SqlQuery("query", null)).ParamName);
            }

            [Fact]
            public void With_valid_arguments_doesnt_throw()
            {
                var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);

                var query = set.SqlQuery("query");

                Assert.NotNull(query);
                Assert.Null(query.InternalQuery.Streaming);
            }
        }

        internal class InternalSetForTests
            : InternalSet<FakeEntity>
        {
            public InternalSetForTests()
                : base(new Mock<InternalContextForMock>(MockBehavior.Loose).Object)
            {
            }

            public InternalSetForTests(InternalContext internalContext)
                : base(internalContext)
            {
            }

            public override void Initialize()
            {
            }
        }
    }
}
