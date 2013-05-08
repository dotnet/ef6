// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public class EntityConventionOfTypeTests
    {
        [Fact]
        public void Apply_invokes_action_when_no_predicates_and_same_type()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                Enumerable.Empty<Func<Type, bool>>(),
                c => actionInvoked = true);
            var type = typeof(LocalType1);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_no_predicates_and_derived_type()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                Enumerable.Empty<Func<Type, bool>>(),
                c => actionInvoked = true);
            var type = typeof(LocalType2);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_no_predicates_and_different_type()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                Enumerable.Empty<Func<Type, bool>>(),
                c => actionInvoked = true);
            var type = typeof(object);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_invokes_action_when_predicate_true_and_same_type()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                new Func<Type, bool>[] { t => true },
                c => actionInvoked = true);
            var type = typeof(LocalType1);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.True(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_and_short_circuts_when_different_type()
        {
            var predicateInvoked = false;
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                new Func<Type, bool>[] { t => predicateInvoked = true },
                c => actionInvoked = true);
            var type = typeof(object);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.False(predicateInvoked);
            Assert.False(actionInvoked);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_predicate_false_but_same_type()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfType<LocalType1>(
                new Func<Type, bool>[] { t => false },
                c => actionInvoked = true);
            var type = typeof(LocalType1);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.False(actionInvoked);
        }

        private class LocalType1
        {
        }

        private class LocalType2 : LocalType1
        {
        }
    }
}
