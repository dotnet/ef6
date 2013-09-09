// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class EntityFunctionsTests : TestBase
    {
        private const BindingFlags FunctionBindingFlags = BindingFlags.Static | BindingFlags.Public;

        [Fact]
        public void All_EntityFunctions_are_attributed_with_DbFunctionAttribute_except_unicode_methods()
        {
#pragma warning disable 612,618
            var entityFunctions = typeof(EntityFunctions).GetMethods(FunctionBindingFlags);
#pragma warning restore 612,618
            Assert.True(entityFunctions.Length >= 93); // Just make sure Reflection is returning what we expect

            foreach (var function in entityFunctions.Where(f => f.Name != "AsUnicode" && f.Name != "AsNonUnicode"))
            {
                Assert.NotNull(function.GetCustomAttributes<DbFunctionAttribute>(inherit: false).FirstOrDefault());
            }
        }

        [Fact]
        public void All_DbFunctions_are_also_included_in_EntityFunctions()
        {
            var dbFunctions = typeof(DbFunctions).GetMethods(FunctionBindingFlags);
#pragma warning disable 612,618
            var entityFunctions = typeof(EntityFunctions).GetMethods(FunctionBindingFlags);
#pragma warning restore 612,618

            Assert.Equal(dbFunctions.Length, entityFunctions.Length);
            Assert.True(dbFunctions.Length >= 93); // Just make sure Reflection is returning what we expect

            foreach (var function in dbFunctions)
            {
                var dbFunction = function;
                Assert.Equal(
                    1, entityFunctions.Count(
                        f => f.Name == dbFunction.Name
                             && f.GetParameters()
                                 .Select(p => p.ParameterType)
                                 .SequenceEqual(dbFunction.GetParameters().Select(p => p.ParameterType))));
            }
        }
    }
}
