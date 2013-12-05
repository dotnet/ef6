// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using Xunit;

    public class IndexAnnotationSerializerTests
    {
        [Fact]
        public void Annotations_containing_single_indexes_are_serialized_to_expected_format()
        {
            Assert.Equal(
                "{ }",
                Serialize(new IndexAttribute()));

            Assert.Equal(
                "{ Name: 'EekyBear' }",
                Serialize(new IndexAttribute("EekyBear")));

            Assert.Equal(
                "{ Name: 'EekyBear', Order: 7 }",
                Serialize(new IndexAttribute("EekyBear", 7)));

            Assert.Equal(
                "{ Order: 8 }",
                Serialize(new IndexAttribute { Order = 8 }));

            Assert.Equal(
                "{ IsClustered: True }",
                Serialize(new IndexAttribute { IsClustered = true }));

            Assert.Equal(
                "{ IsUnique: True }",
                Serialize(new IndexAttribute { IsUnique = true }));

            Assert.Equal(
                "{ Name: 'EekyBear', Order: 7, IsClustered: False, IsUnique: False }",
                Serialize(new IndexAttribute("EekyBear", 7) { IsClustered = false, IsUnique = false }));
        }

        [Fact]
        public void Annotations_containing_multiple_indexes_are_serialized_to_expected_format()
        {
            Assert.Equal(
                "{ }"
                + "{ Name: 'MrsPandy' }"
                + "{ Name: 'EekyBear', Order: 7 }"
                + "{ Name: 'Splash', Order: 8 }"
                + "{ Name: 'Tarquin', IsClustered: False }"
                + "{ Name: 'MrsKoalie', IsUnique: False }"
                + "{ Name: 'EekyJnr', Order: 7, IsClustered: True, IsUnique: True }",
                Serialize(
                    new IndexAttribute(),
                    new IndexAttribute("MrsPandy"),
                    new IndexAttribute("EekyBear", 7),
                    new IndexAttribute("Splash") { Order = 8 },
                    new IndexAttribute("Tarquin") { IsClustered = false },
                    new IndexAttribute("MrsKoalie") { IsUnique = false },
                    new IndexAttribute("EekyJnr", 7) { IsClustered = true, IsUnique = true }));
        }

        [Fact]
        public void SerializeValue_checks_its_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => new IndexAnnotationSerializer().SerializeValue(null, new IndexAnnotation(new IndexAttribute()))).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => new IndexAnnotationSerializer().SerializeValue(" ", new IndexAnnotation(new IndexAttribute()))).Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(
                    () => new IndexAnnotationSerializer().SerializeValue("Index", null)).ParamName);

            Assert.Equal(
                Strings.AnnotationSerializeWrongType("Random", "IndexAnnotationSerializer", "IndexAnnotation"),
                Assert.Throws<ArgumentException>(
                    () => new IndexAnnotationSerializer().SerializeValue("Index", new Random())).Message);
        }

        private static string Serialize(params IndexAttribute[] indexAttributes)
        {
            return new IndexAnnotationSerializer().SerializeValue("Index", new IndexAnnotation(indexAttributes));
        }

        [Fact]
        public void Strings_containing_single_indexes_are_deserialized_to_expected_annotation()
        {
            Assert.Equal(
                "{ }",
                Serialize(Deserialize("{ }")));

            Assert.Equal(
                "{ Name: 'EekyBear' }",
                Serialize(Deserialize("{ Name: 'EekyBear' }")));

            Assert.Equal(
                "{ Name: 'EekyBear', Order: 7 }",
                Serialize(Deserialize("{ Name: 'EekyBear', Order: 7 }")));

            Assert.Equal(
                "{ Order: 8 }",
                Serialize(Deserialize("{ Order: 8 }")));

            Assert.Equal(
                "{ IsClustered: True }",
                Serialize(Deserialize("{ IsClustered: True }")));

            Assert.Equal(
                "{ IsUnique: True }",
                Serialize(Deserialize("{ IsUnique: True }")));

            Assert.Equal(
                "{ Name: 'EekyBear', Order: 7, IsClustered: False, IsUnique: False }",
                Serialize(Deserialize("{ Name: 'EekyBear', Order: 7, IsClustered: False, IsUnique: False }")));

            Assert.Equal(
                "{ Name: 'EekyBear', Order: 7, IsClustered: False, IsUnique: False }",
                Serialize(Deserialize(" {  Name:  'EekyBear' ,  Order:  7 ,  IsClustered:  False ,  IsUnique:  False  } ")));
        }

        [Fact]
        public void Strings_containing_multiple_indexes_are_deserialized_to_expected_annotation()
        {
            Assert.Equal(
                "{ }"
                + "{ Name: 'MrsPandy' }"
                + "{ Name: 'EekyBear', Order: 7 }"
                + "{ Name: 'Splash', Order: 8 }"
                + "{ Name: 'Tarquin', IsClustered: False }"
                + "{ Name: 'MrsKoalie', IsUnique: False }"
                + "{ Name: 'EekyJnr', Order: 7, IsClustered: True, IsUnique: True }",
                Serialize(
                    Deserialize(
                        "{ }"
                        + "{ Name: 'MrsPandy' }"
                        + "{ Name: 'EekyBear', Order: 7 }"
                        + "{ Name: 'Splash', Order: 8 }"
                        + "{ Name: 'Tarquin', IsClustered: False }"
                        + "{ Name: 'MrsKoalie', IsUnique: False }"
                        + "{ Name: 'EekyJnr', Order: 7, IsClustered: True, IsUnique: True }")));

            Assert.Equal(
                "{ }"
                + "{ Name: 'MrsPandy' }"
                + "{ Name: 'EekyBear', Order: 7 }"
                + "{ Name: 'Splash', Order: 88 }"
                + "{ Name: 'Tarquin', IsClustered: False }"
                + "{ Name: 'MrsKoalie', IsUnique: False }"
                + "{ Name: 'EekyJnr', Order: 7, IsClustered: True, IsUnique: True }",
                Serialize(
                    Deserialize(
                        " {} "
                        + " { Name: 'MrsPandy' } "
                        + " { Name: 'EekyBear', Order: 7 } "
                        + " { Name: 'Splash', Order: 88 } "
                        + " { Name: 'Tarquin', IsClustered: False } "
                        + " { Name: 'MrsKoalie', IsUnique: False } "
                        + " { Name: 'EekyJnr', Order: 7, IsClustered: True, IsUnique: True } ")));
        }

        [Fact]
        public void DeserializeValue_checks_its_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => new IndexAnnotationSerializer().DeserializeValue(null, "{}")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(
                    () => new IndexAnnotationSerializer().DeserializeValue(" ", "{}")).Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(
                    () => new IndexAnnotationSerializer().DeserializeValue("Index", null)).ParamName);
        }

        [Fact]
        public void DeserializeValue_throws_on_invalid_formats()
        {
            TestBadDeserialize("{ Name: '' }");
            TestBadDeserialize("{ Name: 'EekyBear', Name: 'EekyBear' }");
            TestBadDeserialize("{ Order: 7, Order: 7 }");
            TestBadDeserialize("{ IsClustered: True, IsClustered: True }");
            TestBadDeserialize("{ IsUnique: True, IsUnique: True }");
            TestBadDeserialize("{ Order: 7a7 }");
            TestBadDeserialize("{ Name: EekyBear }");
            TestBadDeserialize("{ Order: }");
            TestBadDeserialize("{ IsClustered: ");
            TestBadDeserialize("{ IsUnique: }");
            TestBadDeserialize("{ Order: 9876543210 }");
        }

        private static void TestBadDeserialize(string value)
        {
            Assert.Equal(
                Strings.AnnotationSerializeBadFormat(value, "IndexAnnotationSerializer", IndexAnnotationSerializer.FormatExample),
                Assert.Throws<FormatException>(
                    () => new IndexAnnotationSerializer().DeserializeValue("Index", value)).Message);
        }

        private static IndexAnnotation Deserialize(string annotation)
        {
            return (IndexAnnotation)new IndexAnnotationSerializer().DeserializeValue("Index", annotation);
        }

        private static string Serialize(IndexAnnotation annotation)
        {
            return new IndexAnnotationSerializer().SerializeValue("Index", annotation);
        }
    }
}
