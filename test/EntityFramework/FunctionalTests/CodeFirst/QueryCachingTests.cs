// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Data.Entity;
    using System.Linq;
    using Xunit;

    public class QueryCachingTests : FunctionalTestBase
    {
        public class Entity
        {
            public int Id { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
        }

        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer<Context>(null);
            }

            public DbSet<Entity> Entities { get; set; }
        }

        [Fact]
        public void QueryCache_is_not_hit_if_DateTime_constants_in_queries_differ_by_milliseconds()
        {
            using (var context = new Context())
            {
                var query1 = from e in context.Entities
                             where e.DateTime == new DateTime(2013, 6, 26, 1, 2, 3, 4)
                             select e.Id;

                var commandText1 = query1.ToString();

                var query2 = from e in context.Entities
                             where e.DateTime == new DateTime(2013, 6, 26, 1, 2, 3, 5)
                             select e.Id;

                var commandText2 = query2.ToString();

                Assert.NotEqual(commandText1, commandText2);
            }
        }

        [Fact]
        public void QueryCache_is_not_hit_if_DateTimeOffset_constants_in_queries_differ_by_milliseconds()
        {
            using (var context = new Context())
            {
                var query1 = from e in context.Entities
                             where e.DateTimeOffset == new DateTimeOffset(new DateTime(2013, 6, 26, 1, 2, 3, 4))
                             select e.Id;

                var commandText1 = query1.ToString();

                var query2 = from e in context.Entities
                             where e.DateTimeOffset == new DateTimeOffset(new DateTime(2013, 6, 26, 1, 2, 3, 5))
                             select e.Id;

                var commandText2 = query2.ToString();

                Assert.NotEqual(commandText1, commandText2);
            }
        }
    }
}
