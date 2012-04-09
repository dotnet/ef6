using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data.Objects;
using NorthwindEFModel;
using System.Linq;
using SampleEntityFrameworkProvider;

namespace ConsoleTests
{
    class LinqToEntitiesTests
    {
        public static void RunTests()
        {
            LinqToEntitiesQuery_Parameterized();
            ProviderStoreFunctionQuery();
            QueryWithStartsWith();
        }

        #region Simple Linq Query
        static void LinqToEntitiesQuery_Parameterized()
        {
            Console.WriteLine("LinqToEntitiesQuery_Parameterized");

            using (NorthwindEntities context = new NorthwindEntities())
            {
                var query = from c in context.Customers where c.CustomerID == "ALFKI" select c;
                Console.WriteLine("  Query Results");
                foreach (Customer c in query)
                    Console.WriteLine("    Name: {0}, Location: {1}", c.CompanyName, c.Location);
            }

            Console.WriteLine();
        }
        #endregion

        #region Provider Store Function
        static void ProviderStoreFunctionQuery()
        {
            Console.WriteLine("ProviderStoreFunctionQuery");
            using (NorthwindEntities context = new NorthwindEntities())
            {
                var query =
                    from c in context.Customers
                    where c.Address.City == "London"
                    select SampleSqlFunctions.Stuff(c.CompanyName, 6, c.CompanyName.Length - 5, "... - Company");
                ExecuteQuery(query);
            }
        }
        #endregion

        #region Support for translating to Like
        static void QueryWithStartsWith()
        {
            Console.WriteLine("QueryWithStartsWith");
            using (NorthwindEntities context = new NorthwindEntities())
            {
                var query =
                    from c in context.Customers
                    where c.CompanyName.StartsWith("La")
                    select c;
                ExecuteQuery(query);
            }
        }
        #endregion

        #region Helper Methods
        private static void ExecuteQuery(IQueryable<Customer> query)
        {
            Console.WriteLine("-- generated SQL");
            Console.WriteLine(((ObjectQuery)query).ToTraceString());

            Console.WriteLine();
            Console.WriteLine("-- query results");
            foreach (Customer c in query)
                Console.WriteLine("    {0}", c.CompanyName);
            Console.WriteLine();
        }

        private static void ExecuteQuery(IQueryable<string> query)
        {
            Console.WriteLine("-- generated SQL");
            Console.WriteLine(((ObjectQuery)query).ToTraceString());

            Console.WriteLine();
            Console.WriteLine("-- query results");
            foreach (string c in query)
                Console.WriteLine("    {0}", c);
            Console.WriteLine();
        }
        #endregion

    }
}