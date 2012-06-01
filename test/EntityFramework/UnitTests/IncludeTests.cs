namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the Include extension methods on IQueryable.
    /// </summary> 
    public class IncludeTests : TestBase
    {
        #region Some fake entities with pseudo-relationships

        private static readonly DbQuery<RootEntity> FakeDbQuery = new DbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object);
        private static readonly DbQuery FakeNonGenericDbQuery = new InternalDbQuery<RootEntity>(new Mock<IInternalQuery<RootEntity>>().Object);
        private static readonly ArgumentException BadLambdaException = new ArgumentException(Strings.DbExtensions_InvalidIncludePathExpression, "path");

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

        #region String include negative contract tests

        [Fact]
        public void String_Include_with_null_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(null)).Message);
        }

        [Fact]
        public void String_Include_with_empty_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeDbQuery.Include("")).Message);
        }

        [Fact]
        public void String_Include_with_whitespace_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(" ")).Message);
        }

        [Fact]
        public void String_Include_with_null_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeDbQuery, (string)null)).Message);
        }

        [Fact]
        public void String_Include_with_empty_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeDbQuery, "")).Message);
        }

        [Fact]
        public void String_Include_with_whitespace_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeDbQuery, " ")).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_null_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeNonGenericDbQuery.Include(null)).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_empty_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeNonGenericDbQuery.Include("")).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_whitespace_string_called_on_actual_DbQuery_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => FakeNonGenericDbQuery.Include(" ")).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_null_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeNonGenericDbQuery, (string)null)).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_empty_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeNonGenericDbQuery, "")).Message);
        }

        [Fact]
        public void Non_generic_String_Include_with_whitespace_string_called_on_extension_method_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("path"), Assert.Throws<ArgumentException>(() => IQueryableExtensions.Include(FakeNonGenericDbQuery, " ")).Message);
        }

        [Fact]
        public void Lambda_Include_with_null_expression_called_on_extension_method_throws()
        {
            Assert.Equal("path", Assert.Throws<ArgumentNullException>(() => FakeDbQuery.Include((Expression<Func<RootEntity, object>>)null)).ParamName);
        }

        [Fact]
        public void String_Include_with_null_source_called_on_extension_method_throws()
        {
            Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => IQueryableExtensions.Include<FakeEntity>(null, "SomePath")).ParamName);
        }

        [Fact]
        public void Non_generic_String_Include_with_null_source_called_on_extension_method_throws()
        {
            Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => IQueryableExtensions.Include(null, "SomePath")).ParamName);
        }

        [Fact]
        public void Lambda_Include_with_null_source_called_on_extension_method_throws()
        {
            Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => IQueryableExtensions.Include<FakeEntity, int>(null, e => e.Id)).ParamName);
        }

        #endregion

        #region Positive custom IQueryable Include(String) tests

        [Fact]
        public void String_Include_on_IEnumerable_does_nothing()
        {
            var enumerable = new List<FakeEntity> {new FakeEntity(), new FakeEntity(), new FakeEntity()}.AsQueryable();
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
        public void Non_generic_String_Include_on_IEnumerable_does_nothing()
        {
            var enumerable = (IQueryable)new List<FakeEntity> { new FakeEntity(), new FakeEntity(), new FakeEntity() }.AsQueryable();
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

            var afterInclude = ((IQueryable) mockQueryable.Object).Include("FakeRelationship");

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

        #endregion
        
        #region Positive Include(Lambda) expression parsing tests

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

            mockQueryable.Object.Include(e => e.RootReference.RootCollection.Select(r => r.Level1Reference.RootReference.Level1Collection.Select(l1 => l1.Level2Collection.Select(l2 => l2.Level3Collection.Select(l3 => l3.Level2Reference.Level1Collection.Select(l1b => l1b.RootReference))))));

            mockQueryable.Verify(i => i.Include("RootReference.RootCollection.Level1Reference.RootReference.Level1Collection.Level2Collection.Level3Collection.Level2Reference.Level1Collection.RootReference"));
        }

        #endregion

        #region Negative Include(Lambda) expression parsing tests

        [Fact]
        public void Lambda_Include_throws_when_given_fundamentaly_wrong_expression()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => new object())).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_method_call_expression()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.GetType())).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_second_level_method_call_expression()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.Level1Reference.GetType())).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_first_level_method_call_with_second_level_property()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.Level1Reference.GetType().Assembly)).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_call_to_something_other_than_Select()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.Level1Collection.First())).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_second_level_call_to_something_other_than_Select()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.Level1Collection.Select(l1 => l1.Level2Collection.Distinct()))).Message);
        }

        [Fact]
        public void Lambda_Include_throws_when_given_first_level_call_to_something_other_than_Select_containing_expression()
        {
            Assert.Equal(BadLambdaException.Message, Assert.Throws<ArgumentException>(() => FakeDbQuery.Include(e => e.Level1Collection.Any(l1 => l1.BoolProperty))).Message);
        }

        #endregion
    }
}