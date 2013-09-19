// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class AspProxyTests
    {
        public class GetReferencedAssembliesMethod
        {
            [Fact]
            public void BuildManager_GetReferencedAssemblies_method_can_be_found()
            {
                Assert.NotNull(new AspProxy().GetReferencedAssembliesMethod());
            }
        }

        public class MapWebPath
        {
            [Fact]
            public void MapWebPath_does_not_throw_using_Reflection_to_invoke_methods()
            {
                Assert.Null(new AspProxy().InternalMapWebPath(@"~\Cheese\And\Pickle"));
            }
        }
    }
}
