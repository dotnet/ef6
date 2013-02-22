// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Linq;
    using Xunit;

    public class MissingPropertyConfigurationTests
    {
        [Fact]
        public void ClrPropertyInfo_returns_null()
        {
            var config = new MissingPropertyConfiguration();

            Assert.Null(config.ClrPropertyInfo);
        }

        [Fact]
        public void Overrides_all_methods()
        {
            var objectMethodNames = new[] { "ToString", "Equals", "GetHashCode", "GetType" };
            var properties = typeof(MissingPropertyConfiguration).GetProperties();
            var methodsNotOverridden = typeof(MissingPropertyConfiguration).GetMethods()
                .Where(
                    m => m.DeclaringType != typeof(MissingPropertyConfiguration)
                        && !objectMethodNames.Contains(m.Name)
                        && !properties.Any(p => p.GetAccessors().Contains(m)))
                .ToArray();

            Assert.False(
                methodsNotOverridden.Any(),
                string.Format(
                        "The following methods have not been overridden MissingPropertyConfiguration: {0}",
                        string.Join(", ", methodsNotOverridden.Select(m => m.Name))));
        }
    }
}
