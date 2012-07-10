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
