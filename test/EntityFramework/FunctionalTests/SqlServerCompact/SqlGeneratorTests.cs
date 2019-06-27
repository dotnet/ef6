// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.SqlServerCompact
{
    using SimpleModel;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Query;
    using System.Linq;
    using Xunit;

    public class SqlGeneratorTests : FunctionalTestBase
    {
        [Fact]
        public void Large_union_all_should_not_give_query_nested_too_deeply()
        {
            var query = "{" + string.Join(", ", Enumerable.Range(1, 100)) + "}";

            using (var db = new SimpleModelContext(SimpleCeConnection<SimpleModelContext>()))
            {
                using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                {
                    VerifyAgainstBaselineResults(reader, Enumerable.Range(1, 100));
                }
            }
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<int> expectedResults)
        {
            VerifyAgainstBaselineResults(reader, expectedResults.Cast<object>());
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<object> expectedResults)
        {
            var actualResults = new List<object>();
            while (reader.Read())
            {
                actualResults.Add(reader.GetValue(0));
            }

            Assert.True(expectedResults.SequenceEqual(actualResults));
        }
    }
}

#endif
