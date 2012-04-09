using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace ConsoleTests
{
    class SimpleWhidbeyTests
    {
        public static void RunTests()
        {
            GetProviderViaDbProviderFactories();
            GetQueryResultsViaReader();
        }

        public static void GetProviderViaDbProviderFactories()
        {
            Console.WriteLine("GetProviderViaDbProviderFactories");

            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();
            if (factory == null)
                Console.WriteLine("  Failure! - DbProviderFactories.GetFactory({0}) returned null", HelperFunctions.ProviderName);
            else
                Console.WriteLine("  Success!");

            Console.WriteLine();
        }

        public static void GetQueryResultsViaReader()
        {
            Console.WriteLine("GetQueryResultsViaReader");
            DbProviderFactory factory = HelperFunctions.GetFactoryViaDbProviderFactories();

            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = HelperFunctions.NorthwindDirectConnectionString;
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT CompanyName, Location FROM Customers WHERE CustomerID LIKE @CustomerID";

                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@CustomerID";
                    parameter.Value = "A%";
                    command.Parameters.Add(parameter);

                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        command.Transaction = transaction;

                        Console.WriteLine("  Query Results - {0}", command.CommandText);
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("    Name: {0}, Location: {1}", reader["CompanyName"], reader["Location"]);
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
        }

    }
}
