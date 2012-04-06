using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using NorthwindEFModel;

namespace ConsoleTests
{
    class DmlTests
    {
        public static void RunTests()
        {
            PrepareDb();

            string customerId = InsertCustomer();
            UpdateCustomer(customerId);
            DeleteCustomer(customerId);
        }

        static string CustomerID = "ATEST";

        static void PrepareDb()
        {
            using (DbConnection connection = HelperFunctions.GetFactoryViaDbProviderFactories().CreateConnection())
            {
                connection.ConnectionString = HelperFunctions.NorthwindDirectConnectionString;
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("DELETE FROM Customers WHERE CustomerID = '{0}'", CustomerID);
                    command.ExecuteNonQuery();
                }
            }
        }

        static string InsertCustomer()
        {
            Console.WriteLine("DmlTests_InsertCustomer");

            string customerId = CustomerID;
            string companyName = "A Test Customer";

            using (NorthwindEntities northwindContext = new NorthwindEntities())
            {
                Customer c = new Customer();
                c.CustomerID = customerId;
                c.CompanyName = companyName;
                northwindContext.AddObject("Customers", c);
                northwindContext.SaveChanges();

                if (HelperFunctions.AssertRowCount(string.Format("SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}' AND CompanyName = '{1}'", customerId, companyName),
                                                   1,
                                                   string.Format("  Failure - Cound not locate Customer with CustomerID of '{0}' AND CompanyName of '{1}'", customerId, companyName)))
                    Console.WriteLine("  Success!");
            }
            Console.WriteLine();

            return customerId;
        }

        static void UpdateCustomer(string customerId)
        {
            Console.WriteLine("DmlTests_UpdateCustomer");

            string newCompanyName = "New Company Name";
            using (NorthwindEntities northwindContext = new NorthwindEntities())
            {
                Customer c = northwindContext.Customers.Where("it.CustomerID = @CustomerID",
                                                         new ObjectParameter("CustomerID", customerId)).First();
                c.CompanyName = newCompanyName;
                northwindContext.SaveChanges();
                if (HelperFunctions.AssertRowCount(string.Format("SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}' AND CompanyName = '{1}'", customerId, newCompanyName),
                                                   1,
                                                   string.Format("  Failure - Could not locate Order with CustomerID of '{0}' and CompanyName of '{1}'", customerId, newCompanyName)))
                    Console.WriteLine("  Success!");
            }

            Console.WriteLine();
        }

        static void DeleteCustomer(string customerId)
        {
            Console.WriteLine("DmlTests_DeleteCustomer");

            using (NorthwindEntities northwindContext = new NorthwindEntities())
            {
                Customer c = northwindContext.Customers.Where("it.CustomerID = @CustomerID",
                                                         new ObjectParameter("CustomerID", customerId)).First();
                northwindContext.DeleteObject(c);
                northwindContext.SaveChanges();
                if (HelperFunctions.AssertRowCount(string.Format("SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}'", customerId),
                                                   0,
                                                   string.Format("  Failure - There are still Customers with CustomerID of '{0}'", customerId)))
                    Console.WriteLine("  Success!");
            }

            Console.WriteLine();
        }
    }
}
