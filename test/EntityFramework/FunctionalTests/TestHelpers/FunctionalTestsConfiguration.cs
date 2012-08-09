// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;

    public class FunctionalTestsConfiguration : DbConfiguration
    {
        public FunctionalTestsConfiguration()
        {
            AddDependencyResolver(DefaultConnectionFactoryResolver.Instance);
            AddDependencyResolver(new SingletonDependencyResolver<IManifestTokenService>(new FunctionalTestsManifestTokenService()));
        }
    }
}
