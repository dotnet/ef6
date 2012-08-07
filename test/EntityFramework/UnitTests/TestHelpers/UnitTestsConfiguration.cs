// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Config;
    using FunctionalTests.TestHelpers;

    public class UnitTestsConfiguration : DbConfigurationProxy
    {
        public override Type ConfigurationToUse()
        {
            return typeof(FunctionalTestsConfiguration);
        }
    }
}
