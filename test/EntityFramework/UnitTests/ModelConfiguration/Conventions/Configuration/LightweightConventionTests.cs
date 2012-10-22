// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Xunit;

    public class LightweightConventionTests
    {
        [Fact]
        public void Apply_entity_is_noop_when_no_action()
        {
            var predicateInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Where(t => predicateInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(predicateInvoked);
        }

        [Fact]
        public void Apply_entity_invokes_action_when_no_predicates()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_entity_invokes_action_when_single_predicate_true()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Where(t => true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_entity_invokes_action_when_all_predicates_true()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Where(t => true)
                .Where(t => true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_entity_does_not_invoke_action_when_single_predicate_false()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => false)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_entity_does_not_invoke_action_and_short_circuts_when_first_predicate_false()
        {
            var lastPredicateInvoked = false;
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => false)
                .Where(t => lastPredicateInvoked = true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(lastPredicateInvoked);
            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_entity_does_not_invoke_action_when_last_predicate_false()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => true)
                .Where(t => false)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var type = new MockType();
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration);

            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_property_is_noop_when_no_property_configuration()
        {
            var predicateInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Where(t => predicateInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(predicateInvoked);
        }

        [Fact]
        public void Apply_property_is_noop_when_no_action()
        {
            var typePredicateInvoked = false;
            var propertyPredicateInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Where(t => typePredicateInvoked = true).Properties().Where(p => propertyPredicateInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(typePredicateInvoked);
            Assert.False(propertyPredicateInvoked);
        }

        [Fact]
        public void Apply_property_invokes_action_when_no_predicates()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities.Properties().Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_property_invokes_action_when_no_type_predicates_and_property_predicate_true()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Properties()
                .Where(p => true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_property_invokes_action_when_type_predicate_true_and_no_property_predicates()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => true)
                .Properties()
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_property_invokes_action_when_type_and_property_predicate_true()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => true)
                .Properties()
                .Where(p => true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_property_does_not_invoke_action_when_type_predicate_false()
        {
            var propertyPredicateInvoked = false;
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Where(t => false)
                .Properties()
                .Where(p => propertyPredicateInvoked = true)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(propertyPredicateInvoked);
            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_property_does_not_invoke_action_when_property_predicate_false()
        {
            var actionInvoked = false;
            var entities = new EntityConventionConfiguration();
            entities
                .Properties()
                .Where(p => false)
                .Configure(c => actionInvoked = true);
            var convention = new LightweightConvention(entities);
            var propertyInfo = new MockPropertyInfo(new MockType(), "Property1");
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(actionInvoked);
        }
    }
}
