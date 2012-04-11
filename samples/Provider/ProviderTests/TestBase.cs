using System.Configuration;
using System.Data.Common;

namespace ProviderTests
{
    public class TestBase
    {
        protected const string SampleProviderName = "SampleEntityFrameworkProvider";
        protected static readonly string NorthwindDirectConnectionString = ConfigurationManager.ConnectionStrings["NorthwindDirect"].ConnectionString;
        protected static readonly string NorthwindEntitiesConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NorthwindEntities"].ConnectionString;
    }
}
