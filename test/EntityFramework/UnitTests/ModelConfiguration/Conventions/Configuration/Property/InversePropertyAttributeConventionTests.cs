// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class InversePropertyAttributeConventionTests
    {
        [Fact]
        public void Apply_finds_inverse_when_optional_to_many()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA, "A");
            mockTypeA.Property(mockTypeB.AsCollection(), "Bs");
            var mockPropertyInfo = mockTypeA.GetProperty("Bs");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("A"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeA).Navigation(mockPropertyInfo);

            Assert.Same(mockTypeB.GetProperty("A"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_finds_inverse_when_many_to_optional()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA, "A");
            var mockPropertyInfo = mockTypeB.GetProperty("A");
            mockTypeA.Property(mockTypeB.AsCollection(), "Bs");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("Bs"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeB).Navigation(mockPropertyInfo);

            Assert.Same(mockTypeA.GetProperty("Bs"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_finds_inverse_when_optional_to_optional()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA, "A");
            var mockPropertyInfo = mockTypeB.GetProperty("A");
            mockTypeA.Property(mockTypeB, "B");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("B"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeB).Navigation(mockPropertyInfo);

            Assert.Same(mockTypeA.GetProperty("B"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_finds_inverse_when_many_to_many()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA.AsCollection(), "As");
            var mockPropertyInfo = mockTypeB.GetProperty("As");
            mockTypeA.Property(mockTypeB.AsCollection(), "Bs");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("Bs"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeB).Navigation(mockPropertyInfo);

            Assert.Same(mockTypeA.GetProperty("Bs"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_ignores_inverse_when_already_configured()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA, "A1").Property(mockTypeA, "A2");
            mockTypeA.Property(mockTypeB, "B");
            var mockPropertyInfo = mockTypeA.GetProperty("B");
            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeA).Navigation(mockPropertyInfo);
            navigationPropertyConfiguration.InverseNavigationProperty = mockTypeB.GetProperty("A2");

            new InversePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("A1"));

            Assert.NotSame(mockTypeB.GetProperty("A1"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_throws_on_self_inverse()
        {
            var mockTypeA = new MockType("A");
            mockTypeA.Property(mockTypeA, "A");
            var mockPropertyInfo = mockTypeA.GetProperty("A");
            var modelConfiguration = new ModelConfiguration();

            Assert.Equal(
                Strings.InversePropertyAttributeConvention_SelfInverseDetected("A", mockTypeA.Object),
                Assert.Throws<InvalidOperationException>(
                    () => new InversePropertyAttributeConvention()
                              .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("A"))).Message);
        }

        [Fact]
        public void Apply_throws_when_cannot_find_inverse_property()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B");
            mockTypeA.Property(mockTypeB, "B");
            var mockPropertyInfo = mockTypeA.GetProperty("B");
            var modelConfiguration = new ModelConfiguration();

            Assert.Equal(
                Strings.InversePropertyAttributeConvention_PropertyNotFound("Foo", mockTypeB.Object, "B", mockTypeA.Object),
                Assert.Throws<InvalidOperationException>(
                    () => new InversePropertyAttributeConvention()
                              .Apply(mockPropertyInfo, modelConfiguration, new InversePropertyAttribute("Foo"))).Message);
        }
    }
}
