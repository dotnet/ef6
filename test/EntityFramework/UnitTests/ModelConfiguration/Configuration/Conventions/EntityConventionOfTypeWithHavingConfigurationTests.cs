// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using Xunit;

    public class EntityConventionOfTypeWithHavingConfigurationTests
    {
        [Fact]
        public void Configure_evaluates_preconditions()
        {
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionOfTypeConfiguration<object>(conventions);

            var ex = Assert.Throws<ArgumentNullException>(
                () => entities.Having<object>(t => null).Configure(null));
            Assert.Equal("entityConfigurationAction", ex.ParamName);
        }

        [Fact]
        public void Configure_adds_convention()
        {
            Func<Type, bool> predicate = t => true;
            Func<Type, object> capturingPredicate = t => null;
            Action<LightweightEntityConfiguration<object>, object> configurationAction = (c, o) => { };
            var conventions = new ConventionsConfiguration();
            var entities = new EntityConventionOfTypeConfiguration<object>(conventions);

            entities
                .Where(predicate)
                .Having(capturingPredicate)
                .Configure(configurationAction);

            Assert.Equal(36, conventions.Conventions.Count());

            var convention = (EntityConventionOfTypeWithHaving<object, object>)conventions.Conventions.Last();
            Assert.Equal(2, convention.Predicates.Count());
            Assert.Same(predicate, convention.Predicates.Last());
            Assert.Same(capturingPredicate, convention.CapturingPredicate);
            Assert.Same(configurationAction, convention.EntityConfigurationAction);
        }
    }
}
