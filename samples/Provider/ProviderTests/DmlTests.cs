using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NorthwindEFModel;
using Xunit;

namespace ProviderTests
{
    public class DmlTests : TestBase
    {
        private const string customerId = "ATEST";
        private const string companyName = "Test Customer";

        [Fact]
        public void Verify_Insert()
        {
            using (var transaction = new TransactionScope())
            {
                InsertCustomer();

                // Verify the customer was inserted to the database
                Assert.Equal(
                    1, 
                    ExecuteScalar(
                        string.Format(
                            "SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}' AND CompanyName = '{1}'",
                            customerId, 
                            companyName)));

                // rollback transaction to revert changes (automatically rolled back by IDisposable)
            }
        }

        [Fact]
        public void Verify_Update()
        {
            const string newCompanyName = "New Company Name";

            using(var transaction = new TransactionScope())
            {
                // PrepareDb
                InsertCustomer();

                using (var northwindContext = new NorthwindEntities())
                {
                    var customer = northwindContext
                        .Customers
                        .Single(c => c.CustomerID == customerId);

                    customer.CompanyName = newCompanyName;

                    northwindContext.SaveChanges();
                }

                // Verify the customer was updated
                Assert.Equal(
                    1,
                    ExecuteScalar(
                        string.Format(
                            "SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}' AND CompanyName = '{1}'",
                            customerId,
                            newCompanyName)));
            }
        }

        [Fact]
        public void Verify_Delete()
        {
            using (var transaction = new TransactionScope())
            {
                // PrepareDb
                InsertCustomer();

                using (var northwindContext = new NorthwindEntities())
                {
                    var customer = northwindContext
                        .Customers
                        .Single(c => c.CustomerID == customerId);

                    northwindContext.Customers.DeleteObject(customer);
                    northwindContext.SaveChanges();
                }

                // Verify the customer was updated
                Assert.Equal(
                    0,
                    ExecuteScalar(
                        string.Format(
                            "SELECT COUNT(*) FROM Customers WHERE CustomerID = '{0}' AND CompanyName = '{1}'",
                            customerId,
                            companyName)));
            }
        }

        private static void InsertCustomer()
        {
            using (var northwindContext = new NorthwindEntities())
            {
                northwindContext.Customers.AddObject(
                    new Customer()
                        {
                            CustomerID = customerId,
                            CompanyName = companyName
                        });

                northwindContext.SaveChanges();
            }
        }

        private static int ExecuteScalar(string commandText)
        {
            // Verify the customer was inserted to the database
            var providerFactory = DbProviderFactories.GetFactory(SampleProviderName);
            using (var connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = NorthwindDirectConnectionString;

                var command = connection.CreateCommand();
                command.CommandText = commandText;

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }
    }
}
