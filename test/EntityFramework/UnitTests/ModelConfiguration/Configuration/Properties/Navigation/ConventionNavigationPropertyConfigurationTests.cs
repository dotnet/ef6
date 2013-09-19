// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class ConventionNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Methods_dont_throw_if_configuration_is_null()
        {
            var config = new ConventionNavigationPropertyConfiguration(null, null);

            Assert.Null(config.ClrPropertyInfo);
            config.HasConstraint<IndependentConstraintConfiguration>();
            config.HasDeleteAction(OperationAction.Cascade);
            config.HasInverseEndMultiplicity(RelationshipMultiplicity.ZeroOrOne);
            config.HasInverseNavigationProperty(p => new MockPropertyInfo());
            config.IsDeclaringTypePrincipal(true);
            config.HasRelationshipMultiplicity(RelationshipMultiplicity.ZeroOrOne);
        }

        [Fact]
        public void ClrPropertyInfo_returns_the_propertyInfo()
        {
            var propInfo = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var config = new ConventionNavigationPropertyConfiguration(
                new NavigationPropertyConfiguration(propInfo), new ModelConfiguration());

            Assert.Same(propInfo, config.ClrPropertyInfo);
        }

        [Fact]
        public void HasConstraint_sets_the_ForeignKeyConstraint()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasConstraint<ForeignKeyConstraintConfiguration>();
            Assert.IsType<ForeignKeyConstraintConfiguration>(configuration.Constraint);
        }

        [Fact]
        public void HasConstraint_sets_and_configures_the_ForeignKeyConstraint()
        {
            var property = new MockPropertyInfo();
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasConstraint<ForeignKeyConstraintConfiguration>(c => c.AddColumn(property));
            Assert.IsType<ForeignKeyConstraintConfiguration>(configuration.Constraint);
            Assert.Same(property.Object, ((ForeignKeyConstraintConfiguration)configuration.Constraint).ToProperties.Single());
        }

        [Fact]
        public void HasConstraint_sets_the_IndependentConstraint()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasConstraint<IndependentConstraintConfiguration>();
            Assert.Same(IndependentConstraintConfiguration.Instance, configuration.Constraint);
        }

        [Fact]
        public void HasConstraint_is_noop_when_set_to_ForeignKeyConstraint()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            configuration.Constraint = new ForeignKeyConstraintConfiguration();
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasConstraint<IndependentConstraintConfiguration>();
            Assert.IsType<ForeignKeyConstraintConfiguration>(configuration.Constraint);
        }

        [Fact]
        public void HasConstraint_is_noop_when_set_to_IndependentConstraint()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            configuration.Constraint = IndependentConstraintConfiguration.Instance;
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasConstraint<ForeignKeyConstraintConfiguration>();
            Assert.Same(IndependentConstraintConfiguration.Instance, configuration.Constraint);
        }

        [Fact]
        public void HasConstraint_is_noop_when_set_on_inverse()
        {
            var modelConfiguration = new ModelConfiguration();
            var inverseNavigationProperty = typeof(LightweighEntity).GetDeclaredProperty("PrivateNavigationProperty");
            modelConfiguration.Entity(typeof(LightweighEntity))
                .Navigation(inverseNavigationProperty)
                .Constraint = IndependentConstraintConfiguration.Instance;
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            configuration.InverseNavigationProperty = inverseNavigationProperty;
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, modelConfiguration);

            lightweightConfiguration.HasConstraint < ForeignKeyConstraintConfiguration>();
            Assert.Null(configuration.Constraint);
        }

        [Fact]
        public void HasDeleteAction_sets_the_DeleteAction()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasDeleteAction(OperationAction.Cascade);
            Assert.Equal(OperationAction.Cascade, configuration.DeleteAction);

            configuration.DeleteAction = OperationAction.None;
            lightweightConfiguration.HasDeleteAction(OperationAction.Cascade);
            Assert.Equal(OperationAction.None, configuration.DeleteAction);
        }

        [Fact]
        public void HasInverseEndMultiplicity_sets_the_InverseEndKind()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasInverseEndMultiplicity(RelationshipMultiplicity.ZeroOrOne);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, configuration.InverseEndKind);

            configuration.InverseEndKind = RelationshipMultiplicity.Many;
            lightweightConfiguration.HasInverseEndMultiplicity(RelationshipMultiplicity.ZeroOrOne);
            Assert.Equal(RelationshipMultiplicity.Many, configuration.InverseEndKind);
        }

        [Fact]
        public void HasInverseEndMultiplicity_throws_on_incompatible_multiplicity()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var modelConfiguration = new ModelConfiguration();
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, modelConfiguration);

            var inverseNavigationProperty =
                typeof(LightweighEntity).GetDeclaredProperty("ValidInverseNavigationProperty");
            lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty);

            Assert.Equal(
                Strings.LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity(
                "*",
                typeof(LightweighEntity).Name + "." + "ValidInverseNavigationProperty",
                typeof(LightweighEntity).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseEndMultiplicity(RelationshipMultiplicity.Many)).Message);
        }

        [Fact]
        public void HasInverseNavigationProperty_sets_the_InverseNavigationProperty()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var modelConfiguration = new ModelConfiguration();
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, modelConfiguration);

            var inverseNavigationProperty1 =
                typeof(LightweighEntity).GetDeclaredProperty("ValidInverseNavigationProperty");
            lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty1);
            Assert.Same(inverseNavigationProperty1, configuration.InverseNavigationProperty);

            var inverseNavigationProperty2 = typeof(LightweighEntity).GetDeclaredProperty("PrivateNavigationProperty");
            configuration.InverseNavigationProperty = inverseNavigationProperty2;
            lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty1);
            Assert.Same(inverseNavigationProperty2, configuration.InverseNavigationProperty);
        }

        [Fact]
        public void HasInverseNavigationProperty_checks_preconditions()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("PrivateNavigationProperty");
            var lightweightConfiguration =
                new ConventionNavigationPropertyConfiguration(
                    new NavigationPropertyConfiguration(navigationProperty), new ModelConfiguration());

            Assert.Throws<ArgumentNullException>(
                () => lightweightConfiguration.HasInverseNavigationProperty(null));
        }

        [Fact]
        public void HasInverseNavigationProperty_throws_on_invalid_nagivation_property()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var lightweightConfiguration =
                new ConventionNavigationPropertyConfiguration(
                    new NavigationPropertyConfiguration(navigationProperty), new ModelConfiguration());

            var inverseNavigationProperty = typeof(LightweighEntity).GetDeclaredProperty("InvalidNavigationProperty");

            Assert.Equal(
                Strings.LightweightEntityConfiguration_InvalidNavigationProperty(inverseNavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty)).Message);
        }

        [Fact]
        public void HasInverseNavigationProperty_throws_on_nonmatching_nagivation_property()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("UnrelatedNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            var inverseNavigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");

            Assert.Equal(
                Strings.LightweightEntityConfiguration_MismatchedInverseNavigationProperty(
                    typeof(ConventionNavigationPropertyConfigurationTests), navigationProperty.Name,
                    typeof(LightweighEntity), inverseNavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty)).Message);
        }

        [Fact]
        public void HasInverseNavigationProperty_throws_on_nonmatching_inverse_property()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            var inverseNavigationProperty =
                typeof(LightweighEntity).GetDeclaredProperty("UnrelatedNavigationProperty");

            Assert.Equal(
                Strings.LightweightEntityConfiguration_InvalidInverseNavigationProperty(
                    typeof(LightweighEntity), navigationProperty.Name,
                    typeof(ConventionNavigationPropertyConfigurationTests), inverseNavigationProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty)).Message);
        }

        [Fact]
        public void HasInverseNavigationProperty_throws_on_itself()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("PrivateNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            Assert.Equal(
                Strings.NavigationInverseItself(navigationProperty.Name, navigationProperty.DeclaringType),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseNavigationProperty(p => p)).Message);
        }

        [Fact]
        public void HasInverseNavigationProperty_throws_on_incompatible_multiplicity()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var modelConfiguration = new ModelConfiguration();
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, modelConfiguration);

            var inverseNavigationProperty =
                typeof(LightweighEntity).GetDeclaredProperty("ValidInverseNavigationProperty");

            lightweightConfiguration.HasInverseEndMultiplicity(RelationshipMultiplicity.Many);

            Assert.Equal(
                Strings.LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity(
                "*",
                typeof(LightweighEntity).Name + "." + "ValidInverseNavigationProperty",
                typeof(LightweighEntity).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasInverseNavigationProperty(p => inverseNavigationProperty)).Message);
        }

        [Fact]
        public void IsDeclaringTypePrincipal_sets_IsNavigationPropertyDeclaringTypePrincipal()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.IsDeclaringTypePrincipal(false);
            Assert.Equal(false, configuration.IsNavigationPropertyDeclaringTypePrincipal);

            configuration.IsNavigationPropertyDeclaringTypePrincipal = true;
            lightweightConfiguration.IsDeclaringTypePrincipal(false);
            Assert.Equal(true, configuration.IsNavigationPropertyDeclaringTypePrincipal);
        }

        [Fact]
        public void HasRelationshipMultiplicity_gets_and_sets_the_RelationshipMultiplicity()
        {
            var configuration =
                new NavigationPropertyConfiguration(
                    typeof(LightweighEntity).GetDeclaredProperty("ValidNavigationProperty"));
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, new ModelConfiguration());

            lightweightConfiguration.HasRelationshipMultiplicity(RelationshipMultiplicity.ZeroOrOne);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, configuration.RelationshipMultiplicity);

            configuration.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            lightweightConfiguration.HasRelationshipMultiplicity(RelationshipMultiplicity.ZeroOrOne);
            Assert.Equal(RelationshipMultiplicity.Many, configuration.RelationshipMultiplicity);
        }

        [Fact]
        public void HasRelationshipMultiplicity_throws_on_incompatible_multiplicity()
        {
            var navigationProperty = typeof(LightweighEntity).GetDeclaredProperty("PrivateNavigationProperty");
            var configuration = new NavigationPropertyConfiguration(navigationProperty);
            var modelConfiguration = new ModelConfiguration();
            var lightweightConfiguration = new ConventionNavigationPropertyConfiguration(configuration, modelConfiguration);
            
            Assert.Equal(
                Strings.LightweightNavigationPropertyConfiguration_IncompatibleMultiplicity(
                "0..1",
                typeof(LightweighEntity).Name + "." + "PrivateNavigationProperty",
                "System.Collections.Generic.ICollection`1["+
                typeof(LightweighEntity).FullName + "]"),
                Assert.Throws<InvalidOperationException>(
                    () => lightweightConfiguration.HasRelationshipMultiplicity(RelationshipMultiplicity.ZeroOrOne)).Message);
        }

        public class LightweighEntity
        {
            public LightweighEntity InvalidNavigationProperty
            {
                get { return this; }
            }

            public LightweighEntity ValidNavigationProperty { get; set; }
            public LightweighEntity ValidInverseNavigationProperty { get; set; }

            public ConventionNavigationPropertyConfigurationTests UnrelatedNavigationProperty { get; set; }

            private ICollection<LightweighEntity> PrivateNavigationProperty
            {
                get { return new List<LightweighEntity>(); }
            }
        }
    }
}
