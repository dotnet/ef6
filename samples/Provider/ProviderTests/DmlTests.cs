// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NorthwindEFModel;
using SampleEntityFrameworkProvider;
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
                            "SELECT COUNT(*) " +
                            "FROM Customers " +
                            "WHERE CustomerID = '{0}' AND CompanyName = '{1}' AND Location.STAsText() = 'POINT (17.038333 51.107778)'",
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
                    customer.Location = SpatialServices.Instance.GeographyFromText("POINT (-122.191667 47.685833)");

                    northwindContext.SaveChanges();
                }

                // Verify the customer was updated
                Assert.Equal(
                    1,
                    ExecuteScalar(
                        string.Format(
                            "SELECT COUNT(*) " +
                            "FROM Customers " +
                            "WHERE CustomerID = '{0}' AND CompanyName = '{1}' AND Location.STAsText() = 'POINT (-122.191667 47.685833)'",
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

        [Fact]
        public void Verify_Update_DbGeography()
        {
            using (var transaction = new TransactionScope())
            {
                int orderID;
                using (var northwindContext = new NorthwindEntities())
                {
                    var order = northwindContext
                        .Orders
                        .OrderBy(o => o.OrderID)
                        .First();

                    orderID = order.OrderID;

                    order.ContainerSize = SpatialServices.Instance.GeometryFromText("LINESTRING (100 100, 20 180, 180 180)");
                    northwindContext.SaveChanges();
                }

                // Verify the customer was updated
                Assert.Equal(
                    1,
                    ExecuteScalar(
                        string.Format(
                            "SELECT COUNT(*) FROM Orders WHERE OrderID = '{0}' AND ContainerSize.STAsText() = 'LINESTRING (100 100, 20 180, 180 180)'",
                            orderID)));
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
                            CompanyName = companyName,
                            Location = SpatialServices.Instance.GeographyFromText("POINT (17.038333 51.107778)")
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
