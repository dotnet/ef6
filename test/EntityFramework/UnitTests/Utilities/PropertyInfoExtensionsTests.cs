// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Spatial;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public sealed class PropertyInfoExtensionsTests
    {
        public class IsValidStructuralProperty : TestBase
        {
            [Fact]
            public void IsValidStructuralProperty_should_return_true_when_property_read_write()
            {
                var mockProperty = new MockPropertyInfo(typeof(int), "P");

                Assert.True(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_false_when_property_invalid()
            {
                var mockProperty = new MockPropertyInfo(typeof(object), "P");

                Assert.False(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_false_when_property_abstract()
            {
                var mockProperty = new MockPropertyInfo().Abstract();

                Assert.False(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_false_when_property_write_only()
            {
                var mockProperty = new MockPropertyInfo();
                mockProperty.SetupGet(p => p.CanRead).Returns(false);

                Assert.False(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_false_when_property_read_only()
            {
                var mockProperty = new MockPropertyInfo(typeof(object), "Prop");
                mockProperty.SetupGet(p => p.CanWrite).Returns(false);

                var mockType = new MockType();
                mockType.Setup(m => m.GetProperties(It.IsAny<BindingFlags>())).Returns(new[] { mockProperty.Object });

                mockProperty.SetupGet(p => p.DeclaringType).Returns(mockType.Object);

                Assert.False(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_true_when_property_read_only_collection()
            {
                var mockProperty = new MockPropertyInfo(typeof(List<string>), "Coll");
                mockProperty.SetupGet(p => p.CanWrite).Returns(false);

                var mockType = new MockType();
                mockType.Setup(m => m.GetProperties(It.IsAny<BindingFlags>())).Returns(new[] { mockProperty.Object });

                mockProperty.SetupGet(p => p.DeclaringType).Returns(mockType.Object);

                Assert.True(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_false_for_indexed_property()
            {
                var mockProperty = new MockPropertyInfo();
                mockProperty.Setup(p => p.GetIndexParameters()).Returns(new ParameterInfo[1]);

                Assert.False(mockProperty.Object.IsValidStructuralProperty());
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_true_when_declaring_type_can_be_set()
            {
                Assert.True(typeof(Derived).GetProperty("Prop").IsValidStructuralProperty());
            }
        }

        public class CanWriteExtended : TestBase
        {
            [Fact]
            public void CanWriteExtended_returns_true_for_property_that_has_a_private_setter()
            {
                Assert.True(typeof(Base).GetProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_true_for_property_that_has_a_private_setter_on_the_declaring_type()
            {
                Assert.True(typeof(Derived).GetProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_false_for_property_that_has_no_setter()
            {
                Assert.False(typeof(BaseNoSetter).GetProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_false_for_property_that_has_no_setter_on_the_declaring_type()
            {
                Assert.False(typeof(DerivedNoSetter).GetProperty("Prop").CanWriteExtended());
            }

            public class BaseNoSetter
            {
                public int Prop
                {
                    get { return 0; }
                }
            }

            public class DerivedNoSetter : BaseNoSetter
            {
            }

            [Fact]
            public void CanWriteExtended_returns_false_for_indexer_property_that_has_no_setter()
            {
                Assert.False(typeof(BaseWithIndexer).GetProperty("Item").CanWriteExtended());
            }

            [Fact] // CodePlex 1215
            public void CanWriteExtended_returns_false_for_indexer_property_that_has_no_setter_on_the_declaring_type()
            {
                Assert.False(typeof(DerivedWithIndexer).GetProperty("Item").CanWriteExtended());
            }

            public class BaseWithIndexer
            {
                public int this[int index]
                {
                    get { return 0; }
                }
            }

            public class DerivedWithIndexer : BaseWithIndexer
            {
            }
        }

        public class GetPropertyInfoForSet : TestBase
        {
            [Fact]
            public void GetPropertyInfoForSet_returns_given_property_if_it_has_a_setter()
            {
                var propertyInfo = typeof(Base).GetProperty("Prop");
                Assert.Same(propertyInfo, propertyInfo.GetPropertyInfoForSet());
            }

            [Fact]
            public void GetPropertyInfoForSet_returns_declaring_type_property_if_it_has_a_setter()
            {
                Assert.Same(typeof(Base).GetProperty("Prop"), typeof(Derived).GetProperty("Prop").GetPropertyInfoForSet());
            }
        }

        public class IsValidEdmScalarProperty : TestBase
        {
            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_nullable_scalar()
            {
                var mockProperty = new MockPropertyInfo(typeof(int?), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_string()
            {
                var mockProperty = new MockPropertyInfo(typeof(string), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_byte_array()
            {
                var mockProperty = new MockPropertyInfo(typeof(byte[]), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_geography()
            {
                var mockProperty = new MockPropertyInfo(typeof(DbGeography), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_geometry()
            {
                var mockProperty = new MockPropertyInfo(typeof(DbGeometry), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_scalar()
            {
                var mockProperty = new MockPropertyInfo(typeof(decimal), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_enum()
            {
                var mockProperty = new MockPropertyInfo(typeof(FileMode), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_true_for_nullable_enum()
            {
                var mockProperty = new MockPropertyInfo(typeof(FileMode?), "P");

                Assert.True(mockProperty.Object.IsValidEdmScalarProperty());
            }

            [Fact]
            public void IsValidEdmScalarProperty_should_return_false_when_invalid_type()
            {
                var mockProperty = new MockPropertyInfo(typeof(object), "P");

                Assert.False(mockProperty.Object.IsValidEdmScalarProperty());
            }
        }

        public class AsEdmPrimitiveProperty : TestBase
        {
            [Fact]
            public void AsEdmPrimitiveProperty_sets_fields_from_propertyInfo()
            {
                var propertyInfo = typeof(PropertyInfoExtensions_properties_fixture).GetProperty("Key");
                var property = propertyInfo.AsEdmPrimitiveProperty();

                Assert.Equal("Key", property.Name);
                Assert.Equal(false, property.Nullable);
                Assert.Equal(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), property.PrimitiveType);
            }

            [Fact]
            public void AsEdmPrimitiveProperty_sets_is_nullable_for_nullable_type()
            {
                PropertyInfo propertyInfo = new MockPropertyInfo(typeof(string), "P");
                var property = propertyInfo.AsEdmPrimitiveProperty();

                Assert.Equal(true, property.Nullable);
            }

            [Fact]
            public void AsEdmPrimitiveProperty_returns_null_for_non_primitive_type()
            {
                var propertyInfo = typeof(PropertyInfoExtensions_properties_fixture).GetProperty("EdmProperty");
                var property = propertyInfo.AsEdmPrimitiveProperty();

                Assert.Null(property);
            }

            private class PropertyInfoExtensions_properties_fixture
            {
                public int Key { get; set; }
                public EntityType EdmProperty { get; set; }
            }
        }

        public class GetPropertiesInHierarchy : TestBase
        {
            [Fact]
            public void Returns_all_properties_in_virtual_hierarchy()
            {
                Assert.Equal(new[] { "Cher.Mary", "ChimChim.Mary", "Ee.Mary" }, GetProperties("Mary"));
                Assert.Equal(new[] { "Cher.Poppins", "ChimChim.Poppins", "Ee.Poppins" }, GetProperties("Poppins"));
                Assert.Equal(new[] { "Cher.Bert", "ChimChim.Bert", "Ee.Bert" }, GetProperties("Bert"));
                Assert.Equal(new[] { "Cher.Banks", "ChimChim.Banks", "Ee.Banks" }, GetProperties("Banks"));
                Assert.Equal(new[] { "Cher.AdmiralBoom", "ChimChim.AdmiralBoom", "Ee.AdmiralBoom" }, GetProperties("AdmiralBoom"));
                Assert.Equal(new[] { "Cher.MrsBrill", "ChimChim.MrsBrill" }, GetProperties("MrsBrill"));
                Assert.Equal(new[] { "Cher.MrBinnacle", "ChimChim.MrBinnacle" }, GetProperties("MrBinnacle"));
                Assert.Equal(new[] { "ChimChim.ConstableJones" }, GetProperties("ConstableJones"));
                Assert.Equal(new[] { "ChimChim.MrDawesSr" }, GetProperties("MrDawesSr"));
                Assert.Equal(new[] { "ChimChim.UncleAlbert", "Ee.UncleAlbert" }, GetProperties("UncleAlbert"));
            }

            private static IEnumerable<string> GetProperties(string propertyName)
            {
                const BindingFlags bindingFlags =
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                return typeof(ChimChim)
                    .GetProperty(propertyName, bindingFlags)
                    .GetPropertiesInHierarchy()
                    .Select(p => p.DeclaringType.Name + "." + p.Name)
                    .OrderBy(n => n);
            }

            public class ChimChim : Cher
            {
                public override int Mary { get; set; }
                internal override int Poppins { get; set; }
                protected override int Bert { get; set; }
                public override int Banks { get { return 0; } }
                public override int AdmiralBoom { set { } }
                public override int MrsBrill { get; set; }
                public override int MrBinnacle { get; set; }
                public int ConstableJones { get; set; }
                public new static int MrDawesSr { get; set; }
                public override int UncleAlbert { get; set; }
            }

            public class Cher : Ee
            {
                public override int Mary { get; set; }
                internal override int Poppins { get; set; }
                protected override int Bert { get; set; }
                public override int Banks { get { return 0; } }
                public override int AdmiralBoom { set { } }
                public new virtual int MrsBrill { get; set; }
                public virtual int MrBinnacle { get; set; }
                public new static int MrDawesSr { get; set; }
            }

            public class Ee
            {
                public virtual int Mary { get; set; }
                internal virtual int Poppins { get; set; }
                protected virtual int Bert { get; set; }
                public virtual int Banks { get; private set; }
                public virtual int AdmiralBoom { private get; set; }
                public virtual int MrsBrill { get; set; }
                public static int MrDawesSr { get; set; }
                public virtual int UncleAlbert { get; set; }
            }
        }

        public class Base
        {
            public int Prop { get; private set; }
        }

        public class Derived : Base
        {
        }
    }
}
