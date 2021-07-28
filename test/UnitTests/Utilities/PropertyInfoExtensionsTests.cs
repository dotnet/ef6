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
                Assert.False(typeof(AType2).GetInstanceProperty("Prop").IsValidStructuralProperty());
            }

            public class AType2
            {
                public object Prop { get { return null; } }
            }

            [Fact]
            public void IsValidStructuralProperty_should_return_true_when_property_read_only_collection()
            {
                Assert.True(typeof(AType1).GetInstanceProperty("Coll").IsValidStructuralProperty());
            }

            public class AType1
            {
                public List<string> Coll { get { return null; } } 
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
                Assert.True(typeof(Derived).GetRuntimeProperty("Prop").IsValidStructuralProperty());
            }
        }

        public class CanWriteExtended : TestBase
        {
            [Fact]
            public void CanWriteExtended_returns_true_for_property_that_has_a_private_setter()
            {
                Assert.True(typeof(Base).GetDeclaredProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_true_for_property_that_has_a_private_setter_on_the_declaring_type()
            {
                Assert.True(typeof(Derived).GetRuntimeProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_false_for_property_that_has_no_setter()
            {
                Assert.False(typeof(BaseNoSetter).GetDeclaredProperty("Prop").CanWriteExtended());
            }

            [Fact]
            public void CanWriteExtended_returns_false_for_property_that_has_no_setter_on_the_declaring_type()
            {
                Assert.False(typeof(DerivedNoSetter).GetRuntimeProperty("Prop").CanWriteExtended());
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
                Assert.False(typeof(BaseWithIndexer).GetDeclaredProperty("Item").CanWriteExtended());
            }

            [Fact] // CodePlex 1215
            public void CanWriteExtended_returns_false_for_indexer_property_that_has_no_setter_on_the_declaring_type()
            {
                Assert.False(typeof(DerivedWithIndexer).GetRuntimeProperty("Item").CanWriteExtended());
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
                var propertyInfo = typeof(Base).GetDeclaredProperty("Prop");
                Assert.Same(propertyInfo, propertyInfo.GetPropertyInfoForSet());
            }

            [Fact]
            public void GetPropertyInfoForSet_returns_declaring_type_property_if_it_has_a_setter()
            {
                Assert.Same(typeof(Base).GetRuntimeProperty("Prop"), typeof(Derived).GetRuntimeProperty("Prop").GetPropertyInfoForSet());
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
                var propertyInfo = typeof(PropertyInfoExtensions_properties_fixture).GetDeclaredProperty("Key");
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
                var propertyInfo = typeof(PropertyInfoExtensions_properties_fixture).GetDeclaredProperty("EdmProperty");
                var property = propertyInfo.AsEdmPrimitiveProperty();

                Assert.Null(property);
            }

            private class PropertyInfoExtensions_properties_fixture
            {
                public int Key { get; set; }
                public EntityType EdmProperty { get; set; }
            }
        }

        public class IsStaticEtc : TestBase
        {
            [Fact]
            public void IsStatic_identifies_static_properties()
            {
                Assert.True(typeof(KitKat).GetAnyProperty("Yummy").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("Wafers").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("And").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("Chocolate").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("With").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("No").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("Nuts").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("But").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("May").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("Contain").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("TreeNuts").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("Just").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("Like").IsStatic());
                Assert.False(typeof(KitKat).GetAnyProperty("A").IsStatic());
                Assert.True(typeof(KitKat).GetAnyProperty("Twix").IsStatic());
            }

            [Fact]
            public void IsPublic_identifies_public_properties()
            {
                Assert.True(typeof(KitKat).GetAnyProperty("Yummy").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("Wafers").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("And").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("Chocolate").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("With").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("No").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("Nuts").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("But").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("May").IsPublic());
                Assert.False(typeof(KitKat).GetAnyProperty("Contain").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("TreeNuts").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("Just").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("Like").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("A").IsPublic());
                Assert.True(typeof(KitKat).GetAnyProperty("Twix").IsPublic());
            }

            [Fact]
            public void Getter_gets_any_getter()
            {
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Yummy").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Wafers").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("And").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Chocolate").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("With").Getter());
                Assert.Null(typeof(KitKat).GetAnyProperty("No").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Nuts").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("But").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("May").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Contain").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("TreeNuts").Getter());
                Assert.Null(typeof(KitKat).GetAnyProperty("Just").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Like").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("A").Getter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Twix").Getter());
            }

            [Fact]
            public void Setter_gets_any_setter()
            {
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Yummy").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Wafers").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("And").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Chocolate").Setter());
                Assert.Null(typeof(KitKat).GetAnyProperty("With").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("No").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Nuts").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("But").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("May").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Contain").Setter());
                Assert.Null(typeof(KitKat).GetAnyProperty("TreeNuts").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Just").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Like").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("A").Setter());
                Assert.NotNull(typeof(KitKat).GetAnyProperty("Twix").Setter());
            }


            public class KitKat
            {
                public static int Yummy { get; set; }
                private static int Wafers { get; set; }
                internal static int And { private get; set; }
                internal static int Chocolate { get; private set; }
                internal protected static int With { get { return 0; } }
                public static int No { set { } }
                public int Nuts { get; set; }
                private int But { get; set; }
                internal int May { private get; set; }
                internal protected int Contain { get; private set; }
                public int TreeNuts { get { return 0; } }
                public int Just { set { } }
                public static int Like { private get; set; }
                public int A { get; private set; }
                public static int Twix { protected internal get; set; }
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
                return typeof(ChimChim)
                    .GetDeclaredProperty(propertyName)
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
