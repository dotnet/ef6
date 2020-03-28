// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Reflection;
    using Xunit.Sdk;

    public class UseDefaultExecutionStrategyAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            FunctionalTestsConfiguration.SuspendExecutionStrategy = true;
            base.Before(methodUnderTest);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            FunctionalTestsConfiguration.SuspendExecutionStrategy = false;
            base.After(methodUnderTest);
        }
    }
}
