// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

namespace System.Data.Entity.EntityClient
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Threading.Tasks;
    using SimpleModel;
    using Xunit;

    public class AsyncScenarios : FunctionalTestBase
    {
        [Fact]
        public void Async_query_returns_same_as_as_sync()
        {
            var query = @"
SELECT VALUE p 
FROM CodeFirstContainer.Products AS p
WHERE p.ID < 5
ORDER BY p.ID";
            var listSync = Query(SimpleModelEntityConnectionString, query);
            var listAsync = QueryAsync(SimpleModelEntityConnectionString, query).Result;

            Assert.Equal(listSync, listAsync, new ProductEqualityComparer());
        }

        [Fact]
        public void Async_scalar_query_returns_same_as_as_sync()
        {
            var query = @"
COUNT(
    SELECT VALUE p.ID 
    FROM CodeFirstContainer.Products AS p
    WHERE p.ID < 5)";
            var scalarSync = QueryScalar(SimpleModelEntityConnectionString, query);
            var scalarAsync = QueryScalarAsync(SimpleModelEntityConnectionString, query).Result;

            Assert.Equal(scalarSync, scalarAsync);
        }

        [Fact]
        public void Async_nonquery_returns_same_as_as_sync()
        {
            var query = @"SELECT VALUE p FROM CodeFirstContainer.Products AS p";
            var listSync = NonQuery(SimpleModelEntityConnectionString, query);
            var listAsync = NonQueryAsync(SimpleModelEntityConnectionString, query).Result;

            Assert.Equal(listSync, listAsync);
        }

        private async Task<List<Product>> QueryAsync(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            await connection.OpenAsync();
            var command = new EntityCommand(commandString, connection);
            var reader = command.ExecuteReaderAsync(CommandBehavior.SequentialAccess).Result;
            var list = new List<Product>();
            do
            {
                while (await reader.ReadAsync())
                {
                    var values = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(await reader.GetFieldValueAsync<object>(i));
                    }
                    list.Add(CreateProduct(values));
                }
            }
            while (await reader.NextResultAsync());

            connection.Close();

            return list;
        }

        private List<Product> Query(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            connection.Open();
            var command = new EntityCommand(commandString, connection);
            var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

            var list = new List<Product>();
            do
            {
                while (reader.Read())
                {
                    var values = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        values.Add(reader.GetFieldValue<object>(i));
                    }
                    list.Add(CreateProduct(values));
                }
            }
            while (reader.NextResult());

            connection.Close();

            return list;
        }

        private Product CreateProduct(List<object> values)
        {
            var product = new Product();
            product.Id = (int)values[0];
            product.CategoryId = (string)values[1];
            product.Name = (string)values[2];
            return product;
        }

        private async Task<object> QueryScalarAsync(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            await connection.OpenAsync();
            var command = new EntityCommand(commandString, connection);
            var result = await command.ExecuteScalarAsync();

            connection.Close();

            return result;
        }

        private object QueryScalar(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            connection.Open();
            var command = new EntityCommand(commandString, connection);
            var result = command.ExecuteScalar();

            connection.Close();

            return result;
        }

        private async Task<object> NonQueryAsync(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            await connection.OpenAsync();
            var command = new EntityCommand(commandString, connection);
            var result = await command.ExecuteNonQueryAsync();

            connection.Close();

            return result;
        }

        private object NonQuery(string connectionString, string commandString)
        {
            var connection = new EntityConnection(connectionString);
            connection.Open();
            var command = new EntityCommand(commandString, connection);
            var result = command.ExecuteNonQuery();

            connection.Close();

            return result;
        }

        private class ProductEqualityComparer : IEqualityComparer<Product>
        {
            public bool Equals(Product x, Product y)
            {

                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return x.Id == y.Id &&
                       x.Name == y.Name &&
                       x.CategoryId == y.CategoryId;
            }

            public int GetHashCode(Product obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}

#endif
