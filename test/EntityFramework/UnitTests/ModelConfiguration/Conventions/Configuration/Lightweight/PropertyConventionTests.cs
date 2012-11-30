// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class PropertyConventionTests
    {
        [Fact]
        public void Apply_invokes_action_when_no_predicates()
        {
            var actionInvoked = false;
            var convention = new PropertyConvention(
                Enumerable.Empty<Func<PropertyInfo, bool>>(),
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_single_predicate_true()
        {
            var actionInvoked = false;
            var convention = new PropertyConvention(
                new Func<PropertyInfo, bool>[] { p => true },
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_all_predicates_true()
        {
            var actionInvoked = false;
            var convention = new PropertyConvention(
                new Func<PropertyInfo, bool>[]
                    {
                        p => true,
                        p => true
                    },
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_single_predicate_false()
        {
            var actionInvoked = false;
            var convention = new PropertyConvention(
                new Func<PropertyInfo, bool>[] { p => false },
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_and_short_circuts_when_first_predicate_false()
        {
            var lastPredicateInvoked = false;
            var actionInvoked = false;
            var convention = new PropertyConvention(
                new Func<PropertyInfo, bool>[]
                    {
                        p => false,
                        p => lastPredicateInvoked = true
                    },
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(lastPredicateInvoked);
            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_last_predicate_false()
        {
            var actionInvoked = false;
            var convention = new PropertyConvention(
                new Func<PropertyInfo, bool>[]
                    {
                        p => true,
                        p => false
                    },
                c => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(actionInvoked);
        }
    }
}
