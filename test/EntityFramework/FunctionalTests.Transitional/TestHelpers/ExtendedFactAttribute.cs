// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [XunitTestCaseDiscoverer("System.Data.Entity.ExtendedFactDiscoverer", "EntityFramework.FunctionalTests.Transitional")]
    public class ExtendedFactAttribute : FactAttribute
    {
        public TestGroup SlowGroup { get; set; }

        public bool SkipForSqlAzure { get; set; }

        public bool SkipForLocalDb { get; set; }

        public string Justification { get; set; }
    }

    public class ExtendedFactDiscoverer : FactDiscoverer
    {
        public ExtendedFactDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            return ShouldRun(factAttribute, factAttribute.GetNamedArgument<TestGroup>(nameof(ExtendedFactAttribute.SlowGroup)))
                ? base.Discover(discoveryOptions, testMethod, factAttribute)
                : Enumerable.Empty<IXunitTestCase>();
        }

        protected bool ShouldRun(IAttributeInfo factAttribute, TestGroup testGroup)
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

            if (factAttribute.GetNamedArgument<bool>(nameof(ExtendedFactAttribute.SkipForSqlAzure)))
            {
                var connectionString = ConfigurationManager.AppSettings["BaseConnectionString"];
                if (DatabaseTestHelpers.IsSqlAzure(connectionString))
                {
                    return false;
                }
            }

            if (factAttribute.GetNamedArgument<bool>(nameof(ExtendedFactAttribute.SkipForLocalDb)))
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
