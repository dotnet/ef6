// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Reflection;
    using Xunit;

    public class AspProxyTests : TestBase
    {
#if NET452
        public class TryInitializeWebAssembly
        {
            [Fact]
            public void System_Web_is_not_loaded_into_an_app_domain_that_has_not_already_loaded_System_Web()
            {
                RunTestInAppDomain(typeof(TryInitializeWebAssemblyWhenNotAlreadyLoaded));
            }

            public class TryInitializeWebAssemblyWhenNotAlreadyLoaded : MarshalByRefObject
            {
                public TryInitializeWebAssemblyWhenNotAlreadyLoaded()
                {
                    Assert.False(new AspProxy().TryInitializeWebAssembly());
                }
            }

            [Fact]
            public void System_Web_is_found_when_already_loaded_into_the_app_domain()
            {
                RunTestInAppDomain(typeof(TryInitializeWebAssemblyWhenAlreadyLoaded));
            }

            public class TryInitializeWebAssemblyWhenAlreadyLoaded : MarshalByRefObject
            {
                public TryInitializeWebAssemblyWhenAlreadyLoaded()
                {
                    Assert.NotNull(Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
                    Assert.True(new AspProxy().TryInitializeWebAssembly());
                }
            }
        }
#endif

        public class GetReferencedAssembliesMethod
        {
            [Fact]
            public void BuildManager_GetReferencedAssemblies_method_can_be_found()
            {
                // Ensure System.Web is loaded for this test
                Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                Assert.NotNull(new AspProxy().GetReferencedAssembliesMethod());
            }
        }

        public class MapWebPath
        {
            [Fact]
            public void MapWebPath_does_not_throw_using_Reflection_to_invoke_methods()
            {
                // Ensure System.Web is loaded for this test
                Assembly.Load("System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

                Assert.Null(new AspProxy().InternalMapWebPath(@"~\Cheese\And\Pickle"));
            }
        }
    }
}
