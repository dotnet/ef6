// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public class DbHelpersTests
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(DbHelpers.ConvertAndSetMethod);
        }

        [Fact]
        public void KeyValuesEqual_checks_value_equality_for_value_types_and_Equals_implementation_for_reference_types()
        {
            Equality_tests(DbHelpers.KeyValuesEqual);

            Assert.True(DbHelpers.KeyValuesEqual(new ExecutionStrategyKey("foo", "bar"), new ExecutionStrategyKey("foo", "bar")));
        }

        [Fact]
        public void PropertyValuesEqual_checks_value_equality_for_value_types_and_reference_equality_for_reference_types()
        {
            Equality_tests(DbHelpers.PropertyValuesEqual);

            Assert.False(DbHelpers.PropertyValuesEqual(new ExecutionStrategyKey("foo", "bar"), new ExecutionStrategyKey("foo", "bar")));
        }

        private void Equality_tests(Func<object, object, bool> equalityComparer)
        {
            Assert.True(equalityComparer(DBNull.Value, null));
            Assert.True(equalityComparer(null, DBNull.Value));
            Assert.True(equalityComparer(DBNull.Value, DBNull.Value));

            Assert.True(equalityComparer(1, 1));
            Assert.True(equalityComparer((int?)1, 1));
            Assert.True(equalityComparer((int?)1, (int?)1));
            Assert.False(equalityComparer((int?)1, null));

            Assert.True(equalityComparer("foo".ToUpper(), ("fo" + "o").ToUpper()));

            var obj = new object();
            Assert.True(equalityComparer(obj, obj));
            Assert.False(equalityComparer(new object(), new object()));

            Assert.True(equalityComparer(new[] { (byte)1 }, new[] { (byte)1 }));
            Assert.False(equalityComparer(new[] { (byte)1 }, new[] { (byte)2 }));
            Assert.False(equalityComparer(new[] { (byte)1 }, new[] { (byte)1, 0 }));
            Assert.False(equalityComparer(new[] { (byte?)1 }, new[] { (byte?)1 }));
        }
    }
}
