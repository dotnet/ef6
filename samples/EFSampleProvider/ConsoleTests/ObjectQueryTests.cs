using System;
using System.Data.Objects;
using NorthwindEFModel;

namespace ConsoleTests
{
    class ObjectQueryTests
    {
        public static void RunTests()
        {
            ObjectQuery_Parameterized();            
        }

        static void ObjectQuery_Parameterized()
        {
            Console.WriteLine("ObjectQuery_Parameterized");

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NorthwindEntities"].ConnectionString;
            string commandText = "SELECT VALUE C FROM NorthwindEntities.Customers AS C WHERE C.CustomerID LIKE @CustomerID";
            using (ObjectContext context = new ObjectContext(connectionString))
            {
                ObjectQuery<Customer> query = context.CreateQuery<Customer>(commandText, new ObjectParameter("CustomerID", "A%"));
                Console.WriteLine("  Query Results - {0}", commandText);
                foreach (Customer c in query)
                    Console.WriteLine("    Name: {0}, Location: {1}", c.CompanyName, c.Location);
            }

            Console.WriteLine();
        }
    }
}