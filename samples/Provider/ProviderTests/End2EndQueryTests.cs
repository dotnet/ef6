using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Spatial;
using System.Diagnostics;
using NorthwindEFModel;
using Xunit;
using System.Linq;
using SampleEntityFrameworkProvider;
using System.Data.Common;

namespace ProviderTests
{
    public class End2EndQueryTests : TestBase
    {
        private class ExpectedResult
        {
            private readonly string name;
            private readonly string location;

            public ExpectedResult(string name, string location)
            {
                this.name = name;
                this.location = location;
            }

            public string Name
            {
                get { return name; }
            }

            public string Location
            {
                get { return location; }
            }
        }

        private readonly List<ExpectedResult> expectedResults =
            new List<ExpectedResult>()
                {
                    new ExpectedResult("Alfreds Futterkiste", "POINT (13.32737 52.420563)"),
                    new ExpectedResult("Ana Trujillo Emparedados y helados",
                                       "POINT (-99.1327229817708 19.4333312988281)"),
                    new ExpectedResult("Antonio Moreno Taquería", "POINT (-99.2789713541667 19.3417358398438)"),
                    new ExpectedResult("Around the Horn", "POINT (-0.143545896959206 51.513600908938)")
                };

        [Fact]
        public void Verify_querying_database_with_DbCommand_works()
        {
            var factory = DbProviderFactories.GetFactory(SampleProviderName);

            using (var connection = factory.CreateConnection())
            {
                Debug.Assert(connection != null, "connection != null");
                connection.ConnectionString = NorthwindDirectConnectionString;
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT CompanyName, Location FROM Customers WHERE CustomerID LIKE @CustomerID";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@CustomerID";
                    parameter.Value = "A%";
                    command.Parameters.Add(parameter);

                    using (var transaction = connection.BeginTransaction())
                    {
                        command.Transaction = transaction;

                        using (var reader = command.ExecuteReader())
                        {
                            foreach (var expectedResult in expectedResults)
                            {
                                reader.Read();
                                Assert.Equal(expectedResult.Name, reader["CompanyName"]);
                                // location is SqlGeography
                                dynamic location = reader["Location"];
                                Assert.Equal(expectedResult.Location, new string(location.STAsText().Value));
                            }

                            Assert.False(reader.Read());
                        }
                    }
                }
            }
        }

        [Fact]
        public void Verify_querying_database_with_EntityClient_works()
        {
            const string commandText =
                "SELECT C.CompanyName, C.Location FROM NorthwindEntities.Customers AS C WHERE C.CustomerID LIKE @CustomerID";
            using (var connection = new EntityConnection(NorthwindEntitiesConnectionString))
            {
                connection.Open();

                using (var command = new EntityCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("CustomerID", "A%");
                    using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        foreach (var expectedResult in expectedResults)
                        {
                            reader.Read();
                            Assert.Equal(expectedResult.Name, reader["CompanyName"]);
                            Assert.Equal(expectedResult.Location, ((DbGeography)reader["Location"]).AsText());
                        }

                        Assert.False(reader.Read());
                    }
                }
            }

            Console.WriteLine();

        }

        [Fact]
        public void Verify_querying_database_with_ObjectQuery_works()
        {
            const string commandText =
                "SELECT VALUE C FROM NorthwindEntities.Customers AS C WHERE C.CustomerID LIKE @CustomerID";
            using (var context = new ObjectContext(NorthwindEntitiesConnectionString))
            {
                var query = context.CreateQuery<Customer>(commandText, new ObjectParameter("CustomerID", "A%"));

                var caseIdx = 0;

                foreach (var customer in query)
                {
                    Assert.Equal(expectedResults[caseIdx].Name, customer.CompanyName);
                    Assert.Equal(expectedResults[caseIdx].Location, customer.Location.AsText());
                    caseIdx++;
                }

                Assert.Equal(4, caseIdx);
            }
        }

        [Fact]
        public void Verify_parametrized_Linq_query_works()
        {
            using (var context = new NorthwindEntities())
            {
                var query = from c in context.Customers
                            where c.CustomerID == "ALFKI"
                            select c;

                var customer = query.Single();

                Assert.Equal(expectedResults[0].Name, customer.CompanyName);
                Assert.Equal(expectedResults[0].Location, customer.Location.AsText());
            }
        }

        [Fact]
        public void Verify_query_with_provider_store_function_works()
        {
            var expected =
                new string[]
                {
                    "Aroun... - Company",
                    "B's B... - Company",
                    "Conso... - Company",
                    "Easte... - Company",
                    "North... - Company",
                    "Seven... - Company"
                };

            using (var context = new NorthwindEntities())
            {
                var query =
                    from c in context.Customers
                    where c.Address.City == "London"
                    select SampleSqlFunctions.Stuff(c.CompanyName, 6, c.CompanyName.Length - 5, "... - Company");

                var caseIdx = 0;
                foreach (var result in query)
                {
                    Assert.Equal(expected[caseIdx], result);
                    caseIdx++;
                }

                Assert.Equal(6, caseIdx);
            }
        }

        [Fact]
        public void Verify_query_containing_StartsWith_works()
        {
            var expected =
                new string[]
                {
                    "La corne d'abondance",
                    "La maison d'Asie",
                    "Laughing Bacchus Wine Cellars",
                    "Lazy K Kountry Store"
                };

            using (var context = new NorthwindEntities())
            {
                var query = from c in context.Customers
                            where c.CompanyName.StartsWith("La")
                            select c;

                var caseIdx = 0;
                foreach (var customer in query)
                {
                    Assert.Equal(expected[caseIdx], customer.CompanyName);
                    caseIdx++;
                }

                Assert.Equal(4, caseIdx);
            }
        }

        [Fact]
        public void Verify_DbGeometry_can_be_materialized()
        {
            using (var context = new NorthwindEntities())
            {
                var order = context.Orders.OrderBy(o => o.OrderID).First();

                Assert.Equal(10248, order.OrderID);
                Assert.Equal("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))", order.ContainerSize.AsText());
            }
        }

        [Fact]
        public void Verify_DbGeography_instance_method_translated_correctly()
        {
            var seattleLocation = SpatialServices.Instance.GeographyFromText("POINT(-122.333056 47.609722)");

            var expectedResults = new string[]
                                      {
                                          "BOTTM",
                                          "LAUGB",
                                          "LONEP",
                                          "THEBI",
                                          "TRAIH",
                                          "WHITC",
                                      };

            using(var context = new NorthwindEntities())
            {
                var query = from c in context.Customers
                            where c.Location.Distance(seattleLocation) < 250000 // 250 km
                            select c;

                var caseIdx = 0;
                foreach(var customer in query)
                {
                    Assert.Equal(expectedResults[caseIdx++], customer.CustomerID);
                }
            }
        }

        [Fact]
        public void Verify_DbGeography_instance_property_translated_correctly()
        {
            var expectedResults = new string[]
                                      {
                                          "BOTTM",
                                          "LAUGB",
                                          "LONEP",
                                          "THEBI",
                                          "TRAIH",
                                          "WHITC",
                                      };

            using (var context = new NorthwindEntities())
            {
                var query = from c in context.Customers
                            where c.Location.Latitude < 0
                            select c;

                Assert.Equal(9, query.Count());
            }
        }


        [Fact]
        public void Verify_DbGeometry_instance_method_translated_correctly()
        {
            var containerSize =
                SpatialServices.Instance.GeometryFromText("POLYGON ((0 0, 9 0, 9 9, 0 9, 0 0))", 0);

            using(var context = new NorthwindEntities())
            {
                var query = from o in context.Orders
                            where o.ContainerSize.SpatialEquals(containerSize)
                            select o;

                Assert.Equal(73, query.Count());
            }
        }

        [Fact]
        public void Verify_DbGeometry_static_method_translated_correctly()
        {
            using (var context = new NorthwindEntities())
            {
                var query = from o in context.Orders
                            where o.ContainerSize.SpatialEquals(DbGeometry.FromText("POLYGON ((0 0, 9 0, 9 9, 0 9, 0 0))", 0))
                            select o;

                Assert.Equal(73, query.Count());
            }
        }
    }
}