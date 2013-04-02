// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.   

namespace PlanCompilerTests
{
    using System.Data.Entity;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;

    /// <summary>   
    ///     Tests for GroupBy statements in Linq queries.   
    /// </summary>   
    public class LinqGroupByTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        static LinqGroupByTests()
        {
        }

        #endregion

        #region Tests for GroupBy that trigger an aggregate pushdown

        [Fact]
        private void GroupBy_aggregate_pushdown_single_key()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var groupByQuery = from workOrder in context.WorkOrders
                                   group new { workOrder.WorkOrderId, workOrder.Details } by workOrder.EmployeeId into ordersByEmployeeGroup
                                   select new
                                   {
                                       EmployeeId = ordersByEmployeeGroup.Key,
                                       OrderCount = ordersByEmployeeGroup.Count(),
                                       MaxOrderId = ordersByEmployeeGroup.Max(o => o.WorkOrderId)
                                   };
                var sql = groupByQuery.ToString();
                Assert.True(sql != null && sql.ToUpper().Contains("GROUP BY"));
            }
        }

        // Dev11 448362   
        [Fact]
        private void GroupBy_aggregate_pushdown_translates_NewRecordOp()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var groupByNewQuery = from workOrder in context.WorkOrders
                                      group new { workOrder.WorkOrderId, workOrder.Details } by new { workOrder.EmployeeId } into ordersByEmployeeGroup
                                      select new
                                      {
                                          ordersByEmployeeGroup.Key.EmployeeId,
                                          OrderCount = ordersByEmployeeGroup.Count(),
                                          MaxOrderId = ordersByEmployeeGroup.Max(o => o.WorkOrderId)
                                      };
                var sql = groupByNewQuery.ToString();
                Assert.True(sql != null && sql.ToUpper().Contains("GROUP BY"));
            }
        }

        #endregion
    }
}