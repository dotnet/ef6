// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;
    using Xunit.Sdk;

    public class ExtendedFactAttribute : FactAttribute
    {
        public TestGroup SlowGroup { get; set; }

        public bool SkipForSqlAzure { get; set; }

        public bool SkipForLocalDb { get; set; }

        public string Justification { get; set; }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return ShouldRun(SlowGroup)
                ? base.EnumerateTestCommands(method)
                : Enumerable.Empty<ITestCommand>();
        }

        protected bool ShouldRun(TestGroup testGroup)
        {
#if SkipMigrationsTests
            if (testGroup == TestGroup.MigrationsTests)
            {
                return false;
            }
#endif

#if SkipSlowTests
            if (testGroup != TestGroup.Default)
            {
                return false;
            }
#endif

            if (SkipForSqlAzure)
            {
                var connectionString = ConfigurationManager.AppSettings["BaseConnectionString"];
                if (DatabaseTestHelpers.IsSqlAzure(connectionString))
                {
                    return false;
                }
            }

            if (SkipForLocalDb)
            {
                var connectionString = ConfigurationManager.AppSettings["BaseConnectionString"];
                if (DatabaseTestHelpers.IsLocalDb(connectionString))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
