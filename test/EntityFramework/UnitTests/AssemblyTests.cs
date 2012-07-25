// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System;
    using System.Linq;
    using Xunit;

    public class AssemblyTests : TestBase
    {
        [Fact]
        public void Microsoft_Data_Entity_CTP_is_CLSCompliant()
        {
            CLSCompliantAttribute attr = typeof(DbModelBuilder).Assembly.GetCustomAttributes(true).OfType<CLSCompliantAttribute>().Single();
            Assert.True(attr.IsCompliant);
        }
    }
}
