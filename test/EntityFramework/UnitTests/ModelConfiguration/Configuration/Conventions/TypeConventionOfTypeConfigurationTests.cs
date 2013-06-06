// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using Xunit;

    public class TypeConventionOfTypeConfigurationTests
    {
        [Fact]
        public void Where_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Where(null));
            Assert.Equal("predicate", ex.ParamName);
        }

        [Fact]
        public void Where_configures_predicates()
        {
            Func<Type, bool> predicate1 = t => true;
            Func<Type, bool> predicate2 = t => false;
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);

            var config = entities
                .Where(predicate1)
                .Where(predicate2);

            Assert.IsType<TypeConventionOfTypeConfiguration<object>>(config);
            Assert.Same(conventions, config.ConventionsConfiguration);
            Assert.Equal(2, config.Predicates.Count());
            Assert.Same(predicate2, config.Predicates.Last());
        }

        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Configure(null));
            Assert.Equal("entityConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_adds_convention()
        {
            Func<Type, bool> predicate = t => true;
            Action<LightweightTypeConfiguration<object>> configurationAction = c => { };
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);

            entities
                .Where(predicate)
                .Configure(configurationAction);

            Assert.Equal(16, conventions.ConfigurationConventions.Count());

            var convention = (TypeConventionOfType<object>)conventions.ConfigurationConventions.Last();
            Assert.Equal(2, convention.Predicates.Count());
            Assert.Same(predicate, convention.Predicates.Last());
            Assert.Same(configurationAction, convention.EntityConfigurationAction);
        }

        [Fact]
        public void Having_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Having<object>(null));
            Assert.Equal("capturingPredicate", ex.ParamName);
        }

        [Fact]
        public void Having_configures_capturing_predicate()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new TypeConventionOfTypeConfiguration<object>(conventions);
            Func<Type, bool> predicate = t => true;
            Func<Type, object> capturingPredicate = t => null;

            var config = entities
                .Where(predicate)
                .Having(capturingPredicate);

            Assert.IsType<TypeConventionOfTypeWithHavingConfiguration<object, object>>(config);
            Assert.Same(conventions, config.ConventionsConfiguration);
            Assert.Equal(1, config.Predicates.Count());
            Assert.Same(predicate, config.Predicates.Single());
            Assert.Same(capturingPredicate, config.CapturingPredicate);
        }
    }
}
