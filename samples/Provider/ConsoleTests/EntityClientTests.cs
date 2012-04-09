using System;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTests
{
    class EntityClientTests
    {
        public static void RunTests()
        {
            EntityCommand_Parameterized();
        }

        static void EntityCommand_Parameterized()
        {
            Console.WriteLine("EntityCommand_Parameterized");

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NorthwindEntities"].ConnectionString;
            string commandText = "SELECT C.CompanyName FROM NorthwindEntities.Customers AS C WHERE C.CustomerID LIKE @CustomerID";
            using (EntityConnection connection = new EntityConnection(connectionString))
            {
                connection.Open();

                using (EntityCommand command = new EntityCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("CustomerID", "A%");
                    using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        Console.WriteLine("  Query Results - {0}", command.CommandText);
                        while (reader.Read())
                            Console.WriteLine("    {0}", reader["CompanyName"]);
                    }
                }
            }

            Console.WriteLine();
        }
    }
}
