// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class PropertyConventionWithHavingTests
    {
        [Fact]
        public void Apply_invokes_action_with_value_when_not_null()
        {
            var actionInvoked = false;
            object capturedValue = null;
            var value = new object();
            var convention = new PropertyConventionWithHaving<object>(
                Enumerable.Empty<Func<PropertyInfo, bool>>(),
                p => value,
                (c, v) =>
                {
                    actionInvoked = true;
                    capturedValue = v;
                });
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.True(actionInvoked);
            Assert.Same(value, capturedValue);
        }

        [Fact]
        public void Apply_does_not_invoke_action_when_value_null()
        {
            var actionInvoked = false;
            var convention = new PropertyConventionWithHaving<object>(
                Enumerable.Empty<Func<PropertyInfo, bool>>(),
                p => null,
                (c, v) => actionInvoked = true);
            var propertyInfo = new MockPropertyInfo();
            var configuration = new PrimitivePropertyConfiguration();

            convention.Apply(propertyInfo, () => configuration);

            Assert.False(actionInvoked);
        }
    }
}
