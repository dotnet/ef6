// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Xml.Linq;
    using Moq;
    using Xunit;

    public class ModelCompatibilityCheckerTests
    {
        private static Mock<InternalContextForMock<DatabaseInitializerTests.FakeNoRegContext>> CreateContextForCompatibleTest(
            bool modelMatches, bool codeFirst = true)
        {
            var mockInternalContext = new Mock<InternalContextForMock<DatabaseInitializerTests.FakeNoRegContext>>();
            mockInternalContext.Setup(c => c.CodeFirstModel).Returns(codeFirst ? new DbCompiledModel() : null);
            mockInternalContext.Setup(c => c.QueryForModel(It.IsAny<DatabaseExistenceState>())).Returns(new VersionedModel(new XDocument()));
            mockInternalContext.Setup(c => c.ModelMatches(It.IsAny<VersionedModel>())).Returns(modelMatches);
            return mockInternalContext;
        }

        private static Mock<InternalContextForMock<DatabaseInitializerTests.FakeNoRegContext>> CreateContextForCompatibleTest(
            string databaseHash, bool codeFirst = true)
        {
            var mockInternalContext = new Mock<InternalContextForMock<DatabaseInitializerTests.FakeNoRegContext>>();
            mockInternalContext.Setup(c => c.CodeFirstModel).Returns(codeFirst ? new DbCompiledModel() : null);
            mockInternalContext.Setup(c => c.QueryForModel(It.IsAny<DatabaseExistenceState>())).Returns((VersionedModel)null);
            mockInternalContext.Setup(c => c.QueryForModelHash()).Returns(databaseHash);
            return mockInternalContext;
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: true);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: true);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.False(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(modelMatches: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.False(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_using_model_hash_check_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_matches_database_using_model_hash_check_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_false_if_model_does_not_match_database_using_model_hash_check_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash1>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash2>");

            Assert.False(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: true));
        }

        [Fact]
        public void
            CompatibleWithModel_returns_false_if_model_does_not_match_database_using_model_hash_check_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash1>");
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash2>");

            Assert.False(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_model_is_not_from_code_first_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>", codeFirst: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_returns_true_if_database_has_no_hash_and_method_is_asked_not_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(null);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.True(
                new ModelCompatibilityChecker()
                    .CompatibleWithModel(mockInternalContext.Object, mockHashFactory.Object, throwIfNoMetadata: false));
        }

        [Fact]
        public void CompatibleWithModel_throws_if_model_is_not_from_code_first_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest("<Hash>", codeFirst: false);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns((string)null);

            Assert.Equal(
                Strings.Database_NonCodeFirstCompatibilityCheck,
                Assert.Throws<NotSupportedException>(
                    () => new ModelCompatibilityChecker().CompatibleWithModel(
                        mockInternalContext.Object,
                        mockHashFactory.Object,
                        throwIfNoMetadata: true)).Message);
        }

        [Fact]
        public void CompatibleWithModel_throws_if_database_has_no_hash_and_method_is_asked_to_throw()
        {
            var mockInternalContext = CreateContextForCompatibleTest(null);
            var mockHashFactory = new Mock<ModelHashCalculator>();
            mockHashFactory.Setup(f => f.Calculate(It.IsAny<DbCompiledModel>())).Returns("<Hash>");

            Assert.Equal(
                Strings.Database_NoDatabaseMetadata,
                Assert.Throws<NotSupportedException>(
                    () => new ModelCompatibilityChecker().CompatibleWithModel(
                        mockInternalContext.Object,
                        mockHashFactory.Object,
                        throwIfNoMetadata: true)).Message);
        }
    }
}
