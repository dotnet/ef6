using System;
using System.Data;
using System.Data.Common;

namespace ConsoleTests
{
    public class HelperFunctions
    {
        public static string ProviderName = "SampleEntityFrameworkProvider";
        public static string NorthwindDirectConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NorthwindDirect"].ConnectionString;

        public static DbProviderFactory GetFactoryViaDbProviderFactories()
        {
            return DbProviderFactories.GetFactory(HelperFunctions.ProviderName);
        }

        public static DbProviderServices GetProviderServicesViaDbProviderFactories()
        {
            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            IServiceProvider iserviceprovider = factory as IServiceProvider;
            return (DbProviderServices)iserviceprovider.GetService(typeof(DbProviderServices));
        }

        public static bool AssertRowCount(string commandText, int expectedRowCount, string failureMessage)
        {
            return AssertRowCount(HelperFunctions.NorthwindDirectConnectionString, commandText, expectedRowCount, failureMessage);
        }

        public static bool AssertRowCount(string connectionString, string commandText, int expectedRowCount, string failureMessage)
        {
            bool assertSucceeded = false;

            DbProviderFactory factory = GetFactoryViaDbProviderFactories();
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    int actualRowCount = (int)command.ExecuteScalar();
                    if (actualRowCount == expectedRowCount)
                        assertSucceeded = true;
                    else
                        Console.WriteLine(failureMessage);
                }
                connection.Close();
            }

            return assertSucceeded;
        }
    }
}
