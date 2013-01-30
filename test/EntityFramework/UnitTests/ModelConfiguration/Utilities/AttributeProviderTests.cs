// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Xunit;

    public sealed class AttributeProviderTests
    {
        [Fact]
        public void GetAttributes_returns_empty_enumerable_when_no_property_descriptor()
        {
            var attributes = new AttributeProvider().GetAttributes(new MockPropertyInfo());

            Assert.Equal(0, attributes.Count());
        }

        [Fact]
        public void GetAttributes_returns_correct_set_of_type_attributes()
        {
            var attributes = new AttributeProvider().GetAttributes(typeof(AttributeProviderTestClass));

            Assert.Equal(1, attributes.OfType<DataContractAttribute>().Count());
            Assert.Equal(1, attributes.OfType<TableAttribute>().Count());
        }

        [Fact]
        public void GetAttributes_returns_correct_set_of_empty_type_attributes()
        {
            var attributes = new AttributeProvider().GetAttributes(typeof(AttributeProviderTestEmptyClass));

            Assert.Equal(0, attributes.Count());
        }

        [Fact]
        public void GetAttributes_returns_correct_set_of_property_attributes()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(typeof(AttributeProviderTestClass).GetProperty("MyProp"));

            Assert.Equal(1, attributes.OfType<KeyAttribute>().Count());
            Assert.Equal(1, attributes.OfType<RequiredAttribute>().Count());
            Assert.Equal(1, attributes.OfType<DataMemberAttribute>().Count());
            Assert.Equal(1, attributes.OfType<MaxLengthAttribute>().Count());
        }

        [Fact]
        public void AttributeCollectionFactory_returns_correct_set_of_empty_property_attributes()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(typeof(AttributeProviderTestEmptyClass).GetProperty("MyProp"));

            Assert.Equal(0, attributes.Count());
        }

        [Fact]
        public void GetAttributes_returns_attributes_from_buddy_class()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(typeof(AttributeProviderTestClass).GetProperty("BuddyProp"));

            Assert.Equal(1, attributes.OfType<KeyAttribute>().Count());
        }

        [Fact]
        public void AttributeCollectionFactory_returns_only_property_attributes_for_complex_type()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(typeof(AttributeProviderEntityWithComplexProperty).GetProperty("CT"));

            Assert.Equal(1, attributes.Count());
            Assert.Equal(typeof(AttributeProviderEntityWithComplexProperty), ((CustomValidationAttribute)attributes.First()).ValidatorType);
            Assert.Equal("ValidateProperty", ((CustomValidationAttribute)attributes.First()).Method);
        }

        [Fact]
        public void GetAttributes_returns_correct_set_of_non_public_type_attributes()
        {
            var attributes = new AttributeProvider().GetAttributes(typeof(NonPublicAttributeProviderTestClass));

            Assert.Equal(1, attributes.OfType<DataContractAttribute>().Count());
            Assert.Equal(1, attributes.OfType<TableAttribute>().Count());
        }

        [Fact]
        public void GetAttributes_returns_correct_set_of_non_public_property_attributes()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(
                    typeof(NonPublicAttributeProviderTestClass)
                        .GetProperty("MyProp", BindingFlags.NonPublic | BindingFlags.Instance));

            Assert.Equal(1, attributes.OfType<KeyAttribute>().Count());
            Assert.Equal(1, attributes.OfType<RequiredAttribute>().Count());
            Assert.Equal(1, attributes.OfType<DataMemberAttribute>().Count());
            Assert.Equal(1, attributes.OfType<MaxLengthAttribute>().Count());
        }

        [Fact]
        public void GetAttributes_does_not_return_attributes_from_non_public_buddy_class()
        {
            var attributes = new AttributeProvider()
                .GetAttributes(
                    typeof(NonPublicAttributeProviderTestClass)
                        .GetProperty("BuddyProp", BindingFlags.NonPublic | BindingFlags.Instance));

            Assert.Equal(0, attributes.OfType<KeyAttribute>().Count());
        }

        #region Test Fixtures

        [DataContract]
        [Table("Foo")]
        [MetadataType(typeof(AttributeProviderBuddyClass))]
        public class AttributeProviderTestClass
        {
            [Key]
            [Required]
            [DataMember(Order = 55)]
            [MaxLength(5)]
            public string MyProp { get; set; }

            public int BuddyProp { get; set; }
        }

        public class AttributeProviderTestEmptyClass
        {
            public string MyProp { get; set; }
        }

        public class AttributeProviderBuddyClass
        {
            [Key]
            public int BuddyProp { get; set; }
        }

        [DataContract]
        [Table("Foo")]
        [MetadataType(typeof(NonPublicAttributeProviderBuddyClass))]
        private class NonPublicAttributeProviderTestClass
        {
            [Key]
            [Required]
            [DataMember(Order = 55)]
            [MaxLength(5)]
            private string MyProp { get; set; }

            private int BuddyProp { get; set; }
        }

        public class NonPublicAttributeProviderBuddyClass
        {
            [Key]
            private int BuddyProp { get; set; }
        }

        [ComplexType]
        [CustomValidation(typeof(AttributeProviderComplexType), "ValidateWholeType")]
        public class AttributeProviderComplexType
        {
            public int Data { get; set; }

            public static void ValidateWholeType()
            {
            }
        }

        public class AttributeProviderEntityWithComplexProperty
        {
            [CustomValidation(typeof(AttributeProviderEntityWithComplexProperty), "ValidateProperty")]
            public AttributeProviderComplexType CT { get; set; }

            public static void ValidateProperty()
            {
            }
        }

        #endregion
    }
}
