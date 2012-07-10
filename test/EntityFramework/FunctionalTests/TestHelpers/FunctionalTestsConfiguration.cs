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
