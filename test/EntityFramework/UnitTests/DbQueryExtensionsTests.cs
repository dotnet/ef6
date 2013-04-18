// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DbQueryExtensionsTests
    {
        #region Entity types for Include tests

        public class RootEntity
        {
            public Level1Entity Level1Reference { get; set; }
            public ICollection<Level1Entity> Level1Collection { get; set; }
            public ICollection<RootEntity> RootCollection { get; set; }
            public RootEntity RootReference { get; set; }
        }

        public class Level1Entity
        {
            public bool BoolProperty { get; set; }
            public Level2Entity Level2Reference { get; set; }
            public ICollection<Level2Entity> Level2Collection { get; set; }
            public ICollection<RootEntity> RootCollection { get; set; }
            public RootEntity RootReference { get; set; }
        }

        public class Level2Entity
        {
            public Level3Entity Level3Reference { get; set; }
            public ICollection<Level3Entity> Level3Collection { get; set; }
            public ICollection<Level1Entity> Level1Collection { get; set; }
            public Level1Entity Level1Reference { get; set; }
        }

        public class Level3Entity
        {
            public ICollection<Level2Entity> Level2Collection { get; set; }
            public Level2Entity Level2Reference { get; set; }
        }

        #endregion

        public class AsNotracking_Generic
        {
            [Fact]
            public void With_null_source_called_on_extension_method_throws()
            {
                Assert.Equal(
                    "source", Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.AsNoTracking<FakeEntity>(null)).ParamName);
            }

            [Fact]
            public void On_ObjectQuery_returns_a_NoTracking_one()
            {
                var query = MockHelper.CreateMockObjectQuery(new object()).Object;

                var newQuery = query.AsNoTracking();

                Assert.NotSame(query, newQuery);
                Assert.NotEqual(MergeOption.NoTracking, query.MergeOption);
                Assert.Equal(MergeOption.NoTracking, ((ObjectQuery<object>)newQuery).MergeOption);
            }

            [Fact]
            public void On_IEnumerable_does_nothing()
            {
                var enumerable = new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterNoTracking = enumerable.AsNoTracking();

                Assert.Same(enumerable, afterNoTracking);
                Assert.Equal(3, afterNoTracking.Count());
            }

            [Fact]
            public void On_IQueryable_with_no_AsNoTracking_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable<FakeEntity>>().Object;
                var afterAsNoTracking = mockQueryable.AsNoTracking();

                Assert.Same(mockQueryable, afterAsNoTracking);
            }

            public interface INoTrackingable<T> : IQueryable<T>
            {
                INoTrackingable<T> AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_AsNoTracking_method_calls_that_method()
            {
                var mockQueryable = new Mock<INoTrackingable<FakeEntity>>(MockBehavior.Strict);
                IQueryable<FakeEntity> source = mockQueryable.Object;
                var result = new Mock<INoTrackingable<FakeEntity>>().Object;
                mockQueryable.Setup(i => i.AsNoTracking()).Returns(result);

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(result, afterAsNoTracking);
            }

            public interface INoTrackingableWithFunnyAsNoTracking<T> : IQueryable<T>
            {
                INoTrackingableWithFunnyAsNoTracking<T> AsNoTracking(string buffy, string summers);
            }

            [Fact]
            public void On_IQueryable_with_non_matching_AsNoTracking_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<INoTrackingableWithFunnyAsNoTracking<FakeEntity>>(MockBehavior.Strict).Object;

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(source, afterAsNoTracking);
            }

            public interface INoTrackingableReturningVoid<T> : IQueryable<T>
            {
                void AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_void_AsNoTracking_method_is_ignored()
            {
                var mockQueryable = new Mock<INoTrackingableReturningVoid<FakeEntity>>(MockBehavior.Strict);

                var afterAsNoTracking = ((IQueryable<FakeEntity>)mockQueryable.Object).AsNoTracking();

                Assert.Same(mockQueryable.Object, afterAsNoTracking);
            }

            public interface INoTrackingableReturningString<T> : IQueryable<T>
            {
                string AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_AsNoTracking_returning_string_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<INoTrackingableReturningString<FakeEntity>>(MockBehavior.Strict).Object;

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(source, afterAsNoTracking);
            }
        }

        public class AsNotracking_NonGeneric
        {
            [Fact]
            public void With_null_source_called_on_extension_method_throws()
            {
                Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.AsNoTracking(null)).ParamName);
            }

            [Fact]
            public void On_ObjectQuery_returns_a_NoTracking_one()
            {
                var query = (ObjectQuery)MockHelper.CreateMockObjectQuery(new object()).Object;

                var newQuery = query.AsNoTracking();

                Assert.NotSame(query, newQuery);
                Assert.NotEqual(MergeOption.NoTracking, query.MergeOption);
                Assert.Equal(MergeOption.NoTracking, ((ObjectQuery)newQuery).MergeOption);
            }

            [Fact]
            public void On_IEnumerable_does_nothing()
            {
                var enumerable = (IQueryable)new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterAsNoTracking = enumerable.AsNoTracking();

                Assert.Same(enumerable, afterAsNoTracking);
                Assert.Equal(3, afterAsNoTracking.ToList<FakeEntity>().Count());
            }

            [Fact]
            public void On_IQueryable_with_no_AsNoTracking_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable>().Object;
                var afterAsNoTracking = mockQueryable.AsNoTracking();

                Assert.Same(mockQueryable, afterAsNoTracking);
            }

            public interface INoTrackingable : IQueryable
            {
                INoTrackingable AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_AsNoTracking_method_calls_that_method()
            {
                var mockQueryable = new Mock<INoTrackingable>(MockBehavior.Strict);
                IQueryable source = mockQueryable.Object;
                var result = new Mock<INoTrackingable>().Object;
                mockQueryable.Setup(i => i.AsNoTracking()).Returns(result);

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(result, afterAsNoTracking);
            }

            public interface INoTrackingableWithFunnyAsNoTracking : IQueryable
            {
                INoTrackingableWithFunnyAsNoTracking AsNoTracking(string buffy, string summers);
            }

            [Fact]
            public void On_IQueryable_with_non_matching_AsNoTracking_is_ignored()
            {
                IQueryable source = new Mock<INoTrackingableWithFunnyAsNoTracking>(MockBehavior.Strict).Object;

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(source, afterAsNoTracking);
            }

            public interface INoTrackingableReturningVoid : IQueryable
            {
                void AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_void_AsNoTracking_method_is_ignored()
            {
                var mockQueryable = new Mock<INoTrackingableReturningVoid>(MockBehavior.Strict);

                var afterAsNoTracking = ((IQueryable)mockQueryable.Object).AsNoTracking();

                Assert.Same(mockQueryable.Object, afterAsNoTracking);
            }

            public interface INoTrackingableReturningString : IQueryable
            {
                string AsNoTracking();
            }

            [Fact]
            public void On_IQueryable_with_AsNoTracking_returning_string_is_ignored()
            {
                IQueryable source = new Mock<INoTrackingableReturningString>(MockBehavior.Strict).Object;

                var afterAsNoTracking = source.AsNoTracking();

                Assert.Same(source, afterAsNoTracking);
            }
        }

        public class AsStreaming_Generic
        {
            [Fact]
            public void With_null_source_called_on_extension_method_throws()
            {
                Assert.Equal(
                    "source", Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.AsStreaming<FakeEntity>(null)).ParamName);
            }

            [Fact]
            public void On_ObjectQuery_returns_an_AsStreaming_one()
            {
                var query = MockHelper.CreateMockObjectQuery(new object()).Object;

                var newQuery = query.AsStreaming();

                Assert.NotSame(query, newQuery);
                Assert.False(query.Streaming);
                Assert.True(((ObjectQuery<object>)newQuery).Streaming);
            }

            [Fact]
            public void On_IEnumerable_does_nothing()
            {
                var enumerable = new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterAsStreaming = enumerable.AsStreaming();

                Assert.Same(enumerable, afterAsStreaming);
                Assert.Equal(3, afterAsStreaming.Count());
            }

            [Fact]
            public void On_IQueryable_with_no_AsStreaming_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable<FakeEntity>>().Object;
                var afterAsStreaming = mockQueryable.AsStreaming();

                Assert.Same(mockQueryable, afterAsStreaming);
            }

            public interface IAsStreamable<T> : IQueryable<T>
            {
                IAsStreamable<T> AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_AsStreaming_method_calls_that_method()
            {
                var mockQueryable = new Mock<IAsStreamable<FakeEntity>>(MockBehavior.Strict);
                IQueryable<FakeEntity> source = mockQueryable.Object;
                var result = new Mock<IAsStreamable<FakeEntity>>().Object;
                mockQueryable.Setup(i => i.AsStreaming()).Returns(result);

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(result, afterAsStreaming);
            }

            public interface IAsStreamableWithFunnyAsStreaming<T> : IQueryable<T>
            {
                IAsStreamableWithFunnyAsStreaming<T> AsStreaming(string buffy, string summers);
            }

            [Fact]
            public void On_IQueryable_with_non_matching_AsStreaming_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<IAsStreamableWithFunnyAsStreaming<FakeEntity>>(MockBehavior.Strict).Object;

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(source, afterAsStreaming);
            }

            public interface IAsStreamableReturningVoid<T> : IQueryable<T>
            {
                void AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_void_AsStreaming_method_is_ignored()
            {
                var mockQueryable = new Mock<IAsStreamableReturningVoid<FakeEntity>>(MockBehavior.Strict);

                var afterAsStreaming = ((IQueryable<FakeEntity>)mockQueryable.Object).AsStreaming();

                Assert.Same(mockQueryable.Object, afterAsStreaming);
            }

            public interface IAsStreamableReturningString<T> : IQueryable<T>
            {
                string AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_AsStreaming_returning_string_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<IAsStreamableReturningString<FakeEntity>>(MockBehavior.Strict).Object;

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(source, afterAsStreaming);
            }
        }

        public class AsStreaming_NonGeneric
        {
            [Fact]
            public void With_null_source_called_on_extension_method_throws()
            {
                Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.AsStreaming(null)).ParamName);
            }

            [Fact]
            public void On_ObjectQuery_returns_an_AsStreaming_one()
            {
                var query = (ObjectQuery)MockHelper.CreateMockObjectQuery(new object()).Object;

                var newQuery = query.AsStreaming();

                Assert.NotSame(query, newQuery);
                Assert.False(query.Streaming);
                Assert.True(((ObjectQuery)newQuery).Streaming);
            }

            [Fact]
            public void On_IEnumerable_does_nothing()
            {
                var enumerable = (IQueryable)new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterAsStreaming = enumerable.AsStreaming();

                Assert.Same(enumerable, afterAsStreaming);
                Assert.Equal(3, afterAsStreaming.ToList<FakeEntity>().Count());
            }

            [Fact]
            public void On_IQueryable_with_no_AsStreaming_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable>().Object;
                var afterAsStreaming = mockQueryable.AsStreaming();

                Assert.Same(mockQueryable, afterAsStreaming);
            }

            public interface IStreamable : IQueryable
            {
                IStreamable AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_AsStreaming_method_calls_that_method()
            {
                var mockQueryable = new Mock<IStreamable>(MockBehavior.Strict);
                IQueryable source = mockQueryable.Object;
                var result = new Mock<IStreamable>().Object;
                mockQueryable.Setup(i => i.AsStreaming()).Returns(result);

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(result, afterAsStreaming);
            }

            public interface IAsStreamableWithFunnyAsStreaming : IQueryable
            {
                IAsStreamableWithFunnyAsStreaming AsStreaming(string buffy, string summers);
            }

            [Fact]
            public void On_IQueryable_with_non_matching_AsStreaming_is_ignored()
            {
                IQueryable source = new Mock<IAsStreamableWithFunnyAsStreaming>(MockBehavior.Strict).Object;

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(source, afterAsStreaming);
            }

            public interface IAsStreamableReturningVoid : IQueryable
            {
                void AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_void_AsStreaming_method_is_ignored()
            {
                var mockQueryable = new Mock<IAsStreamableReturningVoid>(MockBehavior.Strict);

                var afterAsStreaming = ((IQueryable)mockQueryable.Object).AsStreaming();

                Assert.Same(mockQueryable.Object, afterAsStreaming);
            }

            public interface IAsStreamableReturningString : IQueryable
            {
                string AsStreaming();
            }

            [Fact]
            public void On_IQueryable_with_AsStreaming_returning_string_is_ignored()
            {
                IQueryable source = new Mock<IAsStreamableReturningString>(MockBehavior.Strict).Object;

                var afterAsStreaming = source.AsStreaming();

                Assert.Same(source, afterAsStreaming);
            }
        }

        public class Include_Generic
        {
            [Fact]
            public void String_Include_with_null_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), null)).Message);
            }

            [Fact]
            public void String_Include_with_empty_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), "")).Message);
            }

            [Fact]
            public void String_Include_with_whitespace_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), " ")).Message);
            }

            [Fact]
            public void String_Include_with_null_source_called_on_extension_method_throws()
            {
                Assert.Equal(
                    "source", Assert.Throws<ArgumentNullException>(
                        () =>
                        DbQueryExtensions.Include<FakeEntity>(null, "SomePath")).ParamName);
            }

            [Fact]
            public void String_Include_on_IEnumerable_does_nothing()
            {
                var enumerable = new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterInclude = enumerable.Include("FakeRelationship");

                Assert.Same(enumerable, afterInclude);
                Assert.Equal(3, afterInclude.Count());
            }

            [Fact]
            public void String_Include_on_IQueryable_with_no_Include_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable<FakeEntity>>().Object;
                var afterInclude = mockQueryable.Include("FakeRelationship");

                Assert.Same(mockQueryable, afterInclude);
            }

            public interface IIncludable<T> : IQueryable<T>
            {
                IIncludable<T> Include(string path);
            }

            [Fact]
            public void String_Include_on_IQueryable_with_Include_method_calls_that_method()
            {
                var mockQueryable = new Mock<IIncludable<FakeEntity>>(MockBehavior.Strict);
                IQueryable<FakeEntity> source = mockQueryable.Object;
                var result = new Mock<IIncludable<FakeEntity>>().Object;
                mockQueryable.Setup(i => i.Include("FakeRelationship")).Returns(result);

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(result, afterInclude);
            }

            public interface IIncludableWithFunnyInclude<T> : IQueryable<T>
            {
                IIncludableWithFunnyInclude<T> Include(string buffy, string summers);
            }

            [Fact]
            public void String_Include_on_IQueryable_with_non_matching_Include_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<IIncludableWithFunnyInclude<FakeEntity>>(MockBehavior.Strict).Object;

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(source, afterInclude);
            }

            public interface IIncludableReturningVoid<T> : IQueryable<T>
            {
                void Include(string path);
            }

            [Fact]
            public void String_Include_on_IQueryable_with_void_Include_method_is_ignored()
            {
                var mockQueryable = new Mock<IIncludableReturningVoid<FakeEntity>>(MockBehavior.Strict);

                var afterInclude = ((IQueryable<FakeEntity>)mockQueryable.Object).Include("FakeRelationship");

                Assert.Same(mockQueryable.Object, afterInclude);
            }

            public interface IIncludableReturningString<T> : IQueryable<T>
            {
                string Include(string path);
            }

            [Fact]
            public void String_Include_on_IQueryable_with_Include_returning_string_is_ignored()
            {
                IQueryable<FakeEntity> source = new Mock<IIncludableReturningString<FakeEntity>>(MockBehavior.Strict).Object;

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(source, afterInclude);
            }

            [Fact]
            public void Lambda_Include_can_parse_single_reference_property()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference);

                mockQueryable.Verify(i => i.Include("Level1Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_single_collection_property()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection);

                mockQueryable.Verify(i => i.Include("Level1Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Reference);

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Collection);

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_reference_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Reference.Level3Reference);

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Reference.Level3Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_reference_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Reference.Level3Collection);

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Reference.Level3Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_collection_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Collection.Select(l2 => l2.Level3Reference));

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Collection.Level3Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_reference_followed_by_collection_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Reference.Level2Collection.Select(l2 => l2.Level3Collection));

                mockQueryable.Verify(i => i.Include("Level1Reference.Level2Collection.Level3Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_reference_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference.Level3Reference));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Reference.Level3Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_reference_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Reference.Level3Collection));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Reference.Level3Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_collection_followed_by_reference()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Select(l2 => l2.Level3Reference)));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Collection.Level3Reference"));
            }

            [Fact]
            public void Lambda_Include_can_parse_collection_followed_by_collection_followed_by_collection()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Select(l2 => l2.Level3Collection)));

                mockQueryable.Verify(i => i.Include("Level1Collection.Level2Collection.Level3Collection"));
            }

            [Fact]
            public void Lambda_Include_can_parse_a_huge_funky_include_path()
            {
                var mockQueryable = new Mock<IIncludable<RootEntity>>();

                mockQueryable.Object.Include(
                    e =>
                    e.RootReference.RootCollection.Select(
                        r =>
                        r.Level1Reference.RootReference.Level1Collection.Select(
                            l1 =>
                            l1.Level2Collection.Select(
                                l2 => l2.Level3Collection.Select(l3 => l3.Level2Reference.Level1Collection.Select(l1b => l1b.RootReference))))));

                mockQueryable.Verify(
                    i =>
                    i.Include(
                        "RootReference.RootCollection.Level1Reference.RootReference.Level1Collection.Level2Collection.Level3Collection.Level2Reference.Level1Collection.RootReference"));
            }

            [Fact]
            public void Lambda_Include_with_null_source_called_on_extension_method_throws()
            {
                Assert.Equal(
                    "source",
                    Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.Include<FakeEntity, int>(null, e => e.Id)).ParamName);
            }

            [Fact]
            public void Lambda_Include_with_null_expression_called_on_extension_method_throws()
            {
                Assert.Equal(
                    "path",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(
                            (Expression<Func<RootEntity, object>>)null)).ParamName);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_fundamentaly_wrong_expression()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(e => new object())).Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_method_call_expression()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(e => e.GetType())).Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_second_level_method_call_expression()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(e => e.Level1Reference.GetType()))
                          .Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_first_level_method_call_with_second_level_property()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(
                            e => e.Level1Reference.GetType().Assembly)).Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_call_to_something_other_than_Select()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object).Include(e => e.Level1Collection.First()))
                          .Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_second_level_call_to_something_other_than_Select()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object)
                            .Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Distinct()))).Message);
            }

            [Fact]
            public void Lambda_Include_throws_when_given_first_level_call_to_something_other_than_Select_containing_expression()
            {
                Assert.Equal(
                    new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path").Message,
                    Assert.Throws<ArgumentException>(
                        () =>
                        new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object)
                            .Include(e => e.Level1Collection.Any(l1 => l1.BoolProperty))).Message);
            }
        }

        public class Include_NonGeneric
        {
            [Fact]
            public void Non_generic_String_Include_with_null_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new InternalDbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), null))
                          .Message);
            }

            [Fact]
            public void Non_generic_String_Include_with_empty_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new InternalDbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), ""))
                          .Message);
            }

            [Fact]
            public void Non_generic_String_Include_with_whitespace_string_called_on_extension_method_throws()
            {
                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("path"),
                    Assert.Throws<ArgumentException>(
                        () =>
                        DbQueryExtensions.Include(new InternalDbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object), " "))
                          .Message);
            }

            [Fact]
            public void Non_generic_String_Include_with_null_source_called_on_extension_method_throws()
            {
                Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => DbQueryExtensions.Include(null, "SomePath")).ParamName);
            }

            [Fact]
            public void Non_generic_String_Include_on_IEnumerable_does_nothing()
            {
                var enumerable = (IQueryable)new List<FakeEntity>
                    {
                        new FakeEntity(),
                        new FakeEntity(),
                        new FakeEntity()
                    }.AsQueryable();
                var afterInclude = enumerable.Include("FakeRelationship");

                Assert.Same(enumerable, afterInclude);
                Assert.Equal(3, afterInclude.ToList<FakeEntity>().Count());
            }

            [Fact]
            public void Non_generic_String_Include_on_IQueryable_with_no_Include_method_does_nothing()
            {
                var mockQueryable = new Mock<IQueryable>().Object;
                var afterInclude = mockQueryable.Include("FakeRelationship");

                Assert.Same(mockQueryable, afterInclude);
            }

            public interface IIncludable : IQueryable
            {
                IIncludable Include(string path);
            }

            [Fact]
            public void Non_generic_String_Include_on_IQueryable_with_Include_method_calls_that_method()
            {
                var mockQueryable = new Mock<IIncludable>(MockBehavior.Strict);
                IQueryable source = mockQueryable.Object;
                var result = new Mock<IIncludable>().Object;
                mockQueryable.Setup(i => i.Include("FakeRelationship")).Returns(result);

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(result, afterInclude);
            }

            public interface IIncludableWithFunnyInclude : IQueryable
            {
                IIncludableWithFunnyInclude Include(string buffy, string summers);
            }

            [Fact]
            public void Non_generic_String_Include_on_IQueryable_with_non_matching_Include_is_ignored()
            {
                IQueryable source = new Mock<IIncludableWithFunnyInclude>(MockBehavior.Strict).Object;

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(source, afterInclude);
            }

            public interface IIncludableReturningVoid : IQueryable
            {
                void Include(string path);
            }

            [Fact]
            public void Non_generic_String_Include_on_IQueryable_with_void_Include_method_is_ignored()
            {
                var mockQueryable = new Mock<IIncludableReturningVoid>(MockBehavior.Strict);

                var afterInclude = ((IQueryable)mockQueryable.Object).Include("FakeRelationship");

                Assert.Same(mockQueryable.Object, afterInclude);
            }

            public interface IIncludableReturningString : IQueryable
            {
                string Include(string path);
            }

            [Fact]
            public void Non_generic_String_Include_on_IQueryable_with_Include_returning_string_is_ignored()
            {
                IQueryable source = new Mock<IIncludableReturningString>(MockBehavior.Strict).Object;

                var afterInclude = source.Include("FakeRelationship");

                Assert.Same(source, afterInclude);
            }
        }

        public class TryGetObjectQuery
        {
            [Fact]
            public void TryGetObjectQuery_returns_null_when_given_null()
            {
                Assert.Null(((IQueryable)null).TryGetObjectQuery());
            }

            [Fact]
            public void TryGetObjectQuery_returns_null_when_given_queryable_that_is_not_DbQuery_or_ObjectQuery()
            {
                Assert.Null(new List<int>().AsQueryable().TryGetObjectQuery());
            }

            [Fact]
            public void TryGetObjectQuery_returns_ObjectQuery_when_given_generic_ObjectQuery()
            {
                var query = new Mock<ObjectQuery<int>>().Object;

                Assert.Same(query, query.TryGetObjectQuery());
            }

            [Fact]
            public void TryGetObjectQuery_returns_ObjectQuery_when_given_non_generic_ObjectQuery()
            {
                var query = new Mock<ObjectQuery>().Object;

                Assert.Same(query, query.TryGetObjectQuery());
            }

            [Fact]
            public void TryGetObjectQuery_returns_ObjectQuery_when_given_generic_DbQuery()
            {
                var objectQuery = new Mock<ObjectQuery<int>>().Object;
                var mockInternalQuery = new Mock<IInternalQuery<int>>();
                mockInternalQuery.Setup(m => m.ObjectQuery).Returns(objectQuery);

                Assert.Same(objectQuery, new DbQuery<int>(mockInternalQuery.Object).TryGetObjectQuery());
            }

            [Fact]
            public void TryGetObjectQuery_returns_ObjectQuery_when_given_non_generic_DbQuery()
            {
                var objectQuery = new Mock<ObjectQuery<int>>().Object;
                var mockInternalQuery = new Mock<IInternalQuery<int>>();
                mockInternalQuery.Setup(m => m.ObjectQuery).Returns(objectQuery);

                var mockDbQuery = new Mock<DbQuery>();
                mockDbQuery.Setup(m => m.InternalQuery).Returns(mockInternalQuery.Object);

                Assert.Same(objectQuery, mockDbQuery.Object.TryGetObjectQuery());
            }
        }

        #region Async equivalents of IQueryable extension methods

#if !NET40

        [Fact]
        public void Extension_methods_validate_arguments()
        {
            ArgumentNullTest("source", () => DbQueryExtensions.FirstAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().FirstAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.FirstOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.FirstOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().FirstOrDefaultAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.SingleAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().SingleAsync(null));
            ArgumentNullTest("predicate", () => Source().SingleAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.SingleOrDefaultAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleOrDefaultAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleOrDefaultAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.SingleOrDefaultAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null));
            ArgumentNullTest("predicate", () => Source().SingleOrDefaultAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.ContainsAsync(null, 1));
            ArgumentNullTest("source", () => DbQueryExtensions.ContainsAsync(null, 1, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.AnyAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.AnyAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.AnyAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.AnyAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().AnyAsync(null));
            ArgumentNullTest("predicate", () => Source().AnyAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.AllAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.AllAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().AllAsync(null));
            ArgumentNullTest("predicate", () => Source().AllAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.CountAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.CountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.CountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.CountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().CountAsync(null));
            ArgumentNullTest("predicate", () => Source().CountAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.LongCountAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.LongCountAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.LongCountAsync<int>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.LongCountAsync<int>(null, s => true, new CancellationToken()));
            ArgumentNullTest("predicate", () => Source().LongCountAsync(null));
            ArgumentNullTest("predicate", () => Source().LongCountAsync(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.MinAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.MinAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.MinAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.MinAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null));
            ArgumentNullTest("selector", () => Source().MinAsync<int, bool>(null, new CancellationToken()));

            ArgumentNullTest("source", () => DbQueryExtensions.MaxAsync<int>(null));
            ArgumentNullTest("source", () => DbQueryExtensions.MaxAsync<int>(null, new CancellationToken()));
            ArgumentNullTest("source", () => DbQueryExtensions.MaxAsync<int, bool>(null, s => true));
            ArgumentNullTest("source", () => DbQueryExtensions.MaxAsync<int, bool>(null, s => true, new CancellationToken()));
            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null));
            ArgumentNullTest("selector", () => Source().MaxAsync<int, bool>(null, new CancellationToken()));

            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).SumAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => Source<int>().SumAsync((Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => Source<int?>().SumAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => Source<long>().SumAsync((Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => Source<long?>().SumAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => Source<float>().SumAsync((Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => Source<float?>().SumAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => Source<double>().SumAsync((Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null));
            ArgumentNullTest("selector", () => Source<double?>().SumAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null));
            ArgumentNullTest("selector", () => Source<decimal>().SumAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null));
            ArgumentNullTest("selector", () => Source<decimal?>().SumAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));

            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync());
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<int?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<long?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<float?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<double?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0));
            ArgumentNullTest("source", () => ((IQueryable<decimal?>)null).AverageAsync(i => 0, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null));
            ArgumentNullTest("selector", () => Source<int>().AverageAsync((Expression<Func<int, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null));
            ArgumentNullTest("selector", () => Source<int?>().AverageAsync((Expression<Func<int?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null));
            ArgumentNullTest("selector", () => Source<long>().AverageAsync((Expression<Func<long, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null));
            ArgumentNullTest("selector", () => Source<long?>().AverageAsync((Expression<Func<long?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null));
            ArgumentNullTest("selector", () => Source<float>().AverageAsync((Expression<Func<float, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null));
            ArgumentNullTest("selector", () => Source<float?>().AverageAsync((Expression<Func<float?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null));
            ArgumentNullTest("selector", () => Source<double>().AverageAsync((Expression<Func<double, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<double?>().AverageAsync((Expression<Func<double?, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<decimal>().AverageAsync((Expression<Func<decimal, int>>)null, new CancellationToken()));
            ArgumentNullTest("selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null));
            ArgumentNullTest(
                "selector", () => Source<decimal?>().AverageAsync((Expression<Func<decimal?, int>>)null, new CancellationToken()));
        }

        [Fact]
        public void Extension_methods_throw_on_non_async_source()
        {
            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().AllAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().AnyAsync());
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().AnyAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double?>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal>().AverageAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().AverageAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0));
            SourceNonAsyncQueryableTest(() => Source().ContainsAsync(0, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().CountAsync());
            SourceNonAsyncQueryableTest(() => Source().CountAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().CountAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().FirstAsync());
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().FirstAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync());
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().FirstOrDefaultAsync(e => true, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ForEachAsync(e => e.GetType(), new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => Source().LoadAsync());
            SourceNonAsyncEnumerableTest(() => Source().LoadAsync(new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().LongCountAsync());
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().LongCountAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().MaxAsync());
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source().MaxAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().MinAsync());
            SourceNonAsyncQueryableTest(() => Source().MinAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source().MinAsync(e => e, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().SingleAsync());
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().SingleAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync());
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true));
            SourceNonAsyncQueryableTest(() => Source().SingleOrDefaultAsync(e => true, new CancellationToken()));

            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<int?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<long?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<float?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<double?>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal>().SumAsync(e => e, new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync());
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(new CancellationToken()));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e));
            SourceNonAsyncQueryableTest(() => Source<decimal?>().SumAsync(e => e, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToArrayAsync());
            SourceNonAsyncEnumerableTest<int>(() => Source().ToArrayAsync(new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e));
            SourceNonAsyncEnumerableTest<int>(() => Source().ToDictionaryAsync(e => e, e => e, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(
                () => Source().ToDictionaryAsync(
                    e => e,
                    new Mock<IEqualityComparer<int>>().Object));
            SourceNonAsyncEnumerableTest<int>(
                () => Source().ToDictionaryAsync(
                    e => e,
                    new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));
            SourceNonAsyncEnumerableTest<int>(
                () => Source().ToDictionaryAsync(
                    e => e, e => e,
                    new Mock<IEqualityComparer<int>>().Object));
            SourceNonAsyncEnumerableTest<int>(
                () => Source().ToDictionaryAsync(
                    e => e, e => e,
                    new Mock<IEqualityComparer<int>>().Object, new CancellationToken()));

            SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync());
            SourceNonAsyncEnumerableTest<int>(() => Source().ToListAsync(new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType()));
            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ForEachAsync(e => e.GetType(), new CancellationToken()));

            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ToListAsync());
            SourceNonAsyncEnumerableTest(() => ((IQueryable)Source()).ToListAsync(new CancellationToken()));
        }

        [Fact]
        public void Extension_methods_call_provider_ExecuteAsync()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true));
            VerifyProducedExpression<int, bool>(value => value.AllAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, bool>(value => value.AnyAsync());
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true));
            VerifyProducedExpression<int, bool>(value => value.AnyAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, double>(value => value.AverageAsync());
            VerifyProducedExpression<int, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<int, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<int?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync());
            VerifyProducedExpression<long, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<long, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<long?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync());
            VerifyProducedExpression<float, float>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<float, float>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync());
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<float?, float?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync());
            VerifyProducedExpression<double, double>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<double, double>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync());
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<double?, double?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync());
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<decimal, decimal>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync());
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e));
            VerifyProducedExpression<decimal?, decimal?>(value => value.AverageAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0));
            VerifyProducedExpression<int, bool>(value => value.ContainsAsync(0, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.CountAsync());
            VerifyProducedExpression<int, int>(value => value.CountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.CountAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.FirstAsync());
            VerifyProducedExpression<int, int>(value => value.FirstAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.FirstAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync());
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.FirstOrDefaultAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, long>(value => value.LongCountAsync());
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true));
            VerifyProducedExpression<int, long>(value => value.LongCountAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.MaxAsync());
            VerifyProducedExpression<int, int>(value => value.MaxAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.MaxAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.MinAsync());
            VerifyProducedExpression<int, int>(value => value.MinAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.MinAsync(e => e, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleAsync());
            VerifyProducedExpression<int, int>(value => value.SingleAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.SingleAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync());
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true));
            VerifyProducedExpression<int, int>(value => value.SingleOrDefaultAsync(e => true, cancellationTokenSource.Token));

            VerifyProducedExpression<int, int>(value => value.SumAsync());
            VerifyProducedExpression<int, int>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e));
            VerifyProducedExpression<int, int>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync());
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<int?, int?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync());
            VerifyProducedExpression<long, long>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e));
            VerifyProducedExpression<long, long>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync());
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<long?, long?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync());
            VerifyProducedExpression<float, float>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e));
            VerifyProducedExpression<float, float>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync());
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<float?, float?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync());
            VerifyProducedExpression<double, double>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e));
            VerifyProducedExpression<double, double>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync());
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<double?, double?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync());
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e));
            VerifyProducedExpression<decimal, decimal>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync());
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(cancellationTokenSource.Token));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e));
            VerifyProducedExpression<decimal?, decimal?>(value => value.SumAsync(e => e, cancellationTokenSource.Token));
        }

        private static IQueryable<T> Source<T>()
        {
            return new Mock<IQueryable<T>>().Object;
        }

        private static IQueryable<int> Source()
        {
            return Source<int>();
        }

        private static void ArgumentNullTest(string paramName, Action test)
        {
            Assert.Equal(paramName, Assert.Throws<ArgumentNullException>(() => test()).ParamName);
        }

        private static void SourceNonAsyncQueryableTest(Action test)
        {
            Assert.Equal(Strings.IQueryable_Provider_Not_Async, Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void SourceNonAsyncEnumerableTest(Action test)
        {
            Assert.Equal(Strings.IQueryable_Not_Async(string.Empty), Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void SourceNonAsyncEnumerableTest<T>(Action test)
        {
            Assert.Equal(
                Strings.IQueryable_Not_Async("<" + typeof(T) + ">"), Assert.Throws<InvalidOperationException>(() => test()).Message);
        }

        private static void VerifyProducedExpression<TElement, TResult>(
            Expression<Func<IQueryable<TElement>, Task<TResult>>> testExpression)
        {
            var queryableMock = new Mock<IQueryable<TElement>>();
            var providerMock = new Mock<IDbAsyncQueryProvider>();
            providerMock.Setup(m => m.ExecuteAsync<TResult>(It.IsAny<Expression>(), It.IsAny<CancellationToken>()))
                        .Returns<Expression, CancellationToken>(
                            (e, ct) =>
                                {
                                    var expectedMethodCall = (MethodCallExpression)testExpression.Body;
                                    var actualMethodCall = (MethodCallExpression)e;

                                    Assert.Equal(
                                        expectedMethodCall.Method.Name,
                                        actualMethodCall.Method.Name + "Async");

                                    var lastArgument =
                                        expectedMethodCall.Arguments[expectedMethodCall.Arguments.Count - 1] as MemberExpression;

                                    var cancellationTokenPresent = lastArgument != null && lastArgument.Type == typeof(CancellationToken);

                                    if (cancellationTokenPresent)
                                    {
                                        Assert.NotEqual(ct, CancellationToken.None);
                                    }
                                    else
                                    {
                                        Assert.Equal(ct, CancellationToken.None);
                                    }

                                    var expectedNumberOfArguments = cancellationTokenPresent
                                                                        ? expectedMethodCall.Arguments.Count - 1
                                                                        : expectedMethodCall.Arguments.Count;
                                    Assert.Equal(expectedNumberOfArguments, actualMethodCall.Arguments.Count);
                                    for (var i = 1; i < expectedNumberOfArguments; i++)
                                    {
                                        var expectedArgument = expectedMethodCall.Arguments[i];
                                        var actualArgument = actualMethodCall.Arguments[i];
                                        Assert.Equal(expectedArgument.ToString(), actualArgument.ToString());
                                    }

                                    return Task.FromResult(default(TResult));
                                });

            queryableMock.Setup(m => m.Provider).Returns(providerMock.Object);

            queryableMock.Setup(m => m.Expression).Returns(Expression.Constant(queryableMock.Object, typeof(IQueryable<TElement>)));

            testExpression.Compile()(queryableMock.Object);
        }

#endif

        #endregion
    }
}
