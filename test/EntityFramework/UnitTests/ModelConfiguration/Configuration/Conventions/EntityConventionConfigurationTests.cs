// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using Xunit;

    public class EntityConventionConfigurationTests
    {
        [Fact]
        public void Where_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionConfiguration(conventions);

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
            var entities = new EntityConventionConfiguration(conventions);

            var config = entities
                .Where(predicate1)
                .Where(predicate2);

            Assert.NotNull(config);
            Assert.Same(conventions, config.ConventionsConfiguration);
            Assert.Equal(2, config.Predicates.Count());
            Assert.Same(predicate2, config.Predicates.Last());
        }

        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionConfiguration(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Configure(null));
            Assert.Equal("entityConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_adds_convention()
        {
            Func<Type, bool> predicate = t => true;
            Action<LightweightEntityConfiguration> configurationAction = c => { };
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionConfiguration(conventions);

            entities
                .Where(predicate)
                .Configure(configurationAction);

            Assert.Equal(36, conventions.Conventions.Count());

            var convention = (EntityConvention)conventions.Conventions.Last();
            Assert.Equal(1, convention.Predicates.Count());
            Assert.Same(predicate, convention.Predicates.Single());
            Assert.Same(configurationAction, convention.EntityConfigurationAction);
        }

        [Fact]
        public void Having_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionConfiguration(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Having<object>(null));
            Assert.Equal("capturingPredicate", ex.ParamName);
        }

        [Fact]
        public void Having_configures_capturing_predicate()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionConfiguration(conventions);
            Func<Type, bool> predicate = t => true;
            Func<Type, object> capturingPredicate = t => null;

            var config = entities
                .Where(predicate)
                .Having(capturingPredicate);

            Assert.NotNull(config);
            Assert.Same(conventions, config.ConventionsConfiguration);
            Assert.Equal(1, config.Predicates.Count());
            Assert.Same(predicate, config.Predicates.Single());
            Assert.Same(capturingPredicate, config.CapturingPredicate);
        }
    }
}
