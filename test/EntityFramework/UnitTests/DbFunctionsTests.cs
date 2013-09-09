// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class DbFunctionsTests : TestBase
    {
        [Fact]
        public void All_DbFunctions_are_attributed_with_DbFunctionAttribute_except_unicode_methods()
        {
            var entityFunctions = typeof(DbFunctions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            Assert.True(entityFunctions.Length >= 93); // Just make sure Reflection is returning what we expect

            foreach (var function in entityFunctions.Where(f => f.Name != "AsUnicode" && f.Name != "AsNonUnicode"))
            {
                Assert.NotNull(function.GetCustomAttributes<DbFunctionAttribute>(inherit: false).FirstOrDefault());
            }
        }
    }
}
