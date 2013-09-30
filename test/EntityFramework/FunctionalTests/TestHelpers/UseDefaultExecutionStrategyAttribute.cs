using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.TestHelpers
{
    using System.Reflection;

    public class UseDefaultExecutionStrategyAttribute : Xunit.BeforeAfterTestAttribute
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
