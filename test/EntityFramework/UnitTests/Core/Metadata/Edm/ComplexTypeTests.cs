// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class ComplexTypeTests
    {
        [Fact]
        public void Properties_list_should_be_live_on_reread()
        {
            var complexType = new ComplexType("C");

            Assert.Empty(complexType.Properties);

            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexType.AddMember(property);

            Assert.Equal(1, complexType.Properties.Count);
        }

        [Fact]
        public void Create_sets_properties_and_seals_the_type()
        {
            var complexType = ComplexType.Create(
                "foo",
                "bar",
                DataSpace.CSpace,
                new[]
                    {
                        EdmProperty.Primitive("prop1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                        EdmProperty.Primitive("prop2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))
                    },
                new[]
                    {
                        new MetadataProperty(
                            "TestProperty",
                            TypeUsage.CreateDefaultTypeUsage(
                                PrimitiveType.GetEdmPrimitiveType(
                                    PrimitiveTypeKind.String)),
                            "value"),
                    });

            Assert.Equal("bar.foo", complexType.FullName);
            Assert.Equal(DataSpace.CSpace, complexType.DataSpace);
            Assert.Equal(new[] { "prop1", "prop2" }, complexType.Members.Select(m => m.Name));
            Assert.True(complexType.IsReadOnly);

            var metadataProperty = complexType.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public void Create_throws_for_null_arguments()
        {
            Assert.Equal(
                "name",
                Assert.Throws<ArgumentNullException>(
                    () => ComplexType.Create(null, "foo", DataSpace.CSpace, new EdmMember[0], null)).ParamName);

            Assert.Equal(
                "namespaceName",
                Assert.Throws<ArgumentNullException>(
                    () => ComplexType.Create("foo", null, DataSpace.CSpace, new EdmMember[0], null)).ParamName);

            Assert.Equal(
                "members",
                Assert.Throws<ArgumentNullException>(
                    () => ComplexType.Create("foo", "bar", DataSpace.CSpace, null, null)).ParamName);


        }
    }
}
