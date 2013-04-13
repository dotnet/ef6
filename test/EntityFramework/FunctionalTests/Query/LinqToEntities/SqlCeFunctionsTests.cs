// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.SqlServerCompact;
    using System.Data.Entity.TestModels.ArubaCeModel;
    using System.Linq;
    using Xunit;

    public class SqlCeFunctionsTests : FunctionalTestBase
    {
        [Fact]
        public void SqlCeFunction_passing_function_as_argument_to_another_works()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.AllTypes.Select(a => SqlCeFunctions.Asin(SqlCeFunctions.Acos(a.c10_float)));
                Assert.Contains("ASIN", query.ToString().ToUpperInvariant());
                Assert.Contains("ACOS", query.ToString().ToUpperInvariant());
            }
        }

        [Fact]
        public void SqlCeFunctions_scalar_function_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query1 = context.AllTypes.Select(a => SqlCeFunctions.Acos(a.c7_decimal_28_4));
                var query2 = context.AllTypes.Select(a => SqlCeFunctions.Acos(a.c10_float));
                Assert.Contains("ACOS", query1.ToString().ToUpperInvariant());
                Assert.Contains("ACOS", query2.ToString().ToUpperInvariant());
            }
        }

        private ArubaCeContext GetArubaCeContext()
        {
            return new ArubaCeContext("Scenario_Use_SqlCe_AppConfig_connection_string");
        }

    }
}
