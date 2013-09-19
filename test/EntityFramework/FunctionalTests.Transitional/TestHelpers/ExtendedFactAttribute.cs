// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Sdk;

    public class ExtendedFactAttribute : FactAttribute
    {
        public TestGroup SlowGroup { get; set; }

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
            return true;
        }
    }
}
