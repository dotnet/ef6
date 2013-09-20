// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ComplexTypeMappingTests
    {
        [Fact]
        public void Can_create_with_valid_complex_type()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);

            Assert.Same(complexType, complexTypeMapping.ComplexType);
        }

        [Fact]
        public void Cannot_create_with_null_complex_type()
        {
            Assert.Equal(
                "complexType",
                Assert.Throws<ArgumentNullException>(
                    () => new ComplexTypeMapping(null)).ParamName);
        }

        [Fact]
        public void Can_add_remove_properties()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            Assert.Empty(complexTypeMapping.Properties);

            complexTypeMapping.AddProperty(scalarPropertyMapping);

            Assert.Same(scalarPropertyMapping, complexTypeMapping.Properties.Single());

            complexTypeMapping.RemoveProperty(scalarPropertyMapping);

            Assert.Empty(complexTypeMapping.Properties);
        }

        [Fact]
        public void Can_add_remove_conditions()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);
            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            Assert.Empty(complexTypeMapping.Conditions);

            complexTypeMapping.AddCondition(conditionMapping);

            Assert.Same(conditionMapping, complexTypeMapping.Conditions.Single());

            complexTypeMapping.RemoveCondition(conditionMapping);

            Assert.Empty(complexTypeMapping.Conditions);
        }

        [Fact]
        public void Cannot_add_property_when_read_only()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);

            complexTypeMapping.SetReadOnly();

            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => complexTypeMapping.AddProperty(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_read_only()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);
            var scalarPropertyMapping = new ScalarPropertyMapping(new EdmProperty("P"), new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })));

            complexTypeMapping.AddProperty(scalarPropertyMapping);

            complexTypeMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => complexTypeMapping.RemoveProperty(scalarPropertyMapping)).Message);
        }

        [Fact]
        public void Cannot_add_condition_when_read_only()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);

            complexTypeMapping.SetReadOnly();

            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => complexTypeMapping.AddCondition(conditionMapping)).Message);
        }

        [Fact]
        public void Cannot_remove_condition_when_read_only()
        {
            var complexType = new ComplexType();
            var complexTypeMapping = new ComplexTypeMapping(complexType);
            var conditionMapping = new IsNullConditionMapping(new EdmProperty("P"), true);

            complexTypeMapping.AddCondition(conditionMapping);

            complexTypeMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => complexTypeMapping.RemoveCondition(conditionMapping)).Message);
        }
    }
}
