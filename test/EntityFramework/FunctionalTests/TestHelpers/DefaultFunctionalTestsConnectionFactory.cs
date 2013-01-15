// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    ///     This connection factory is set in the functional tests app.config, but is then replaced by
    ///     the OnLockingConfiguration event handler of <see cref="FunctionalTestsConfiguration" />.
    /// </summary>
    public class DefaultFunctionalTestsConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            throw new NotImplementedException(
                "This connection factory should never be used because it is overriden in the OnLockingConfiguration event.");
        }
    }
}
