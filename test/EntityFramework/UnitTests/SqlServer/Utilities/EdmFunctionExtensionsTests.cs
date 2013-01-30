// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class EdmFunctionExtensionsTests
    {
        [Fact]
        public void IsCSpace_returns_false_if_MetadataProperties_contains_no_data_space_property()
        {
            var mockEdmFunction = new Mock<EdmFunction>();
            mockEdmFunction.Setup(m => m.MetadataProperties).Returns(new ReadOnlyMetadataCollection<MetadataProperty>());

            Assert.False(mockEdmFunction.Object.IsCSpace());
        }

        [Fact]
        public void IsCSpace_returns_false_if_MetadataProperties_contains_non_c_space_data_space_property()
        {
            Assert.False(CreateMockEdmFunction(DataSpace.OCSpace, null).Object.IsCSpace());
        }

        [Fact]
        public void IsCSpace_returns_true_if_MetadataProperties_contains_c_space_data_space_property()
        {
            Assert.True(CreateMockEdmFunction(DataSpace.CSpace, null).Object.IsCSpace());
        }

        [Fact]
        public void IsCanonicalFunction_returns_true_if_function_is_in_c_space_and_has_Edm_namespace()
        {
            Assert.True(CreateMockEdmFunction(DataSpace.CSpace, "Edm").Object.IsCanonicalFunction());
        }

        [Fact]
        public void IsCanonicalFunction_returns_false_if_function_is_in_c_space_but_not_in_Edm_namespace()
        {
            Assert.False(CreateMockEdmFunction(DataSpace.CSpace, "Sql").Object.IsCanonicalFunction());
        }

        [Fact]
        public void IsCanonicalFunction_returns_false_if_function_is_in_Edm_namespace_but_not_in_c_space()
        {
            Assert.False(CreateMockEdmFunction(DataSpace.SSpace, "Edm").Object.IsCanonicalFunction());
        }

        private static Mock<EdmFunction> CreateMockEdmFunction(DataSpace dataSpace, string namespaceName)
        {
            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.Name).Returns("DataSpace");
            mockProperty.Setup(m => m.Value).Returns(dataSpace);

            var mockEdmFunction = new Mock<EdmFunction>();
            mockEdmFunction.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockProperty.Object }));
            mockEdmFunction.Setup(m => m.NamespaceName).Returns(namespaceName);

            return mockEdmFunction;
        }
    }
}
