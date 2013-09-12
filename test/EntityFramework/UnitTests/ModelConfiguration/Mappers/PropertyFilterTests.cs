// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    public sealed class PropertyFilterTests
    {
        [Fact]
        public void PropertyFilter_finds_all_properties_on_base_type()
        {
            var propertyNames = new[]
                                    {
                                        "PublicBase",
                                        "PublicBaseForNew",
                                        "PublicVirtualBase",
                                        "PublicVirtualBase2",
                                        "InterfaceImplicit",
                                    };

            var properties = new PropertyFilter().GetProperties(
                typeof(PropertyFilterTests_Base), true, null);

            Assert.True(properties.All(x => propertyNames.Contains(x.Name)));

            properties = new PropertyFilter().GetProperties(
                typeof(PropertyFilterTests_Base), false, Enumerable.Empty<PropertyInfo>());

            Assert.Equal(propertyNames.Length, properties.Count());
            Assert.True(properties.All(x => propertyNames.Contains(x.Name)));
        }

        [Fact]
        public void PropertyFilter_finds_all_properties_on_derived_type()
        {
            var propertyNames = new[]
                                    {
                                        "PublicBase",
                                        "PublicBaseForNew",
                                        "PublicVirtualBase",
                                        "PublicVirtualBase2",
                                        "InterfaceImplicit",
                                        "PublicDerived"
                                    };

            var properties = new PropertyFilter().GetProperties(
                typeof(PropertyFilterTests_Derived), false, Enumerable.Empty<PropertyInfo>());

            Assert.Equal(propertyNames.Length, properties.Count());
            Assert.True(properties.All(x => propertyNames.Contains(x.Name)));
        }

        [Fact]
        public void PropertyFilter_finds_declared_properties_on_derived_type()
        {
            var propertyNames = new[]
                                    {
                                        "PublicDerived"
                                    };

            var properties = new PropertyFilter().GetProperties(
                typeof(PropertyFilterTests_Derived), true, Enumerable.Empty<PropertyInfo>());

            Assert.Equal(propertyNames.Length, properties.Count());
            Assert.True(properties.All(x => propertyNames.Contains(x.Name)));
        }

        [Fact]
        public void PropertyFilter_supports_EF6_properties_if_no_specific_builder_version_is_used()
        {
            Assert.True(new PropertyFilter().Ef6FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_supports_EF6_properties_if_builder_version_for_EF6_is_used()
        {
            Assert.True(new PropertyFilter(DbModelBuilderVersion.V6_0).Ef6FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_supports_EF6_properties_if_latest_builder_version_is_used()
        {
            Assert.True(new PropertyFilter(DbModelBuilderVersion.Latest).Ef6FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_does_not_support_EF6_properties_if_earlier_builder_version_is_used()
        {
            Assert.False(new PropertyFilter(DbModelBuilderVersion.V4_1).Ef6FeaturesSupported);
            Assert.False(new PropertyFilter(DbModelBuilderVersion.V5_0).Ef6FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_supports_V3_properties_if_no_specific_builder_version_is_used()
        {
            Assert.True(new PropertyFilter().EdmV3FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_supports_V3_properties_if_builder_version_for_V3_schema_is_used()
        {
            Assert.True(new PropertyFilter(DbModelBuilderVersion.V5_0).EdmV3FeaturesSupported);
            Assert.True(new PropertyFilter(DbModelBuilderVersion.V6_0).EdmV3FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_supports_V3_properties_if_latest_builder_version_is_used()
        {
            Assert.True(new PropertyFilter(DbModelBuilderVersion.Latest).EdmV3FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_does_not_support_V3_properties_if_V2_builder_version_is_used()
        {
            Assert.False(new PropertyFilter(DbModelBuilderVersion.V4_1).EdmV3FeaturesSupported);
        }

        [Fact]
        public void PropertyFilter_validates_enum_properties_if_no_specific_builder_version_is_used()
        {
            PropertyFilter_validates_enum_types(new PropertyFilter());
        }

        [Fact]
        public void PropertyFilter_validates_spatial_properties_if_no_specific_builder_version_is_used()
        {
            PropertyFilter_validates_spatial_types(new PropertyFilter());
        }

        [Fact]
        public void PropertyFilter_validates_spatial_properties_if_V3_builder_version_is_used()
        {
            PropertyFilter_validates_spatial_types(new PropertyFilter(DbModelBuilderVersion.V5_0));
        }

        [Fact]
        public void PropertyFilter_validates_spatial_properties_if_latest_builder_version_is_used()
        {
            PropertyFilter_validates_spatial_types(new PropertyFilter(DbModelBuilderVersion.Latest));
        }

        [Fact]
        public void PropertyFilter_validates_enum_properties_if_V3_builder_version_is_used()
        {
            PropertyFilter_validates_enum_types(new PropertyFilter(DbModelBuilderVersion.V5_0));
        }

        [Fact]
        public void PropertyFilter_validates_enum_properties_if_latest_builder_version_is_used()
        {
            PropertyFilter_validates_enum_types(new PropertyFilter(DbModelBuilderVersion.Latest));
        }

        private void PropertyFilter_validates_enum_types(PropertyFilter filter)
        {
            var mockType = new MockType();
            mockType.Setup(m => m.IsEnum).Returns(true);

            var properties = new List<PropertyInfo>
                                 {
                                     new MockPropertyInfo(mockType, "EnumProp")
                                 };

            filter.ValidatePropertiesForModelVersion(mockType, properties);
        }

        private void PropertyFilter_validates_spatial_types(PropertyFilter filter)
        {
            var properties = new List<PropertyInfo>
                                 {
                                     new MockPropertyInfo(typeof(DbGeography), "Geography"),
                                     new MockPropertyInfo(typeof(DbGeometry), "Geometry")
                                 };

            filter.ValidatePropertiesForModelVersion(new MockType(), properties);
        }

        [Fact]
        public void PropertyFilter_rejects_enum_properties_if_V2_builder_version_is_used()
        {
            var mockType = new MockType("BadType");
            mockType.Setup(m => m.IsEnum).Returns(true);

            var properties = new List<PropertyInfo>
                                 {
                                     new MockPropertyInfo(mockType, "EnumProp")
                                 };

            Assert.Equal(
                Strings.UnsupportedUseOfV3Type("BadType", "EnumProp"),
                Assert.Throws<NotSupportedException>(
                    () => new PropertyFilter(DbModelBuilderVersion.V4_1).ValidatePropertiesForModelVersion(mockType, properties)).Message);
        }

        [Fact]
        public void PropertyFilter_rejects_DbGeography_properties_if_V2_builder_version_is_used()
        {
            var properties = new List<PropertyInfo>
                                 {
                                     new MockPropertyInfo(typeof(DbGeography), "Geography")
                                 };

            Assert.Equal(
                Strings.UnsupportedUseOfV3Type("BadType", "Geography"),
                Assert.Throws<NotSupportedException>(
                    () =>
                    new PropertyFilter(DbModelBuilderVersion.V4_1).ValidatePropertiesForModelVersion(new MockType("BadType"), properties)).
                    Message);
        }

        [Fact]
        public void PropertyFilter_rejects_DbGeometry_properties_if_V2_builder_version_is_used()
        {
            var properties = new List<PropertyInfo>
                                 {
                                     new MockPropertyInfo(typeof(DbGeometry), "Geometry")
                                 };

            Assert.Equal(
                Strings.UnsupportedUseOfV3Type("BadType", "Geometry"),
                Assert.Throws<NotSupportedException>(
                    () =>
                    new PropertyFilter(DbModelBuilderVersion.V4_1).ValidatePropertiesForModelVersion(new MockType("BadType"), properties)).
                    Message);
        }

        [Fact]
        public void PropertyFilter_includes_enum_and_spatial_properties_if_V3_features_are_supported()
        {
            var filteredProperties = new PropertyFilter().GetProperties(typeof(AType1), declaredOnly: false);

            Assert.Equal(
                new[] { "EnumProp", "Geography", "Geometry" },
                filteredProperties.Select(f => f.Name).OrderBy(n => n));
        }

        [Fact]
        public void PropertyFilter_excludes_enum_and_spatial_properties_if_V3_features_are_not_supported()
        {
            Assert.Equal(0, new PropertyFilter(DbModelBuilderVersion.V4_1).GetProperties(typeof(AType1), declaredOnly: false).Count());
        }

        public enum EnumType1
        {
        }

        public class AType1
        {
            public DbGeography Geography { get; set; }
            public DbGeometry Geometry { get; set; }
            public EnumType1 EnumProp { get; set; }
        }

        public interface PropertyFilterTests_Interface
        {
            int InterfaceImplicit { get; set; }
            int InterfaceExplicit { get; set; }
        }

        public abstract class PropertyFilterTests_Base : PropertyFilterTests_Interface
        {
            public int PublicBase { get; set; }
            private int PrivateBase { get; set; }
            internal int InternalBase { get; set; }
            protected int ProtectedBase { get; set; }

            public int PublicBaseForNew { get; set; }

            public virtual int PublicVirtualBase { get; set; }
            internal virtual int InternalVirtualBase { get; set; }
            protected virtual int ProtectedVirtualBase { get; set; }

            public virtual int PublicVirtualBase2 { get; set; }
            internal virtual int InternalVirtualBase2 { get; set; }
            protected virtual int ProtectedVirtualBase2 { get; set; }

            public static int PublicStaticBase { get; set; }
            private static int PrivateStaticBase { get; set; }
            internal static int InternalStaticBase { get; set; }
            protected static int ProtectedStaticBase { get; set; }

            public int InterfaceImplicit { get; set; }
            int PropertyFilterTests_Interface.InterfaceExplicit { get; set; }
        }

        public abstract class PropertyFilterTests_Derived : PropertyFilterTests_Base
        {
            public int PublicDerived { get; set; }
            private int PrivateDerived { get; set; }
            internal int InternalDerived { get; set; }
            protected int ProtectedDerived { get; set; }

            public new int PublicBaseForNew { get; set; }

            public override int PublicVirtualBase { get; set; }
            internal override int InternalVirtualBase { get; set; }
            protected override int ProtectedVirtualBase { get; set; }

            public static int PublicStaticDerived { get; set; }
            private static int PrivateStaticDerived { get; set; }
            internal static int InternalStaticDerived { get; set; }
            protected static int ProtectedStaticDerived { get; set; }
        }
    }
}
