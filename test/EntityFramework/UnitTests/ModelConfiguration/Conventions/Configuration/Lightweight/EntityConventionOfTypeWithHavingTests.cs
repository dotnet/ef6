// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Linq;
    using Xunit;

    public class EntityConventionOfTypeWithHavingTests
    {
        [Fact]
        public void Apply_invokes_action_with_value_when_not_null()
        {
            var actionInvoked = false;
            object capturedValue = null;
            var value = new object();
            var convention = new EntityConventionOfTypeWithHaving<object, object>(
                Enumerable.Empty<Func<Type, bool>>(),
                t => value,
                (c, v) =>
                {
                    actionInvoked = true;
                    capturedValue = v;
                });
            var type = typeof(object);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.True(actionInvoked);
            Assert.Same(value, capturedValue);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_value_null()
        {
            var actionInvoked = false;
            var convention = new EntityConventionOfTypeWithHaving<object, object>(
                Enumerable.Empty<Func<Type, bool>>(),
                t => null,
                (c, v) => actionInvoked = true);
            var type = typeof(object);
            var configuration = new EntityTypeConfiguration(type);

            convention.Apply(type, () => configuration, new ModelConfiguration());

            Assert.False(actionInvoked);
        }
    }
}
