// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class ContextConfigTests
    {
        [Fact]
        public void TryGetCommandTimeout_returns_null_when_no_commandTimeout_is_set_for_context()
        {
            Assert.Null(
                new ContextConfig(new EntityFrameworkSection())
                    .TryGetCommandTimeout(typeof(DbContext)));
        }

        [Fact]
        public void TryGetCommandTimeout_returns_null_when_context_section_exists_but_no_commandTimeout_is_set()
        {
            Assert.Null(
                new ContextConfig(
                    CreateEfSection(
                        typeof(DbContext).AssemblyQualifiedName,
                        null))
                    .TryGetCommandTimeout(typeof(DbContext)));
        }

        [Fact]
        public void TryGetCommandTimeout_returns_commandTimeout_set_in_config_section()
        {
            Assert.Equal(66,
                new ContextConfig(
                    CreateEfSection(
                        typeof(DbContext).AssemblyQualifiedName,
                        66))
                    .TryGetCommandTimeout(typeof(DbContext)));
        }

        [Fact]
        public void TryGetCommandTimeout_throws_if_bad_context_type_with_commandTimeout_is_registered_in_context_section()
        {
            var contextConfig = new ContextConfig(
                CreateEfSection("A.Bad.Context.Type", 66));

            var exception = Assert.Throws<InvalidOperationException>(() => contextConfig.TryGetCommandTimeout(typeof(DbContext)));

            Assert.Equal(
                Strings.Database_InitializationException, exception.Message);

            Assert.IsType<TypeLoadException>(exception.InnerException);
        }


        private static EntityFrameworkSection CreateEfSection(string contextTypeName, int? commandTimeout = null)
        {
            var mockContextElement = new Mock<ContextElement>();
            mockContextElement.Setup(m => m.ContextTypeName).Returns(contextTypeName);
            mockContextElement.SetupGet(m => m.CommandTimeout).Returns(commandTimeout);

            var mockContextCollection = new Mock<ContextCollection>();
            mockContextCollection.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(
                new List<ContextElement>
                    {
                        mockContextElement.Object
                    }.GetEnumerator());

            var mockEfSection = new Mock<EntityFrameworkSection>();
            mockEfSection.Setup(m => m.Contexts).Returns(mockContextCollection.Object);

            return mockEfSection.Object;
        }
    }
}
