// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using Xunit;

    public sealed class InversePropertyAttributeConventionTests
    {
        [Fact]
        public void Apply_finds_inverse_when_optional_to_many()
        {
            var propertyInfo = typeof(AType1).GetInstanceProperty("Bs");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(AType1), () => modelConfiguration.Entity(typeof(AType1)), modelConfiguration),
                    new InversePropertyAttribute("A"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(AType1)).Navigation(propertyInfo);

            Assert.Same(typeof(BType1).GetDeclaredProperty("A"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        public class AType1
        {
            public ICollection<BType1> Bs { get; set; }
        }

        public class BType1
        {
            public AType1 A { get; set; }
        }

        [Fact]
        public void Apply_finds_inverse_when_many_to_optional()
        {
            var propertyInfo = typeof(BType1).GetInstanceProperty("A");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(BType1), () => modelConfiguration.Entity(typeof(BType1)), modelConfiguration),
                    new InversePropertyAttribute("Bs"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType1)).Navigation(propertyInfo);

            Assert.Same(typeof(AType1).GetDeclaredProperty("Bs"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_finds_inverse_when_optional_to_optional()
        {
            var propertyInfo = typeof(BType3).GetInstanceProperty("A");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(BType3), () => modelConfiguration.Entity(typeof(BType3)), modelConfiguration),
                    new InversePropertyAttribute("B"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType3)).Navigation(propertyInfo);

            Assert.Same(typeof(AType3).GetDeclaredProperty("B"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        public class AType3
        {
            public BType3 B { get; set; }
        }

        public class BType3
        {
            public AType3 A { get; set; }
        }

        [Fact]
        public void Apply_finds_inverse_when_many_to_many()
        {
            var propertyInfo = typeof(BType2).GetInstanceProperty("As");
            var modelConfiguration = new ModelConfiguration();

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(BType2), () => modelConfiguration.Entity(typeof(BType2)), modelConfiguration),
                    new InversePropertyAttribute("Bs"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType2)).Navigation(propertyInfo);

            Assert.Same(typeof(AType2).GetInstanceProperty("Bs"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        public class AType2
        {
            public ICollection<BType2> Bs { get; set; }
        }

        public class BType2
        {
            public ICollection<AType2> As { get; set; }
        }

        [Fact]
        public void Apply_ignores_inverse_when_already_configured()
        {
            var propertyInfo = typeof(AType4).GetInstanceProperty("B");
            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(AType4)).Navigation(propertyInfo);
            navigationPropertyConfiguration.InverseNavigationProperty = typeof(BType4).GetInstanceProperty("A2");

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(AType4), () => modelConfiguration.Entity(typeof(AType4)), modelConfiguration),
                    new InversePropertyAttribute("A1"));

            Assert.NotSame(typeof(BType4).GetInstanceProperty("A1"), navigationPropertyConfiguration.InverseNavigationProperty);
        }

        public class AType4
        {
            public BType4 B { get; set; }
        }

        public class BType4
        {
            public AType4 A1 { get; set; }
            public AType4 A2 { get; set; }
        }

        [Fact]
        public void Apply_ignores_inverse_on_nonnavigation()
        {
            var propertyInfo = typeof(AType5).GetInstanceProperty("B");
            var modelConfiguration = new ModelConfiguration();
            var entityConfiguration = modelConfiguration.Entity(typeof(AType5));

            new InversePropertyAttributeConvention()
                .Apply(
                    propertyInfo, new ConventionTypeConfiguration(typeof(AType5), () => entityConfiguration, modelConfiguration),
                    new InversePropertyAttribute("A1"));

            Assert.False(entityConfiguration.IsNavigationPropertyConfigured(propertyInfo));
        }

        public class AType5
        {
            public int B { get; set; }
        }

        [Fact]
        public void Apply_throws_on_self_inverse()
        {
            var propertyInfo = typeof(AType6).GetInstanceProperty("A");
            var modelConfiguration = new ModelConfiguration();

            Assert.Equal(
                Strings.InversePropertyAttributeConvention_SelfInverseDetected("A", typeof(AType6)),
                Assert.Throws<InvalidOperationException>(
                    () => new InversePropertyAttributeConvention()
                              .Apply(
                                  propertyInfo,
                                  new ConventionTypeConfiguration(
                              typeof(AType6), () => modelConfiguration.Entity(typeof(AType6)), modelConfiguration),
                                  new InversePropertyAttribute("A"))).Message);
        }

        public class AType6
        {
            public AType6 A { get; set; }
        }

        [Fact]
        public void Apply_throws_when_cannot_find_inverse_property()
        {
            var propertyInfo = typeof(AType7).GetInstanceProperty("B");
            var modelConfiguration = new ModelConfiguration();

            Assert.Equal(
                Strings.InversePropertyAttributeConvention_PropertyNotFound("Foo", typeof(BType7), "B", typeof(AType7)),
                Assert.Throws<InvalidOperationException>(
                    () => new InversePropertyAttributeConvention()
                              .Apply(
                                  propertyInfo,
                                  new ConventionTypeConfiguration(
                              typeof(AType7), () => modelConfiguration.Entity(typeof(AType7)), modelConfiguration),
                                  new InversePropertyAttribute("Foo"))).Message);
        }

        public class AType7
        {
            public BType7 B { get; set; }
        }

        public class BType7
        {
        }
    }
}
