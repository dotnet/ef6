// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.TestHelpers
{
    using System.Data.Entity.Config;

    public class FunctionalTestsConfiguration : DbConfiguration
    {
        public FunctionalTestsConfiguration()
        {
            AddDependencyResolver(DefaultConnectionFactoryResolver.Instance);
        }
    }
}
